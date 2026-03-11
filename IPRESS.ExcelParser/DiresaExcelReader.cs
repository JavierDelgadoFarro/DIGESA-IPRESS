using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;

namespace IPRESS.ExcelParser;

/// <summary>
/// Lee archivos Excel de Diresas (.xlsx) como ZIP+XML (solo BCL).
/// Se usa desde un proceso auxiliar para no cerrar la API si el parsing falla.
/// </summary>
public static class DiresaExcelReader
{
    private const string NsMain = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string NsOfficeRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const int MaxRows = 5000;

    /// <summary>Resultado para serializar a JSON (salida del proceso helper).</summary>
    public class RawResult
    {
        public bool Ok { get; set; }
        public List<RowDto> Rows { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class RowDto
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Departamento { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string Distrito { get; set; } = "";
        public string Ubigeo { get; set; } = "";
    }

    /// <summary>Lee el Excel y devuelve filas crudas + errores de parsing. Para uso en proceso auxiliar.</summary>
    public static RawResult GetRawRows(byte[] fileBytes)
    {
        var errores = new List<string>();
        var result = new RawResult();
        try
        {
            var rows = ReadRowsFromXlsx(fileBytes, errores);
            result.Errores = errores;
            if (rows == null)
            {
                result.Ok = false;
                result.ErrorMessage = errores.Count > 0 ? errores[0] : "No se pudo leer el archivo Excel.";
                return result;
            }
            result.Rows = rows.Select(r => new RowDto { Codigo = r.Codigo, Nombre = r.Nombre, Departamento = r.Departamento, Provincia = r.Provincia, Distrito = r.Distrito, Ubigeo = r.Ubigeo }).ToList();
            result.Ok = true;
            if (rows.Count == 0)
                result.Errores.Add("No hay filas de datos. Use un archivo con hoja 'Diresas' y columnas Codigo, Nombre, Ubigeo.");
            return result;
        }
        catch (Exception ex)
        {
            result.Ok = false;
            result.ErrorMessage = ex.Message ?? "Error al procesar el Excel.";
            result.Errores = errores;
            return result;
        }
    }

    /// <summary>Serializa el resultado a JSON en una sola línea (stdout del helper).</summary>
    public static string ToJson(RawResult result)
    {
        return JsonSerializer.Serialize(result);
    }

    private static List<(string Codigo, string Nombre, string Departamento, string Provincia, string Distrito, string Ubigeo)>? ReadRowsFromXlsx(byte[] fileBytes, List<string> errores)
    {
        try
        {
            ZipArchive? zip = null;
            try
            {
                var stream = new MemoryStream(fileBytes, writable: false);
                zip = new ZipArchive(stream, ZipArchiveMode.Read);
            }
            catch (InvalidDataException)
            {
                zip?.Dispose();
                errores.Add("El archivo no es un Excel .xlsx válido (ZIP corrupto o no es ZIP).");
                return null;
            }
            catch (Exception ex)
            {
                zip?.Dispose();
                errores.Add("No se pudo abrir el archivo como ZIP: " + (ex.Message ?? "error desconocido"));
                return null;
            }

            using (zip)
            {
                var workbookEntry = zip.GetEntry("xl/workbook.xml") ?? zip.GetEntry("xl/Workbook.xml");
                if (workbookEntry == null) { errores.Add("El archivo no parece un Excel .xlsx válido (falta workbook)."); return null; }

                XDocument workbookDoc;
                using (var ws = workbookEntry.Open())
                    workbookDoc = XDocument.Load(ws);

                XNamespace main = NsMain;
                XNamespace r = NsOfficeRel;
                var sheets = workbookDoc.Root?.Elements(main + "sheets")?.Elements(main + "sheet").ToList() ?? new List<XElement>();
                var sheetEl = sheets.FirstOrDefault(s =>
                    string.Equals((string?)s.Attribute("name"), "Diresas", StringComparison.OrdinalIgnoreCase));
                if (sheetEl == null)
                    sheetEl = sheets.FirstOrDefault();
                if (sheetEl == null) { errores.Add("No se encontró ninguna hoja en el libro."); return null; }

                var rId = (string?)sheetEl.Attribute(r + "id");
                if (string.IsNullOrEmpty(rId))
                    rId = sheetEl.Attributes().FirstOrDefault(a => a.Name.LocalName == "id")?.Value;
                if (string.IsNullOrEmpty(rId)) { errores.Add("No se pudo identificar la hoja."); return null; }

                var relsEntry = zip.GetEntry("xl/_rels/workbook.xml.rels") ?? zip.GetEntry("xl/_rels/Workbook.xml.rels");
                string? sheetPath = null;
                if (relsEntry != null)
                {
                    using var relsStream = relsEntry.Open();
                    var relsDoc = XDocument.Load(relsStream);
                    XNamespace relNs = "http://schemas.openxmlformats.org/package/2006/relationships";
                    var rel = relsDoc.Root?.Elements(relNs + "Relationship")
                        .FirstOrDefault(x => (string?)x.Attribute("Id") == rId);
                    var target = (string?)rel?.Attribute("Target");
                    if (!string.IsNullOrEmpty(target))
                        sheetPath = "xl/" + target.Replace("\\", "/").TrimStart('/');
                }
                if (string.IsNullOrEmpty(sheetPath)) sheetPath = "xl/worksheets/sheet1.xml";

                var sheetEntry = zip.GetEntry(sheetPath) ?? zip.GetEntry(sheetPath.Replace("xl/", "xl/").ToLowerInvariant());
                if (sheetEntry == null) { errores.Add("No se encontró la hoja de datos."); return null; }

                List<string>? sharedStrings = null;
                var ssEntry = zip.GetEntry("xl/sharedStrings.xml");
                if (ssEntry != null)
                {
                    try
                    {
                        using var ssStream = ssEntry.Open();
                        var ssDoc = XDocument.Load(ssStream);
                        XNamespace ssMain = NsMain;
                        sharedStrings = ssDoc.Root?.Elements(ssMain + "si")
                            .Select(si =>
                            {
                                var t = si.Element(ssMain + "t");
                                if (t != null) return (string?)t ?? "";
                                return string.Concat(si.Elements(ssMain + "r").Select(rel => (string?)rel.Element(ssMain + "t") ?? ""));
                            })
                            .ToList() ?? new List<string>();
                    }
                    catch { /* ignorar */ }
                }

                XDocument sheetDoc;
                using (var sheetStream = sheetEntry.Open())
                    sheetDoc = XDocument.Load(sheetStream);

                var rows = new List<(string Codigo, string Nombre, string Departamento, string Provincia, string Distrito, string Ubigeo)>();
                var sheetData = sheetDoc.Root?.Element(main + "sheetData");
                if (sheetData == null) return rows;

                foreach (var rowEl in sheetData.Elements(main + "row"))
                {
                    var rowNumAttr = (string?)rowEl.Attribute("r");
                    if (!int.TryParse(rowNumAttr, out var rowNum) || rowNum < 2) continue;
                    var cells = new Dictionary<int, string>();
                    foreach (var c in rowEl.Elements(main + "c"))
                    {
                        var refAttr = (string?)c.Attribute("r");
                        if (string.IsNullOrEmpty(refAttr)) continue;
                        var col = GetColumnIndex(refAttr);
                        if (col < 1 || col > 8) continue;
                        var vEl = c.Element(main + "v");
                        var isEl = c.Element(main + "is");
                        var t = (string?)c.Attribute("t");
                        var val = "";
                        if (vEl != null)
                        {
                            if (t == "s" && sharedStrings != null && int.TryParse((string?)vEl, out var idx) && idx >= 0 && idx < sharedStrings.Count)
                                val = sharedStrings[idx] ?? "";
                            else
                                val = (string?)vEl ?? "";
                        }
                        else if (isEl != null)
                            val = (string?)isEl.Element(main + "t") ?? "";
                        if (!string.IsNullOrEmpty(val)) cells[col] = val.Trim();
                    }
                    var codigo = cells.TryGetValue(1, out var c1) ? c1 : "";
                    var nombre = cells.TryGetValue(2, out var c2) ? c2 : "";
                    // Formato 8 columnas: C=3 Dep, E=5 Prov, G=7 Dist, H=8 Ubigeo. Si hay dato en 3, 5, 7 u 8 es formato con listas.
                    var hasFormatoListas = cells.ContainsKey(3) || cells.ContainsKey(5) || cells.ContainsKey(7) || cells.ContainsKey(8);
                    var departamento = hasFormatoListas && cells.TryGetValue(3, out var c3) ? c3 : "";
                    var provincia = hasFormatoListas && cells.TryGetValue(5, out var c5) ? c5 : "";
                    var distrito = hasFormatoListas && cells.TryGetValue(7, out var c7) ? c7 : "";
                    // En formato listas el ubigeo solo viene en columna 8 (6 dígitos). En formato simple: col 8, 6 o 3.
                    var ubigeo = hasFormatoListas
                        ? (cells.TryGetValue(8, out var u8) ? u8 : "")
                        : (cells.TryGetValue(8, out var u8b) ? u8b : (cells.TryGetValue(6, out var u6) ? u6 : (cells.TryGetValue(3, out var u3) ? u3 : "")));
                    rows.Add((codigo, nombre, departamento, provincia, distrito, ubigeo));
                    if (rows.Count >= MaxRows) break;
                }
                return rows;
            }
        }
        catch (InvalidDataException) { errores.Add("El archivo no es un Excel .xlsx válido (ZIP corrupto o no es ZIP)."); return null; }
        catch (Exception ex) { errores.Add("Error al leer el Excel: " + (ex.Message ?? "error desconocido")); return null; }
    }

    private static int GetColumnIndex(string cellRef)
    {
        int i = 0;
        while (i < cellRef.Length && char.IsLetter(cellRef[i])) i++;
        if (i == 0) return 0;
        var col = 0;
        for (int j = 0; j < i; j++)
            col = col * 26 + (char.ToUpperInvariant(cellRef[j]) - 'A' + 1);
        return col;
    }
}
