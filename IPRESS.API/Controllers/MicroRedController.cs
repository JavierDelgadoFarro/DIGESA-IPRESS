using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPRESS.Infrastructure.Persistence;
using IPRESS.Domain.Entities;

namespace IPRESS.API.Controllers
{
    public class ExcelFileRequestMicroRed { public string FileBase64 { get; set; } = ""; }

    public class PutMicroRedRequest
    {
        public int IdRed { get; set; }
        public int Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Ubigeo { get; set; }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MicroRedController : ControllerBase
    {
        private const int MaxFileSizeBytes = 8 * 1024 * 1024;
        private readonly IpressDbContext _context;

        public MicroRedController(IpressDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] int? idRed)
        {
            var q = _context.MicroRedes.AsNoTracking()
                .Include(m => m.Red)
                .Include(m => m.Distrito).ThenInclude(d => d!.Provincia).ThenInclude(p => p!.Departamento)
                .Where(m => m.Activo);
            if (idRed.HasValue) q = q.Where(m => m.IdRed == idRed.Value);
            return Ok(await q.OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    m.IdMicroRed,
                    m.IdRed,
                    Red = m.Red != null ? m.Red.Nombre : "",
                    m.Codigo,
                    m.Nombre,
                    Departamento = m.Distrito != null && m.Distrito.Provincia != null && m.Distrito.Provincia.Departamento != null ? m.Distrito.Provincia.Departamento.Nombre : null,
                    Provincia = m.Distrito != null && m.Distrito.Provincia != null ? m.Distrito.Provincia.Nombre : null,
                    Distrito = m.Distrito != null ? m.Distrito.Nombre : null,
                    m.Ubigeo
                }).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IpressMicroRed>> Get(int id)
        {
            var m = await _context.MicroRedes.Include(x => x.Red).FirstOrDefaultAsync(x => x.IdMicroRed == id);
            return m == null ? NotFound() : Ok(m);
        }

        [HttpPost]
        public async Task<ActionResult<IpressMicroRed>> Post([FromBody] PutMicroRedRequest m)
        {
            if (await _context.MicroRedes.AnyAsync(x => x.Codigo == m.Codigo && x.IdRed == m.IdRed))
                return BadRequest(new { message = "El código ya existe en esta Red." });
            var entity = new IpressMicroRed
            {
                IdRed = m.IdRed,
                Codigo = m.Codigo,
                Nombre = m.Nombre ?? "",
                Ubigeo = string.IsNullOrWhiteSpace(m.Ubigeo) ? null : (m.Ubigeo.Trim().Length == 6 ? m.Ubigeo.Trim() : null),
                Activo = true
            };
            _context.MicroRedes.Add(entity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = entity.IdMicroRed }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PutMicroRedRequest m)
        {
            var exist = await _context.MicroRedes.FindAsync(id);
            if (exist == null) return NotFound();
            exist.IdRed = m.IdRed;
            exist.Codigo = m.Codigo;
            exist.Nombre = m.Nombre ?? "";
            exist.Ubigeo = string.IsNullOrWhiteSpace(m.Ubigeo) ? null : (m.Ubigeo.Trim().Length == 6 ? m.Ubigeo.Trim() : null);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.MicroRedes.FindAsync(id);
            if (entity == null) return NotFound();
            entity.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Vista previa del Excel en JSON (base64). Mismo flujo que Diresas.</summary>
        [HttpPost("preview-json")]
        [RequestSizeLimit(12 * 1024 * 1024)]
        public async Task<IActionResult> PreviewJson([FromBody] ExcelFileRequestMicroRed? body, CancellationToken cancellationToken = default)
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

                var redesPorCodigo = await _context.Redes.Where(r => r.Activo).ToDictionaryAsync(r => r.Codigo, r => r.IdRed, cancellationToken);
                var parejasExistentes = await _context.MicroRedes.Where(m => m.Activo).Select(m => new { m.IdRed, m.Codigo }).ToListAsync(cancellationToken);
                var setExistentes = new HashSet<(int IdRed, int Codigo)>(parejasExistentes.Select(p => (p.IdRed, p.Codigo)));
                var distritosConGeo = await _context.Distritos.AsNoTracking()
                    .Include(d => d.Provincia).ThenInclude(p => p!.Departamento)
                    .ToListAsync(cancellationToken);
                var ubigeoPorNombres = distritosConGeo
                    .Where(d => d.Provincia != null && d.Provincia.Departamento != null)
                    .GroupBy(d => (Dep: d.Provincia!.Departamento!.Nombre.Trim().ToUpperInvariant(), Prov: d.Provincia.Nombre.Trim().ToUpperInvariant(), Dist: d.Nombre.Trim().ToUpperInvariant()))
                    .ToDictionary(g => g.Key, g => g.First().Ubigeo);

                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                var filas = new List<object>();
                var errores = new List<string>();
                var duplicados = 0;
                var tieneErrores = false;
                using (var stream = new MemoryStream(fileBytes))
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("MicroRed", StringComparison.OrdinalIgnoreCase)) ?? package.Workbook.Worksheets[0];
                    var rowCount = ws.Dimension?.Rows ?? 0;
                    if (rowCount < 2) return Ok(new { filas = new List<object>(), errores = new[] { "No hay filas de datos." }, duplicados = 0, tieneErrores = true });
                    for (var row = 2; row <= rowCount; row++)
                    {
                        var codigoRed = ws.Cells[row, 1].Text?.Trim();
                        var codigo = ws.Cells[row, 2].Text?.Trim();
                        var nombre = ws.Cells[row, 3].Text?.Trim();
                        var dep = ws.Cells[row, 4].Text?.Trim();
                        var prov = ws.Cells[row, 5].Text?.Trim();
                        var dist = ws.Cells[row, 6].Text?.Trim();
                        var ubigeo = ws.Cells[row, 7].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(nombre)) continue;
                        if (string.IsNullOrWhiteSpace(ubigeo) && !string.IsNullOrWhiteSpace(dep) && !string.IsNullOrWhiteSpace(prov) && !string.IsNullOrWhiteSpace(dist)
                            && ubigeoPorNombres.TryGetValue((dep.ToUpperInvariant(), prov.ToUpperInvariant(), dist.ToUpperInvariant()), out var u))
                            ubigeo = u;
                        string estado; string? mensaje = null;
                        if (string.IsNullOrWhiteSpace(codigoRed)) { estado = "error"; mensaje = "CodigoRed vacío"; tieneErrores = true; }
                        else if (string.IsNullOrWhiteSpace(codigo)) { estado = "error"; mensaje = "Código vacío"; tieneErrores = true; }
                        else if (!int.TryParse(codigo, out var codigoInt)) { estado = "error"; mensaje = "Código debe ser numérico"; tieneErrores = true; }
                        else if (string.IsNullOrWhiteSpace(nombre)) { estado = "error"; mensaje = "Nombre vacío"; tieneErrores = true; }
                        else if (!int.TryParse(codigoRed, out var codigoRedInt) || !redesPorCodigo.TryGetValue(codigoRedInt, out var idRed)) { estado = "error"; mensaje = "Red '" + codigoRed + "' no encontrada"; tieneErrores = true; }
                        else if (setExistentes.Contains((idRed, codigoInt))) { estado = "duplicado"; mensaje = "Registro duplicado (código ya existe en esta Red)"; duplicados++; }
                        else { estado = "ok"; setExistentes.Add((idRed, codigoInt)); }
                        filas.Add(new { codigo = codigo ?? "", nombre = nombre ?? "", departamento = dep ?? "", provincia = prov ?? "", distrito = dist ?? "", ubigeo = ubigeo ?? "", estado, mensaje });
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
        public async Task<IActionResult> ImportarJson([FromBody] ExcelFileRequestMicroRed? body, CancellationToken cancellationToken = default)
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

                var redesPorCodigo = await _context.Redes.Where(r => r.Activo).ToDictionaryAsync(r => r.Codigo, r => r.IdRed, cancellationToken);
                var parejasExistentes = await _context.MicroRedes.Where(m => m.Activo).Select(m => new { m.IdRed, m.Codigo }).ToListAsync(cancellationToken);
                var setExistentes = new HashSet<(int IdRed, int Codigo)>(parejasExistentes.Select(p => (p.IdRed, p.Codigo)));
                var distritosConGeoImport = await _context.Distritos.AsNoTracking()
                    .Include(d => d.Provincia).ThenInclude(p => p!.Departamento)
                    .ToListAsync(cancellationToken);
                var ubigeoPorNombresImport = distritosConGeoImport
                    .Where(d => d.Provincia != null && d.Provincia.Departamento != null)
                    .GroupBy(d => (Dep: d.Provincia!.Departamento!.Nombre.Trim().ToUpperInvariant(), Prov: d.Provincia.Nombre.Trim().ToUpperInvariant(), Dist: d.Nombre.Trim().ToUpperInvariant()))
                    .ToDictionary(g => g.Key, g => g.First().Ubigeo);
                var insertados = 0; var duplicados = 0; var errores = new List<string>();
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var stream = new MemoryStream(fileBytes))
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    var ws = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("MicroRed", StringComparison.OrdinalIgnoreCase)) ?? package.Workbook.Worksheets[0];
                    var rowCount = ws.Dimension?.Rows ?? 0;
                    if (rowCount < 2) return Ok(new { insertados = 0, duplicados = 0, errores = new[] { "No hay filas de datos." } });
                    for (var row = 2; row <= rowCount; row++)
                    {
                        var codigoRed = ws.Cells[row, 1].Text?.Trim();
                        var codigo = ws.Cells[row, 2].Text?.Trim();
                        var nombre = ws.Cells[row, 3].Text?.Trim();
                        var dep = ws.Cells[row, 4].Text?.Trim();
                        var prov = ws.Cells[row, 5].Text?.Trim();
                        var dist = ws.Cells[row, 6].Text?.Trim();
                        var ubigeo = ws.Cells[row, 7].Text?.Trim();
                        if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(nombre)) continue;
                        if (string.IsNullOrWhiteSpace(codigoRed)) { errores.Add($"Fila {row}: CodigoRed vacío"); continue; }
                        if (string.IsNullOrWhiteSpace(codigo)) { errores.Add($"Fila {row}: Código vacío"); continue; }
                        if (!int.TryParse(codigo, out var codigoInt)) { errores.Add($"Fila {row}: Código debe ser numérico"); continue; }
                        if (string.IsNullOrWhiteSpace(nombre)) { errores.Add($"Fila {row}: Nombre vacío"); continue; }
                        if (!int.TryParse(codigoRed, out var codigoRedInt) || !redesPorCodigo.TryGetValue(codigoRedInt, out var idRed)) { errores.Add($"Fila {row}: Red '{codigoRed}' no encontrada"); continue; }
                        if (setExistentes.Contains((idRed, codigoInt))) { duplicados++; continue; }
                        if (string.IsNullOrWhiteSpace(ubigeo) && !string.IsNullOrWhiteSpace(dep) && !string.IsNullOrWhiteSpace(prov) && !string.IsNullOrWhiteSpace(dist)
                            && ubigeoPorNombresImport.TryGetValue((dep.ToUpperInvariant(), prov.ToUpperInvariant(), dist.ToUpperInvariant()), out var uImp))
                            ubigeo = uImp;
                        var ubigeoValido = (!string.IsNullOrWhiteSpace(ubigeo) && ubigeo.Trim().Length == 6) ? ubigeo.Trim() : null;
                        _context.MicroRedes.Add(new IpressMicroRed { IdRed = idRed, Codigo = codigoInt, Nombre = nombre!, Ubigeo = ubigeoValido, Activo = true });
                        setExistentes.Add((idRed, codigoInt)); insertados++;
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return Ok(new { insertados, duplicados, errores });
            }
            catch (Exception ex) { return StatusCode(500, new { message = "Error al importar: " + (ex.Message ?? "error desconocido") }); }
        }

        [HttpGet("formato-descarga")]
        public IActionResult DescargarFormato()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var ws = package.Workbook.Worksheets.Add("MicroRed");
                        ws.Cells[1, 1].Value = "CodigoRed";
                        ws.Cells[1, 2].Value = "Codigo";
                        ws.Cells[1, 3].Value = "Nombre";
                        ws.Cells[1, 4].Value = "Departamento";
                        ws.Cells[1, 5].Value = "Provincia";
                        ws.Cells[1, 6].Value = "Distrito";
                        ws.Cells[1, 7].Value = "Ubigeo";
                        ws.Column(1).Style.Numberformat.Format = "@";
                        ws.Column(2).Style.Numberformat.Format = "@";
                        ws.Column(7).Style.Numberformat.Format = "@";
                        package.Save();
                    }
                    bytes = stream.ToArray();
                }
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Formato_MicroRed.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el formato.", detail = ex.Message });
            }
        }

        /// <summary>Exporta las MicroRedes registradas (con Red y ubicación) a Excel.</summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar()
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                var data = await _context.MicroRedes.AsNoTracking()
                    .Include(m => m.Red)
                    .Include(m => m.Distrito).ThenInclude(d => d!.Provincia).ThenInclude(p => p!.Departamento)
                    .Where(m => m.Activo)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        var ws = package.Workbook.Worksheets.Add("MicroRed");
                        ws.Cells[1, 1].Value = "Red";
                        ws.Cells[1, 2].Value = "Codigo";
                        ws.Cells[1, 3].Value = "Microred";
                        ws.Cells[1, 4].Value = "Departamento";
                        ws.Cells[1, 5].Value = "Provincia";
                        ws.Cells[1, 6].Value = "Distrito";
                        ws.Cells[1, 7].Value = "Ubigeo";

                        var row = 2;
                        foreach (var m in data)
                        {
                            ws.Cells[row, 1].Value = m.Red?.Nombre;
                            ws.Cells[row, 2].Value = m.Codigo;
                            ws.Cells[row, 3].Value = m.Nombre;
                            var dep = m.Distrito?.Provincia?.Departamento?.Nombre;
                            var prov = m.Distrito?.Provincia?.Nombre;
                            var dist = m.Distrito?.Nombre;
                            ws.Cells[row, 4].Value = dep;
                            ws.Cells[row, 5].Value = prov;
                            ws.Cells[row, 6].Value = dist;
                            ws.Cells[row, 7].Value = m.Ubigeo;
                            row++;
                        }

                        ws.Column(2).Style.Numberformat.Format = "@";
                        ws.Column(7).Style.Numberformat.Format = "@";
                        package.Save();
                    }
                    bytes = stream.ToArray();
                }

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MicroRedes.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al exportar las MicroRedes.", detail = ex.Message });
            }
        }
    }
}
