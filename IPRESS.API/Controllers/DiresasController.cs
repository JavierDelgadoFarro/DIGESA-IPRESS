using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPRESS.Infrastructure.Persistence;
using IPRESS.Domain.Entities;
using IPRESS.API.Services;

namespace IPRESS.API.Controllers
{
    /// <summary>Body para preview-json e importar-json: evita multipart y previene cierre del proceso.</summary>
    public class ExcelFileRequest
    {
        public string FileBase64 { get; set; } = "";
    }

    /// <summary>Body para PUT: solo campos editables (evita 400 por Distrito/Departamento/Provincia en JSON).</summary>
    public class PutDiresaRequest
    {
        public int Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Ubigeo { get; set; }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DiresasController : ControllerBase
    {
        private const int MaxFileSizeBytes = 8 * 1024 * 1024;

        private readonly IpressDbContext _context;

        public DiresasController(IpressDbContext context) => _context = context;

        /// <summary>Lista departamentos del Perú (para combos).</summary>
        [HttpGet("departamentos")]
        public async Task<ActionResult<IEnumerable<object>>> GetDepartamentos() =>
            Ok(await _context.Departamentos.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new { x.Codigo, x.Nombre }).ToListAsync());

        /// <summary>Lista provincias por código de departamento (2 caracteres).</summary>
        [HttpGet("provincias")]
        public async Task<ActionResult<IEnumerable<object>>> GetProvincias([FromQuery] string? codigoDepartamento) =>
            Ok(await _context.Provincias.AsNoTracking()
                .Where(x => string.IsNullOrEmpty(codigoDepartamento) || x.CodigoDepartamento == codigoDepartamento)
                .OrderBy(x => x.Nombre).Select(x => new { x.Codigo, x.Nombre, x.CodigoDepartamento }).ToListAsync());

        /// <summary>Lista distritos por código de provincia (4 caracteres) o por ubigeo parcial.</summary>
        [HttpGet("distritos")]
        public async Task<ActionResult<IEnumerable<object>>> GetDistritos([FromQuery] string? codigoProvincia) =>
            Ok(await _context.Distritos.AsNoTracking()
                .Where(x => string.IsNullOrEmpty(codigoProvincia) || x.CodigoProvincia == codigoProvincia)
                .OrderBy(x => x.Nombre).Select(x => new { x.Ubigeo, x.Nombre, x.CodigoProvincia }).ToListAsync());

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get()
        {
            var diresas = await _context.Diresas.AsNoTracking()
                .Include(d => d.Distrito).ThenInclude(x => x!.Provincia).ThenInclude(p => p!.Departamento)
                .Where(d => d.Activo).OrderBy(d => d.Nombre).ToListAsync();

            // Cargar solo la geografía que usan las diresas (evita cargar ~2000 distritos)
            var ubigeo2 = diresas.Where(d => d.Ubigeo?.Length == 2).Select(d => d.Ubigeo!).Distinct().ToList();
            var ubigeo4 = diresas.Where(d => d.Ubigeo?.Length == 4).Select(d => d.Ubigeo!).Distinct().ToList();
            var ubigeo6 = diresas.Where(d => d.Ubigeo?.Length == 6).Select(d => d.Ubigeo!).Distinct().ToList();

            var departamentos = ubigeo2.Count > 0
                ? await _context.Departamentos.AsNoTracking().Where(x => ubigeo2.Contains(x.Codigo)).ToListAsync()
                : new List<IpressDepartamento>();
            var provincias = ubigeo4.Count > 0
                ? await _context.Provincias.AsNoTracking().Include(p => p.Departamento).Where(p => ubigeo4.Contains(p.Codigo)).ToListAsync()
                : new List<IpressProvincia>();
            var distritos = ubigeo6.Count > 0
                ? await _context.Distritos.AsNoTracking().Include(x => x.Provincia).ThenInclude(p => p!.Departamento).Where(x => ubigeo6.Contains(x.Ubigeo)).ToListAsync()
                : new List<IpressDistrito>();

            var list = new List<object>();
            foreach (var d in diresas)
            {
                string? dep = null, prov = null, dist = null;
                if (d.Distrito != null && d.Distrito.Provincia != null && d.Distrito.Provincia.Departamento != null)
                {
                    dep = d.Distrito.Provincia.Departamento.Nombre;
                    prov = d.Distrito.Provincia.Nombre;
                    dist = d.Distrito.Nombre;
                }
                else if (!string.IsNullOrEmpty(d.Ubigeo))
                {
                    if (d.Ubigeo.Length == 6)
                    {
                        var distrito = distritos.FirstOrDefault(x => x.Ubigeo == d.Ubigeo);
                        if (distrito?.Provincia != null)
                        {
                            dep = distrito.Provincia.Departamento?.Nombre;
                            prov = distrito.Provincia.Nombre;
                            dist = distrito.Nombre;
                        }
                    }
                    else if (d.Ubigeo.Length == 4)
                    {
                        var provincia = provincias.FirstOrDefault(p => p.Codigo == d.Ubigeo);
                        if (provincia != null)
                        {
                            dep = provincia.Departamento?.Nombre;
                            prov = provincia.Nombre;
                        }
                    }
                    else if (d.Ubigeo.Length == 2)
                    {
                        var departamento = departamentos.FirstOrDefault(x => x.Codigo == d.Ubigeo);
                        if (departamento != null) dep = departamento.Nombre;
                    }
                }
                // Ubigeo siempre con el valor almacenado (2/4/6) para que el modal editar pueda llenar Dep/Prov/Dist
                list.Add(new { d.IdDiresa, d.Codigo, d.Nombre, Departamento = dep, Provincia = prov, Distrito = dist, Ubigeo = d.Ubigeo });
            }
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IpressDiresa>> Get(int id)
        {
            var r = await _context.Diresas.Include(d => d.Distrito).ThenInclude(x => x!.Provincia).ThenInclude(p => p!.Departamento).FirstOrDefaultAsync(d => d.IdDiresa == id);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPost]
        public async Task<ActionResult<IpressDiresa>> Post([FromBody] PutDiresaRequest d)
        {
            if (await _context.Diresas.AnyAsync(x => x.Codigo == d.Codigo))
                return BadRequest(new { message = "El código ya existe." });
            var entity = new IpressDiresa
            {
                Codigo = d.Codigo,
                Nombre = d.Nombre ?? "",
                Ubigeo = string.IsNullOrWhiteSpace(d.Ubigeo) ? null : (d.Ubigeo.Trim().Length == 6 ? d.Ubigeo.Trim() : null),
                Activo = true
            };
            _context.Diresas.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.IdDiresa }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PutDiresaRequest d)
        {
            var exist = await _context.Diresas.FindAsync(id);
            if (exist == null) return NotFound();
            exist.Codigo = d.Codigo;
            exist.Nombre = d.Nombre ?? "";
            exist.Ubigeo = string.IsNullOrWhiteSpace(d.Ubigeo) ? null : (d.Ubigeo.Trim().Length == 6 ? d.Ubigeo.Trim() : null);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _context.Diresas.FindAsync(id);
            if (d == null) return NotFound();
            var tieneRedes = await _context.Redes.AnyAsync(r => r.IdDiresa == id && r.Activo);
            if (tieneRedes)
                return BadRequest(new { message = "No se puede eliminar la Diresa porque tiene al menos una Red asociada." });
            d.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Formato de carga masiva con listas desplegables Departamento → Provincia → Distrito; Ubigeo se calcula por fórmula.</summary>
        [HttpGet("formato-descarga")]
        public async Task<IActionResult> DescargarFormato()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var departamentos = await _context.Departamentos.AsNoTracking().OrderBy(x => x.Nombre).Select(x => new { x.Codigo, x.Nombre }).ToListAsync();
                var provincias = await _context.Provincias.AsNoTracking().OrderBy(x => x.CodigoDepartamento).ThenBy(x => x.Nombre).Select(x => new { x.CodigoDepartamento, x.Nombre, x.Codigo }).ToListAsync();
                var distritos = await _context.Distritos.AsNoTracking().OrderBy(x => x.CodigoProvincia).ThenBy(x => x.Nombre).Select(x => new { x.CodigoProvincia, x.Nombre, x.Ubigeo }).ToListAsync();

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        // Hoja listas: Departamentos (Nombre, Codigo) para validación y VLOOKUP
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

                        // Hoja Provincias (CodigoDepartamento, Nombre, Codigo)
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

                        // Hoja Distritos (CodigoProvincia, Nombre, Ubigeo)
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

                        // Rangos con nombre por departamento: Prov_01, Prov_02, ... (nombres) y Prov_01_Data (Nombre,Codigo para VLOOKUP)
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

                        // Rangos por provincia: Dist_0101, Dist_0101_Data
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

                        // Hoja de datos Diresas: A=Codigo, B=Diresa, C=Departamento, D=CodigoDep(hidden), E=Provincia, F=CodigoProv(hidden), G=Distrito, H=Ubigeo
                        var ws = package.Workbook.Worksheets.Add("Diresas");
                        ws.Cells[1, 1].Value = "Codigo";
                        ws.Cells[1, 2].Value = "Diresa";
                        ws.Cells[1, 3].Value = "Departamento";
                        ws.Cells[1, 4].Value = "CodigoDep";
                        ws.Cells[1, 5].Value = "Provincia";
                        ws.Cells[1, 6].Value = "CodigoProv";
                        ws.Cells[1, 7].Value = "Distrito";
                        ws.Cells[1, 8].Value = "Ubigeo";
                        ws.Column(1).Style.Numberformat.Format = "@"; // Texto para conservar ceros a la izquierda (ej. 00002)
                        ws.Column(8).Style.Numberformat.Format = "@";
                        const int dataRows = 500;
                        string depRange = depRows > 0 ? $"Departamentos!$A$2:$A${1 + depRows}" : "Departamentos!$A$2:$A$2";

                        for (int r = 2; r <= dataRows + 1; r++)
                        {
                            // D: CodigoDep = VLOOKUP(C; Departamentos; 2)
                            ws.Cells[r, 4].Formula = $"IF(C{r}=\"\",\"\",VLOOKUP(C{r},Departamentos!$A:$B,2,0))";
                            // F: CodigoProv = VLOOKUP(E; Prov_X_Data; 2)
                            ws.Cells[r, 6].Formula = $"IF(E{r}=\"\",\"\",VLOOKUP(E{r},INDIRECT(\"Prov_\"&D{r}&\"_Data\"),2,0))";
                            // H: Ubigeo = VLOOKUP(G; Dist_X_Data; 2)
                            ws.Cells[r, 8].Formula = $"IF(G{r}=\"\",\"\",VLOOKUP(G{r},INDIRECT(\"Dist_\"&F{r}&\"_Data\"),2,0))";
                        }

                        // Validación lista Departamento (col C) — opcional: permitir celda en blanco
                        var valDep = ws.DataValidations.AddListValidation($"C2:C{dataRows + 1}");
                        valDep.Formula.ExcelFormula = depRange;
                        valDep.AllowBlank = true;
                        valDep.ShowErrorMessage = true;
                        valDep.ErrorTitle = "Departamento";
                        valDep.Error = "Seleccione un valor de la lista (opcional).";

                        // Validación lista Provincia (col E) — opcional
                        var valProv = ws.DataValidations.AddListValidation($"E2:E{dataRows + 1}");
                        valProv.Formula.ExcelFormula = "INDIRECT(\"Prov_\"&D2)";
                        valProv.AllowBlank = true;
                        valProv.ShowErrorMessage = true;

                        // Validación lista Distrito (col G) — opcional
                        var valDist = ws.DataValidations.AddListValidation($"G2:G{dataRows + 1}");
                        valDist.Formula.ExcelFormula = "INDIRECT(\"Dist_\"&F2)";
                        valDist.AllowBlank = true;
                        valDist.ShowErrorMessage = true;

                        // Ocultar columnas D (CodigoDep) y F (CodigoProv): no se muestran al usuario, solo para fórmulas
                        ws.Column(4).Hidden = true;
                        ws.Column(6).Hidden = true;

                        // Ocultar hojas de listas: solo la hoja "Diresas" debe verse al abrir el libro
                        wsDep.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        wsProv.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        wsDist.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                        // Pestaña activa al abrir = Diresas (índice 3: Departamentos=0, Provincias=1, Distritos=2, Diresas=3)
                        package.Workbook.View.ActiveTab = 3;

                        package.Save();
                    }
                    bytes = stream.ToArray();
                }
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Formato_Diresas.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el formato.", detail = ex.Message });
            }
        }

        /// <summary>Exporta las Diresas registradas (con ubicación) a Excel.</summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var data = await _context.Diresas.AsNoTracking()
                    .Include(d => d.Distrito).ThenInclude(x => x!.Provincia).ThenInclude(p => p!.Departamento)
                    .Where(d => d.Activo)
                    .OrderBy(d => d.Nombre)
                    .ToListAsync();

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var ws = package.Workbook.Worksheets.Add("Diresas");
                        ws.Cells[1, 1].Value = "Codigo";
                        ws.Cells[1, 2].Value = "Diresa";
                        ws.Cells[1, 3].Value = "Departamento";
                        ws.Cells[1, 4].Value = "Provincia";
                        ws.Cells[1, 5].Value = "Distrito";
                        ws.Cells[1, 6].Value = "Ubigeo";

                        var row = 2;
                        foreach (var d in data)
                        {
                            ws.Cells[row, 1].Value = d.Codigo;
                            ws.Cells[row, 2].Value = d.Nombre;
                            var dep = d.Distrito?.Provincia?.Departamento?.Nombre;
                            var prov = d.Distrito?.Provincia?.Nombre;
                            var dist = d.Distrito?.Nombre;
                            ws.Cells[row, 3].Value = dep;
                            ws.Cells[row, 4].Value = prov;
                            ws.Cells[row, 5].Value = dist;
                            ws.Cells[row, 6].Value = d.Ubigeo;
                            row++;
                        }

                        ws.Column(1).Style.Numberformat.Format = "@";
                        ws.Column(6).Style.Numberformat.Format = "@";
                        package.Save();
                    }
                    bytes = stream.ToArray();
                }

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Diresas.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al exportar las Diresas.", detail = ex.Message });
            }
        }

        /// <summary>Vista previa del Excel enviado en JSON (base64). Usar este endpoint para evitar el cierre del proceso al seleccionar archivo.</summary>
        [HttpPost("preview-json")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> PreviewJson([FromBody] ExcelFileRequest? body, CancellationToken cancellationToken = default)
        {
            try
            {
                if (body == null || string.IsNullOrWhiteSpace(body.FileBase64))
                    return BadRequest(new { message = "Envíe el cuerpo JSON con la propiedad 'fileBase64' (archivo Excel en base64)." });

                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(body.FileBase64.Trim());
                }
                catch (FormatException)
                {
                    return BadRequest(new { message = "El valor de 'fileBase64' no es base64 válido." });
                }

                if (fileBytes.Length == 0)
                    return BadRequest(new { message = "El archivo está vacío." });
                if (fileBytes.Length > MaxFileSizeBytes)
                    return BadRequest(new { message = "El archivo no debe superar 8 MB." });

                var list = await _context.Diresas.AsNoTracking().Where(d => d.Activo).Select(d => d.Codigo).ToListAsync(cancellationToken);
                var codigosExistentes = new HashSet<int>(list);

                var helperOutput = await ExcelHelperRunner.RunAsync(fileBytes, cancellationToken);
                if (helperOutput == null)
                    return StatusCode(500, new { message = "No se pudo procesar el archivo. Ejecute la solución desde Visual Studio o 'dotnet build' en la raíz y vuelva a ejecutar la API (debe existir la carpeta ExcelHelper junto al ejecutable)." });

                if (!helperOutput.Ok)
                    return StatusCode(500, new { message = "Error al procesar el Excel: " + (helperOutput.ErrorMessage ?? "error desconocido") });

                var filas = new List<object>();
                var errores = new List<string>(helperOutput.Errores ?? new List<string>());
                var duplicados = 0;
                var tieneErrores = false;
                var showDepartamento = false;
                var showProvincia = false;
                var showDistrito = false;
                var showUbigeo = false;
                foreach (var row in helperOutput.Rows ?? new List<ExcelHelperRunner.RowOutput>())
                {
                    if (string.IsNullOrWhiteSpace(row.Codigo) && string.IsNullOrWhiteSpace(row.Nombre)) continue;
                    string estado;
                    string? mensaje = null;
                    if (string.IsNullOrWhiteSpace(row.Codigo))
                    {
                        estado = "error";
                        mensaje = "Código vacío";
                        tieneErrores = true;
                    }
                    else if (!int.TryParse(row.Codigo.Trim(), out var codigoInt))
                    {
                        estado = "error";
                        mensaje = "Código debe ser numérico";
                        tieneErrores = true;
                    }
                    else if (string.IsNullOrWhiteSpace(row.Nombre))
                    {
                        estado = "error";
                        mensaje = "Nombre vacío";
                        tieneErrores = true;
                    }
                    else if (codigosExistentes.Contains(codigoInt))
                    {
                        estado = "duplicado";
                        mensaje = "Registro duplicado (el código ya existe)";
                        duplicados++;
                    }
                    else
                    {
                        estado = "ok";
                        codigosExistentes.Add(codigoInt);
                    }
                    var dep = (row.Departamento ?? "").Trim();
                    var prov = (row.Provincia ?? "").Trim();
                    var dist = (row.Distrito ?? "").Trim();
                    var ubigeoVal = (row.Ubigeo ?? "").Trim();
                    if (ubigeoVal.Length == 6) showUbigeo = true;
                    if (dep.Length > 0) showDepartamento = true;
                    if (prov.Length > 0) showProvincia = true;
                    if (dist.Length > 0) showDistrito = true;
                    filas.Add(new
                    {
                        codigo = row.Codigo,
                        nombre = row.Nombre,
                        departamento = dep,
                        provincia = prov,
                        distrito = dist,
                        ubigeo = ubigeoVal.Length == 6 ? ubigeoVal : "",
                        estado,
                        mensaje
                    });
                }
                var columnas = new List<string>();
                if (showDepartamento) columnas.Add("departamento");
                if (showProvincia) columnas.Add("provincia");
                if (showDistrito) columnas.Add("distrito");
                if (showUbigeo) columnas.Add("ubigeo");
                if (errores.Count > 0) tieneErrores = true;
                return Ok(new { filas, columnas, errores, duplicados, tieneErrores });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar el Excel: " + (ex.Message ?? "error desconocido") });
            }
        }

        /// <summary>Importar Excel enviado en JSON (base64). Usar este endpoint para evitar el cierre del proceso.</summary>
        [HttpPost("importar-json")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> ImportarJson([FromBody] ExcelFileRequest? body, CancellationToken cancellationToken = default)
        {
            try
            {
                if (body == null || string.IsNullOrWhiteSpace(body.FileBase64))
                    return BadRequest(new { message = "Envíe el cuerpo JSON con la propiedad 'fileBase64' (archivo Excel en base64)." });

                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(body.FileBase64.Trim());
                }
                catch (FormatException)
                {
                    return BadRequest(new { message = "El valor de 'fileBase64' no es base64 válido." });
                }

                if (fileBytes.Length == 0)
                    return BadRequest(new { message = "El archivo está vacío." });
                if (fileBytes.Length > MaxFileSizeBytes)
                    return BadRequest(new { message = "El archivo no debe superar 8 MB." });

                var erroresList = new List<string>();
                var rowsToInsert = new List<(int Codigo, string Nombre, string Departamento, string Provincia, string Distrito, string Ubigeo)>();

                var helperOutput = await ExcelHelperRunner.RunAsync(fileBytes, cancellationToken);
                if (helperOutput == null)
                    return StatusCode(500, new { message = "No se pudo procesar el archivo. Ejecute 'dotnet build' en la raíz de la solución y vuelva a ejecutar la API (debe existir la carpeta ExcelHelper junto al ejecutable)." });

                if (!helperOutput.Ok)
                    return StatusCode(500, new { message = "Error al procesar el Excel: " + (helperOutput.ErrorMessage ?? "error desconocido") });

                foreach (var row in helperOutput.Rows ?? new List<ExcelHelperRunner.RowOutput>())
                {
                    if (string.IsNullOrWhiteSpace(row.Codigo) && string.IsNullOrWhiteSpace(row.Nombre)) continue;
                    if (string.IsNullOrWhiteSpace(row.Codigo)) { erroresList.Add("Fila: Código vacío"); continue; }
                    if (!int.TryParse(row.Codigo.Trim(), out var codigoInt)) { erroresList.Add("Fila: Código debe ser numérico"); continue; }
                    if (string.IsNullOrWhiteSpace(row.Nombre)) { erroresList.Add("Fila: Nombre vacío"); continue; }
                    var dep = (row.Departamento ?? "").Trim();
                    var prov = (row.Provincia ?? "").Trim();
                    var dist = (row.Distrito ?? "").Trim();
                    var ub = (row.Ubigeo ?? "").Trim();
                    rowsToInsert.Add((codigoInt, row.Nombre, dep, prov, dist, ub));
                }
                erroresList.AddRange(helperOutput.Errores ?? new List<string>());

                if (rowsToInsert.Count == 0 && erroresList.Count > 0)
                    return Ok(new { insertados = 0, duplicados = 0, errores = erroresList });

                var codigosExistentes = await _context.Diresas.Where(d => d.Activo).Select(d => d.Codigo).ToListAsync(cancellationToken);
                var insertados = 0;
                var duplicados = 0;
                foreach (var (codigo, nombre, departamento, provincia, distrito, ubigeo) in rowsToInsert)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (codigosExistentes.Contains(codigo)) { duplicados++; continue; }
                    var ubigeoNorm = await ResolverUbigeoDiresaAsync(departamento, provincia, distrito, ubigeo, cancellationToken);
                    _context.Diresas.Add(new IpressDiresa { Codigo = codigo, Nombre = nombre, Ubigeo = ubigeoNorm, Activo = true });
                    codigosExistentes.Add(codigo);
                    insertados++;
                }
                await _context.SaveChangesAsync(cancellationToken);
                return Ok(new { insertados, duplicados, errores = erroresList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al procesar el Excel: " + (ex.Message ?? "error desconocido") });
            }
        }

        /// <summary>Resuelve nombres Dep/Prov/Dist o ubigeo 6 dígitos a valor a guardar: 2 (dep), 4 (prov) o 6 (distrito) caracteres.</summary>
        private async Task<string?> ResolverUbigeoDiresaAsync(string? depNombre, string? provNombre, string? distNombre, string? ubigeo, CancellationToken cancellationToken)
        {
            var ub = (ubigeo ?? "").Trim();
            if (ub.Length == 6 && ub.All(char.IsDigit)) return ub;

            var dep = (depNombre ?? "").Trim();
            var prov = (provNombre ?? "").Trim();
            var dist = (distNombre ?? "").Trim();

            if (dist.Length > 0 && prov.Length > 0 && dep.Length > 0)
            {
                var codigoDep = await _context.Departamentos.AsNoTracking()
                    .Where(x => x.Nombre.Trim().ToUpper() == dep.ToUpper())
                    .Select(x => x.Codigo).FirstOrDefaultAsync(cancellationToken);
                if (string.IsNullOrEmpty(codigoDep)) return null;
                var codigoProv = await _context.Provincias.AsNoTracking()
                    .Where(p => p.CodigoDepartamento == codigoDep && p.Nombre.Trim().ToUpper() == prov.ToUpper())
                    .Select(p => p.Codigo).FirstOrDefaultAsync(cancellationToken);
                if (string.IsNullOrEmpty(codigoProv)) return null;
                var ubigeoDist = await _context.Distritos.AsNoTracking()
                    .Where(x => x.CodigoProvincia == codigoProv && x.Nombre.Trim().ToUpper() == dist.ToUpper())
                    .Select(x => x.Ubigeo).FirstOrDefaultAsync(cancellationToken);
                return ubigeoDist;
            }
            if (prov.Length > 0 && dep.Length > 0)
            {
                var codigoDep = await _context.Departamentos.AsNoTracking()
                    .Where(x => x.Nombre.Trim().ToUpper() == dep.ToUpper())
                    .Select(x => x.Codigo).FirstOrDefaultAsync(cancellationToken);
                if (string.IsNullOrEmpty(codigoDep)) return null;
                var codigoProv = await _context.Provincias.AsNoTracking()
                    .Where(p => p.CodigoDepartamento == codigoDep && p.Nombre.Trim().ToUpper() == prov.ToUpper())
                    .Select(p => p.Codigo).FirstOrDefaultAsync(cancellationToken);
                return codigoProv;
            }
            if (dep.Length > 0)
            {
                var codigoDep = await _context.Departamentos.AsNoTracking()
                    .Where(x => x.Nombre.Trim().ToUpper() == dep.ToUpper())
                    .Select(x => x.Codigo).FirstOrDefaultAsync(cancellationToken);
                return codigoDep;
            }
            return null;
        }
    }
}
