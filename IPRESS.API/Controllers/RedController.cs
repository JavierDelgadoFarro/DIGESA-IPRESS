using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPRESS.Infrastructure.Persistence;
using IPRESS.Domain.Entities;

namespace IPRESS.API.Controllers
{
    public class ExcelFileRequestRed { public string FileBase64 { get; set; } = ""; }

    /// <summary>Body para PUT: solo campos editables (evita 400 por Diresa/Departamento/Provincia/Distrito en JSON).</summary>
    public class PutRedRequest
    {
        public int IdDiresa { get; set; }
        public int Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Ubigeo { get; set; }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RedController : ControllerBase
    {
        private const int MaxFileSizeBytes = 8 * 1024 * 1024;
        private readonly IpressDbContext _context;

        public RedController(IpressDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] int? idDiresa)
        {
            var q = _context.Redes.AsNoTracking()
                .Include(r => r.Diresa)
                .Include(r => r.Distrito).ThenInclude(d => d!.Provincia).ThenInclude(p => p!.Departamento)
                .Where(r => r.Activo);
            if (idDiresa.HasValue) q = q.Where(r => r.IdDiresa == idDiresa.Value);
            return Ok(await q.OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    r.IdRed,
                    r.IdDiresa,
                    Diresa = r.Diresa != null ? r.Diresa.Nombre : "",
                    r.Codigo,
                    r.Nombre,
                    Departamento = r.Distrito != null && r.Distrito.Provincia != null && r.Distrito.Provincia.Departamento != null ? r.Distrito.Provincia.Departamento.Nombre : null,
                    Provincia = r.Distrito != null && r.Distrito.Provincia != null ? r.Distrito.Provincia.Nombre : null,
                    Distrito = r.Distrito != null ? r.Distrito.Nombre : null,
                    r.Ubigeo
                }).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IpressRed>> Get(int id)
        {
            var r = await _context.Redes.Include(x => x.Diresa).FirstOrDefaultAsync(x => x.IdRed == id);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPost]
        public async Task<ActionResult<IpressRed>> Post([FromBody] PutRedRequest r)
        {
            if (await _context.Redes.AnyAsync(x => x.Codigo == r.Codigo && x.IdDiresa == r.IdDiresa))
                return BadRequest(new { message = "El código ya existe en esta Diresa." });
            var entity = new IpressRed
            {
                IdDiresa = r.IdDiresa,
                Codigo = r.Codigo,
                Nombre = r.Nombre ?? "",
                Ubigeo = string.IsNullOrWhiteSpace(r.Ubigeo) ? null : (r.Ubigeo.Trim().Length == 6 ? r.Ubigeo.Trim() : null),
                Activo = true
            };
            _context.Redes.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.IdRed }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PutRedRequest r)
        {
            var exist = await _context.Redes.FindAsync(id);
            if (exist == null) return NotFound();
            exist.IdDiresa = r.IdDiresa;
            exist.Codigo = r.Codigo;
            exist.Nombre = r.Nombre ?? "";
            exist.Ubigeo = string.IsNullOrWhiteSpace(r.Ubigeo) ? null : (r.Ubigeo.Trim().Length == 6 ? r.Ubigeo.Trim() : null);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Redes.FindAsync(id);
            if (entity == null) return NotFound();
            entity.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Vista previa del Excel en JSON (base64). Mismo flujo que Diresas: validar y mostrar todas las filas con estado.</summary>
        [HttpPost("preview-json")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> PreviewJson([FromBody] ExcelFileRequestRed? body, CancellationToken cancellationToken = default)
        {
            try
            {
                if (body == null || string.IsNullOrWhiteSpace(body.FileBase64))
                    return BadRequest(new { message = "Envíe el cuerpo JSON con la propiedad 'fileBase64'." });
                byte[] fileBytes;
                try { fileBytes = Convert.FromBase64String(body.FileBase64.Trim()); }
                catch (FormatException) { return BadRequest(new { message = "El valor de 'fileBase64' no es base64 válido." }); }
                if (fileBytes.Length == 0 || fileBytes.Length > MaxFileSizeBytes)
                    return BadRequest(new { message = "El archivo no debe superar 8 MB." });

                var diresasPorCodigo = await _context.Diresas.Where(d => d.Activo).ToDictionaryAsync(d => d.Codigo, d => d.IdDiresa, cancellationToken);
                var parejasExistentes = await _context.Redes.Where(r => r.Activo).Select(r => new { r.IdDiresa, r.Codigo }).ToListAsync(cancellationToken);
                var setExistentes = new HashSet<(int IdDiresa, int Codigo)>(parejasExistentes.Select(p => (p.IdDiresa, p.Codigo)));

                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                var filas = new List<object>();
                var errores = new List<string>();
                var duplicados = 0;
                var tieneErrores = false;
                using (var stream = new MemoryStream(fileBytes))
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Red", StringComparison.OrdinalIgnoreCase)) ?? package.Workbook.Worksheets[0];
                    var rowCount = ws.Dimension?.Rows ?? 0;
                    if (rowCount < 2) return Ok(new { filas = new List<object>(), errores = new[] { "No hay filas de datos." }, duplicados = 0, tieneErrores = true });
                    for (var row = 2; row <= rowCount; row++)
                    {
                        // Formato nuevo: A=Codigo, B=Nombre, C=Diresa, D=CodigoDiresa, ... J=Ubigeo. Compatible con formato antiguo: A=CodigoDiresa, B=Codigo, C=Nombre
                        var col1 = ws.Cells[row, 1].Text?.Trim();
                        var col2 = ws.Cells[row, 2].Text?.Trim();
                        var col3 = ws.Cells[row, 3].Text?.Trim();
                        var col4 = ws.Cells[row, 4].Text?.Trim();
                        var col10 = ws.Dimension?.Columns >= 10 ? ws.Cells[row, 10].Text?.Trim() : null;
                        string codigo, nombre, codigoDiresa;
                        if (ws.Dimension?.Columns >= 10 && int.TryParse(col4, out _))
                        {
                            codigo = col1 ?? "";
                            nombre = col2 ?? "";
                            codigoDiresa = col4 ?? "";
                        }
                        else
                        {
                            codigoDiresa = col1 ?? "";
                            codigo = col2 ?? "";
                            nombre = col3 ?? "";
                        }
                        var ubigeo = (col10?.Length == 6 ? col10 : null) ?? "";
                        if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(nombre)) continue;
                        string estado; string? mensaje = null;
                        if (string.IsNullOrWhiteSpace(codigoDiresa)) { estado = "error"; mensaje = "Diresa vacía"; tieneErrores = true; }
                        else if (string.IsNullOrWhiteSpace(codigo)) { estado = "error"; mensaje = "Código vacío"; tieneErrores = true; }
                        else if (!int.TryParse(codigo, out var codigoInt)) { estado = "error"; mensaje = "Código debe ser numérico"; tieneErrores = true; }
                        else if (string.IsNullOrWhiteSpace(nombre)) { estado = "error"; mensaje = "Nombre vacío"; tieneErrores = true; }
                        else if (!int.TryParse(codigoDiresa, out var codigoDiresaInt) || !diresasPorCodigo.TryGetValue(codigoDiresaInt, out var idDiresa)) { estado = "error"; mensaje = "Diresa no encontrada"; tieneErrores = true; }
                        else if (setExistentes.Contains((idDiresa, codigoInt))) { estado = "duplicado"; mensaje = "Registro duplicado (código ya existe en esta Diresa)"; duplicados++; }
                        else { estado = "ok"; setExistentes.Add((idDiresa, codigoInt)); }
                        filas.Add(new { codigo = codigo ?? "", nombre = nombre ?? "", ubigeo, estado, mensaje });
                    }
                }
                if (errores.Count > 0) tieneErrores = true;
                return Ok(new { filas, errores, duplicados, tieneErrores });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al procesar el Excel: " + (ex.Message ?? "error desconocido") }); }
        }

        /// <summary>Importar Excel en JSON (base64).</summary>
        [HttpPost("importar-json")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> ImportarJson([FromBody] ExcelFileRequestRed? body, CancellationToken cancellationToken = default)
        {
            try
            {
                if (body == null || string.IsNullOrWhiteSpace(body.FileBase64))
                    return BadRequest(new { message = "Envíe el cuerpo JSON con la propiedad 'fileBase64'." });
                byte[] fileBytes;
                try { fileBytes = Convert.FromBase64String(body.FileBase64.Trim()); }
                catch (FormatException) { return BadRequest(new { message = "El valor de 'fileBase64' no es base64 válido." }); }
                if (fileBytes.Length == 0 || fileBytes.Length > MaxFileSizeBytes)
                    return BadRequest(new { message = "El archivo no debe superar 8 MB." });

                var diresasPorCodigo = await _context.Diresas.Where(d => d.Activo).ToDictionaryAsync(d => d.Codigo, d => d.IdDiresa, cancellationToken);
                var redesExistentes = await _context.Redes.Where(r => r.Activo).ToListAsync(cancellationToken);
                var dictExistentes = redesExistentes.GroupBy(r => (r.IdDiresa, r.Codigo)).ToDictionary(g => g.Key, g => g.First());
                var setExistentes = new HashSet<(int IdDiresa, int Codigo)>(dictExistentes.Keys);
                var insertados = 0; var duplicados = 0; var errores = new List<string>();
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var stream = new MemoryStream(fileBytes))
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("Red", StringComparison.OrdinalIgnoreCase)) ?? package.Workbook.Worksheets[0];
                    var rowCount = ws.Dimension?.Rows ?? 0;
                    if (rowCount < 2) return Ok(new { insertados = 0, duplicados = 0, errores = new[] { "No hay filas de datos." } });
                    for (var row = 2; row <= rowCount; row++)
                    {
                        var col1 = ws.Cells[row, 1].Text?.Trim();
                        var col2 = ws.Cells[row, 2].Text?.Trim();
                        var col3 = ws.Cells[row, 3].Text?.Trim();
                        var col4 = ws.Cells[row, 4].Text?.Trim();
                        var col10 = ws.Dimension?.Columns >= 10 ? ws.Cells[row, 10].Text?.Trim() : null;
                        string codigo, nombre, codigoDiresa;
                        if (ws.Dimension?.Columns >= 10 && int.TryParse(col4, out _))
                        {
                            codigo = col1 ?? "";
                            nombre = col2 ?? "";
                            codigoDiresa = col4 ?? "";
                        }
                        else
                        {
                            codigoDiresa = col1 ?? "";
                            codigo = col2 ?? "";
                            nombre = col3 ?? "";
                        }
                        var ubigeo = col10?.Length == 6 ? col10 : null;
                        if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(nombre)) continue;
                        if (string.IsNullOrWhiteSpace(codigoDiresa)) { errores.Add($"Fila {row}: Diresa vacía"); continue; }
                        if (string.IsNullOrWhiteSpace(codigo)) { errores.Add($"Fila {row}: Código vacío"); continue; }
                        if (!int.TryParse(codigo, out var codigoInt)) { errores.Add($"Fila {row}: Código debe ser numérico"); continue; }
                        if (string.IsNullOrWhiteSpace(nombre)) { errores.Add($"Fila {row}: Nombre vacío"); continue; }
                        if (!int.TryParse(codigoDiresa, out var codigoDiresaInt) || !diresasPorCodigo.TryGetValue(codigoDiresaInt, out var idDiresa)) { errores.Add($"Fila {row}: Diresa no encontrada"); continue; }
                        if (setExistentes.Contains((idDiresa, codigoInt))) { duplicados++; continue; }
                        var existente = dictExistentes.TryGetValue((idDiresa, codigoInt), out var red) ? red : null;
                        if (existente != null)
                        {
                            existente.Nombre = nombre!;
                            existente.Ubigeo = ubigeo;
                            setExistentes.Add((idDiresa, codigoInt));
                            insertados++;
                        }
                        else
                        {
                            _context.Redes.Add(new IpressRed { IdDiresa = idDiresa, Codigo = codigoInt, Nombre = nombre!, Ubigeo = ubigeo, Activo = true });
                            setExistentes.Add((idDiresa, codigoInt));
                            insertados++;
                        }
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return Ok(new { insertados, duplicados, errores });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al importar: " + (ex.Message ?? "error desconocido") }); }
        }

        /// <summary>Formato de carga masiva: Codigo, Nombre, Diresa (lista), Departamento/Provincia/Distrito (listas en cascada), Ubigeo por fórmula.</summary>
        [HttpGet("formato-descarga")]
        public async Task<IActionResult> DescargarFormato()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var diresas = await _context.Diresas.AsNoTracking().Where(d => d.Activo).OrderBy(d => d.Nombre).Select(d => new { d.Codigo, d.Nombre }).ToListAsync();
                var departamentos = await _context.Departamentos.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new { x.Codigo, x.Nombre }).ToListAsync();
                var provincias = await _context.Provincias.AsNoTracking().OrderBy(x => x.CodigoDepartamento).ThenBy(x => x.Nombre).Select(x => new { x.CodigoDepartamento, x.Nombre, x.Codigo }).ToListAsync();
                var distritos = await _context.Distritos.AsNoTracking().OrderBy(x => x.CodigoProvincia).ThenBy(x => x.Nombre).Select(x => new { x.CodigoProvincia, x.Nombre, x.Ubigeo }).ToListAsync();

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        // Hoja ListaDiresas (Nombre, Codigo) para dropdown y VLOOKUP
                        var wsDir = package.Workbook.Worksheets.Add("ListaDiresas");
                        wsDir.Cells[1, 1].Value = "Nombre";
                        wsDir.Cells[1, 2].Value = "Codigo";
                        int rowDir = 2;
                        foreach (var d in diresas)
                        {
                            wsDir.Cells[rowDir, 1].Value = d.Nombre;
                            wsDir.Cells[rowDir, 2].Value = d.Codigo;
                            rowDir++;
                        }
                        int dirRows = rowDir - 1;

                        // Hoja Departamentos
                        var wsDep = package.Workbook.Worksheets.Add("Departamentos");
                        wsDep.Cells[1, 1].Value = "Nombre";
                        wsDep.Cells[1, 2].Value = "Codigo";
                        int rowDep = 2;
                        foreach (var d in departamentos)
                        {
                            wsDep.Cells[rowDep, 1].Value = d.Nombre;
                            wsDep.Cells[rowDep, 2].Value = d.Codigo;
                            rowDep++;
                        }
                        int depRows = rowDep - 1;

                        // Hoja Provincias
                        var wsProv = package.Workbook.Worksheets.Add("Provincias");
                        wsProv.Cells[1, 1].Value = "CodigoDepartamento";
                        wsProv.Cells[1, 2].Value = "Nombre";
                        wsProv.Cells[1, 3].Value = "Codigo";
                        int rowProv = 2;
                        foreach (var p in provincias)
                        {
                            wsProv.Cells[rowProv, 1].Value = p.CodigoDepartamento;
                            wsProv.Cells[rowProv, 2].Value = p.Nombre;
                            wsProv.Cells[rowProv, 3].Value = p.Codigo;
                            rowProv++;
                        }

                        // Hoja Distritos
                        var wsDist = package.Workbook.Worksheets.Add("Distritos");
                        wsDist.Cells[1, 1].Value = "CodigoProvincia";
                        wsDist.Cells[1, 2].Value = "Nombre";
                        wsDist.Cells[1, 3].Value = "Ubigeo";
                        int rowDist = 2;
                        foreach (var d in distritos)
                        {
                            wsDist.Cells[rowDist, 1].Value = d.CodigoProvincia;
                            wsDist.Cells[rowDist, 2].Value = d.Nombre;
                            wsDist.Cells[rowDist, 3].Value = d.Ubigeo;
                            rowDist++;
                        }

                        // Rangos nombrados Prov y Dist (como en formato Diresas)
                        int provRow = 2;
                        foreach (var grp in provincias.GroupBy(p => p.CodigoDepartamento).OrderBy(g => g.Key))
                        {
                            int start = provRow;
                            provRow += grp.Count();
                            int end = provRow - 1;
                            string safeName = "Prov_" + (grp.Key ?? "").Trim();
                            if (end >= start && !string.IsNullOrEmpty(safeName))
                            {
                                package.Workbook.Names.Add(safeName, wsProv.Cells[start, 2, end, 2]);
                                package.Workbook.Names.Add(safeName + "_Data", wsProv.Cells[start, 2, end, 3]);
                            }
                        }
                        int distRow = 2;
                        foreach (var grp in distritos.GroupBy(d => d.CodigoProvincia).OrderBy(g => g.Key))
                        {
                            int start = distRow;
                            distRow += grp.Count();
                            int end = distRow - 1;
                            string safeName = "Dist_" + (grp.Key ?? "").Trim();
                            if (end >= start && !string.IsNullOrEmpty(safeName))
                            {
                                package.Workbook.Names.Add(safeName, wsDist.Cells[start, 2, end, 2]);
                                package.Workbook.Names.Add(safeName + "_Data", wsDist.Cells[start, 2, end, 3]);
                            }
                        }

                        // Hoja Red: A=Codigo, B=Nombre, C=Diresa, D=CodigoDiresa(hidden), E=Departamento, F=CodigoDep(hidden), G=Provincia, H=CodigoProv(hidden), I=Distrito, J=Ubigeo
                        var ws = package.Workbook.Worksheets.Add("Red");
                        ws.Cells[1, 1].Value = "Codigo";
                        ws.Cells[1, 2].Value = "Nombre";
                        ws.Cells[1, 3].Value = "Diresa";
                        ws.Cells[1, 4].Value = "CodigoDiresa";
                        ws.Cells[1, 5].Value = "Departamento";
                        ws.Cells[1, 6].Value = "CodigoDep";
                        ws.Cells[1, 7].Value = "Provincia";
                        ws.Cells[1, 8].Value = "CodigoProv";
                        ws.Cells[1, 9].Value = "Distrito";
                        ws.Cells[1, 10].Value = "Ubigeo";
                        ws.Column(1).Style.Numberformat.Format = "@";
                        ws.Column(10).Style.Numberformat.Format = "@";
                        const int dataRows = 500;
                        string dirRange = dirRows > 0 ? $"ListaDiresas!$A$2:$A${1 + dirRows}" : "ListaDiresas!$A$2:$A$2";
                        string depRange = depRows > 0 ? $"Departamentos!$A$2:$A${1 + depRows}" : "Departamentos!$A$2:$A$2";

                        for (int r = 2; r <= dataRows + 1; r++)
                        {
                            ws.Cells[r, 4].Formula = $"IF(C{r}=\"\",\"\",VLOOKUP(C{r},ListaDiresas!$A:$B,2,0))";
                            ws.Cells[r, 6].Formula = $"IF(E{r}=\"\",\"\",VLOOKUP(E{r},Departamentos!$A:$B,2,0))";
                            ws.Cells[r, 8].Formula = $"IF(G{r}=\"\",\"\",VLOOKUP(G{r},INDIRECT(\"Prov_\"&F{r}&\"_Data\"),2,0))";
                            ws.Cells[r, 10].Formula = $"IF(I{r}=\"\",\"\",VLOOKUP(I{r},INDIRECT(\"Dist_\"&H{r}&\"_Data\"),2,0))";
                        }

                        var valDir = ws.DataValidations.AddListValidation($"C2:C{dataRows + 1}");
                        valDir.Formula.ExcelFormula = dirRange;
                        valDir.ShowErrorMessage = true;
                        valDir.ErrorTitle = "Diresa";
                        valDir.Error = "Seleccione un valor de la lista.";
                        // Departamento, Provincia y Distrito opcionales: permitir celda en blanco
                        var valDep = ws.DataValidations.AddListValidation($"E2:E{dataRows + 1}");
                        valDep.Formula.ExcelFormula = depRange;
                        valDep.AllowBlank = true;
                        valDep.ShowErrorMessage = true;
                        valDep.ErrorTitle = "Departamento";
                        valDep.Error = "Seleccione un valor (opcional).";
                        var valProv = ws.DataValidations.AddListValidation($"G2:G{dataRows + 1}");
                        valProv.Formula.ExcelFormula = "INDIRECT(\"Prov_\"&F2)";
                        valProv.AllowBlank = true;
                        valProv.ShowErrorMessage = true;
                        var valDist = ws.DataValidations.AddListValidation($"I2:I{dataRows + 1}");
                        valDist.Formula.ExcelFormula = "INDIRECT(\"Dist_\"&H2)";
                        valDist.AllowBlank = true;
                        valDist.ShowErrorMessage = true;

                        ws.Column(4).Hidden = true;
                        ws.Column(6).Hidden = true;
                        ws.Column(8).Hidden = true;

                        wsDir.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        wsDep.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        wsProv.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        wsDist.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        package.Workbook.View.ActiveTab = 4; // Red es la 5ª hoja (0-based: 4)

                        package.Save();
                    }
                    bytes = stream.ToArray();
                }
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Formato_Red.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el formato.", detail = ex.Message });
            }
        }

        /// <summary>Exporta las Redes registradas (con Diresa y ubicación) a Excel.</summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var data = await _context.Redes.AsNoTracking()
                    .Include(r => r.Diresa)
                    .Include(r => r.Distrito).ThenInclude(d => d!.Provincia).ThenInclude(p => p!.Departamento)
                    .Where(r => r.Activo)
                    .OrderBy(r => r.Nombre)
                    .ToListAsync();

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var ws = package.Workbook.Worksheets.Add("Red");
                        ws.Cells[1, 1].Value = "Diresa";
                        ws.Cells[1, 2].Value = "Codigo";
                        ws.Cells[1, 3].Value = "Microred";
                        ws.Cells[1, 4].Value = "Departamento";
                        ws.Cells[1, 5].Value = "Provincia";
                        ws.Cells[1, 6].Value = "Distrito";
                        ws.Cells[1, 7].Value = "Ubigeo";

                        var row = 2;
                        foreach (var r in data)
                        {
                            ws.Cells[row, 1].Value = r.Diresa?.Nombre;
                            ws.Cells[row, 2].Value = r.Codigo;
                            ws.Cells[row, 3].Value = r.Nombre;
                            var dep = r.Distrito?.Provincia?.Departamento?.Nombre;
                            var prov = r.Distrito?.Provincia?.Nombre;
                            var dist = r.Distrito?.Nombre;
                            ws.Cells[row, 4].Value = dep;
                            ws.Cells[row, 5].Value = prov;
                            ws.Cells[row, 6].Value = dist;
                            ws.Cells[row, 7].Value = r.Ubigeo;
                            row++;
                        }

                        ws.Column(2).Style.Numberformat.Format = "@";
                        ws.Column(7).Style.Numberformat.Format = "@";
                        package.Save();
                    }
                    bytes = stream.ToArray();
                }

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Redes.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al exportar las Redes.", detail = ex.Message });
            }
        }
    }
}
