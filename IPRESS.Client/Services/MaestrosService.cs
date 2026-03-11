using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPRESS.Client.Services
{
    public class MaestrosService
    {
        private readonly HttpClient _http;

        public MaestrosService(HttpClient http) => _http = http;

        // Diresas
        public async Task<List<DiresaDto>> GetDiresasAsync() =>
            await _http.GetFromJsonAsync<List<DiresaDto>>("api/diresas") ?? new();

        public async Task<bool> PostDiresaAsync(DiresaDto d)
        {
            var r = await _http.PostAsJsonAsync("api/diresas", d);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> PutDiresaAsync(int id, DiresaDto d) =>
            (await _http.PutAsJsonAsync($"api/diresas/{id}", d)).IsSuccessStatusCode;

        public async Task<(bool Success, string? ErrorMessage)> DeleteDiresaAsync(int id)
        {
            var res = await _http.DeleteAsync($"api/diresas/{id}");
            if (res.IsSuccessStatusCode) return (true, null);
            var body = await res.Content.ReadAsStringAsync();
            string? msg = null;
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(body);
                if (json.RootElement.TryGetProperty("message", out var m)) msg = m.GetString();
            }
            catch { msg = body; }
            return (false, msg ?? "No se pudo eliminar la Diresa.");
        }

        public async Task<List<GeografiaItemDto>> GetDepartamentosAsync() =>
            await _http.GetFromJsonAsync<List<GeografiaItemDto>>("api/diresas/departamentos") ?? new();
        public async Task<List<ProvinciaItemDto>> GetProvinciasAsync(string? codigoDepartamento = null)
        {
            var url = string.IsNullOrEmpty(codigoDepartamento) ? "api/diresas/provincias" : $"api/diresas/provincias?codigoDepartamento={Uri.EscapeDataString(codigoDepartamento)}";
            return await _http.GetFromJsonAsync<List<ProvinciaItemDto>>(url) ?? new();
        }
        public async Task<List<DistritoItemDto>> GetDistritosAsync(string? codigoProvincia = null)
        {
            var url = string.IsNullOrEmpty(codigoProvincia) ? "api/diresas/distritos" : $"api/diresas/distritos?codigoProvincia={Uri.EscapeDataString(codigoProvincia)}";
            return await _http.GetFromJsonAsync<List<DistritoItemDto>>(url) ?? new();
        }

        public async Task<byte[]?> DescargarFormatoDiresasAsync()
        {
            try
            {
                var r = await _http.GetAsync("api/diresas/formato-descarga");
                if (!r.IsSuccessStatusCode) return null;
                return await r.Content.ReadAsByteArrayAsync();
            }
            catch { return null; }
        }

        public async Task<byte[]?> ExportarDiresasAsync()
        {
            try
            {
                var r = await _http.GetAsync("api/diresas/exportar");
                if (!r.IsSuccessStatusCode) return null;
                return await r.Content.ReadAsByteArrayAsync();
            }
            catch { return null; }
        }

        public async Task<(ImportPreviewDto? Data, string? ErrorMessage)> PreviewDiresasAsyncWithError(Stream fileStream, string fileName)
        {
            byte[] bytes;
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                bytes = ms.ToArray();
            }
            catch (Exception ex)
            {
                return (null, "No se pudo leer el archivo: " + (ex.Message ?? "intente de nuevo."));
            }

            // Usar preview-json (base64) para evitar que el proceso se cierre con multipart
            var body = new { fileBase64 = Convert.ToBase64String(bytes) };
            for (int intento = 0; intento < 2; intento++)
            {
                try
                {
                    var r = await _http.PostAsJsonAsync("api/diresas/preview-json", body);
                    if (r.IsSuccessStatusCode)
                        return (await r.Content.ReadFromJsonAsync<ImportPreviewDto>(), null);
                    var errBody = await r.Content.ReadAsStringAsync();
                    var msg = ExtraerMensajeError(errBody);
                    return (null, msg ?? "No se pudo procesar el archivo. Revise que sea un Excel (.xlsx) con hoja 'Diresas' y columnas Codigo, Nombre, Ubigeo.");
                }
                catch (Exception ex)
                {
                    var m = ex.Message ?? "";
                    var esErrorConexion = m.Contains("REFUSED", StringComparison.OrdinalIgnoreCase) ||
                                         m.Contains("fetch", StringComparison.OrdinalIgnoreCase) ||
                                         m.Contains("Failed to fetch", StringComparison.OrdinalIgnoreCase);
                    if (esErrorConexion)
                    {
                        var mensaje = "No se pudo conectar con el servidor (conexión rechazada). " +
                            "Asegúrese de que está ejecutando el proyecto IPRESS.API como proyecto de inicio (no el Client), " +
                            "que la ventana/consola donde lo inició sigue abierta y que la URL es la correcta (ej. http://localhost:5116).";
                        if (intento == 0)
                        {
                            await Task.Delay(800);
                            continue;
                        }
                        return (null, mensaje);
                    }
                    return (null, "Error al obtener la vista previa: " + (m.Length > 120 ? m.Substring(0, 120) + "…" : m));
                }
            }

            return (null, "No se pudo conectar con el servidor. Compruebe que IPRESS.API esté en ejecución.");
        }

        private static string? ExtraerMensajeError(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var prop))
                    return prop.GetString();
            }
            catch { }
            return json.Length > 200 ? "Error del servidor." : json;
        }

        public async Task<ImportResultDto?> ImportarDiresasAsync(Stream fileStream, string fileName)
        {
            try
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await fileStream.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }
                var body = new { fileBase64 = Convert.ToBase64String(bytes) };
                var r = await _http.PostAsJsonAsync("api/diresas/importar-json", body);
                if (!r.IsSuccessStatusCode)
                {
                    var errBody = await r.Content.ReadAsStringAsync();
                    var msg = ExtraerMensajeError(errBody) ?? "Error en el servidor.";
                    throw new InvalidOperationException(msg);
                }
                return await r.Content.ReadFromJsonAsync<ImportResultDto>();
            }
            catch (InvalidOperationException) { throw; }
            catch { return null; }
        }

        // Red
        public async Task<List<RedDto>> GetRedesAsync(int? idDiresa = null)
        {
            var url = idDiresa.HasValue ? $"api/red?idDiresa={idDiresa}" : "api/red";
            return await _http.GetFromJsonAsync<List<RedDto>>(url) ?? new();
        }

        public async Task<bool> PostRedAsync(RedDto d) =>
            (await _http.PostAsJsonAsync("api/red", d)).IsSuccessStatusCode;

        public async Task<bool> PutRedAsync(int id, RedDto d) =>
            (await _http.PutAsJsonAsync($"api/red/{id}", d)).IsSuccessStatusCode;

        public async Task<bool> DeleteRedAsync(int id) =>
            (await _http.DeleteAsync($"api/red/{id}")).IsSuccessStatusCode;

        public async Task<byte[]?> DescargarFormatoRedAsync()
        {
            var r = await _http.GetAsync("api/red/formato-descarga");
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]?> ExportarRedAsync()
        {
            var r = await _http.GetAsync("api/red/exportar");
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadAsByteArrayAsync();
        }

        public async Task<(ImportPreviewDto?, string?)> PreviewRedAsyncWithError(Stream fileStream, string fileName)
        {
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var body = new { fileBase64 = Convert.ToBase64String(bytes) };
                var r = await _http.PostAsJsonAsync("api/red/preview-json", body);
                if (r.IsSuccessStatusCode)
                    return (await r.Content.ReadFromJsonAsync<ImportPreviewDto>(), null);
                var errBody = await r.Content.ReadAsStringAsync();
                return (null, ExtraerMensajeError(errBody) ?? "No se pudo validar el archivo Red.");
            }
            catch (Exception ex) { return (null, ex.Message ?? "Error al validar."); }
        }

        public async Task<ImportResultDto?> ImportarRedAsync(Stream fileStream, string fileName)
        {
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                var body = new { fileBase64 = Convert.ToBase64String(ms.ToArray()) };
                var r = await _http.PostAsJsonAsync("api/red/importar-json", body);
                if (!r.IsSuccessStatusCode) { var err = await r.Content.ReadAsStringAsync(); throw new InvalidOperationException(ExtraerMensajeError(err) ?? "Error en el servidor."); }
                return await r.Content.ReadFromJsonAsync<ImportResultDto>();
            }
            catch (InvalidOperationException) { throw; }
            catch { return null; }
        }

        // MicroRed
        public async Task<List<MicroRedDto>> GetMicroRedesAsync(int? idRed = null)
        {
            var url = idRed.HasValue ? $"api/microred?idRed={idRed}" : "api/microred";
            return await _http.GetFromJsonAsync<List<MicroRedDto>>(url) ?? new();
        }

        public async Task<bool> PostMicroRedAsync(MicroRedDto d) =>
            (await _http.PostAsJsonAsync("api/microred", d)).IsSuccessStatusCode;

        public async Task<bool> PutMicroRedAsync(int id, MicroRedDto d) =>
            (await _http.PutAsJsonAsync($"api/microred/{id}", d)).IsSuccessStatusCode;

        public async Task<bool> DeleteMicroRedAsync(int id) =>
            (await _http.DeleteAsync($"api/microred/{id}")).IsSuccessStatusCode;

        public async Task<byte[]?> DescargarFormatoMicroRedAsync()
        {
            var r = await _http.GetAsync("api/microred/formato-descarga");
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]?> ExportarMicroRedAsync()
        {
            var r = await _http.GetAsync("api/microred/exportar");
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadAsByteArrayAsync();
        }

        public async Task<(ImportPreviewDto?, string?)> PreviewMicroRedAsyncWithError(Stream fileStream, string fileName)
        {
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var body = new { fileBase64 = Convert.ToBase64String(bytes) };
                var r = await _http.PostAsJsonAsync("api/microred/preview-json", body);
                if (r.IsSuccessStatusCode)
                    return (await r.Content.ReadFromJsonAsync<ImportPreviewDto>(), null);
                var errBody = await r.Content.ReadAsStringAsync();
                return (null, ExtraerMensajeError(errBody) ?? "No se pudo validar el archivo MicroRed.");
            }
            catch (Exception ex) { return (null, ex.Message ?? "Error al validar."); }
        }

        public async Task<ImportResultDto?> ImportarMicroRedAsync(Stream fileStream, string fileName)
        {
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                var body = new { fileBase64 = Convert.ToBase64String(ms.ToArray()) };
                var r = await _http.PostAsJsonAsync("api/microred/importar-json", body);
                if (!r.IsSuccessStatusCode) { var err = await r.Content.ReadAsStringAsync(); throw new InvalidOperationException(ExtraerMensajeError(err) ?? "Error en el servidor."); }
                return await r.Content.ReadFromJsonAsync<ImportResultDto>();
            }
            catch (InvalidOperationException) { throw; }
            catch { return null; }
        }

        // Establecimientos (deserializar con camelCase de la API)
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public async Task<List<EstablecimientoListDto>> GetEstablecimientosAsync(int? idMicroRed = null)
        {
            try
            {
                var url = idMicroRed.HasValue ? $"api/establecimientos?idMicroRed={idMicroRed}" : "api/establecimientos";
                return await _http.GetFromJsonAsync<List<EstablecimientoListDto>>(url, _jsonOpts) ?? new List<EstablecimientoListDto>();
            }
            catch { return new List<EstablecimientoListDto>(); }
        }

        public async Task<EstablecimientoDto?> GetEstablecimientoAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<EstablecimientoDto>($"api/establecimientos/{id}", _jsonOpts);
            }
            catch { return null; }
        }

        public async Task<bool> PostEstablecimientoAsync(EstablecimientoDto d) =>
            (await _http.PostAsJsonAsync("api/establecimientos", d)).IsSuccessStatusCode;

        public async Task<bool> PutEstablecimientoAsync(int id, EstablecimientoDto d) =>
            (await _http.PutAsJsonAsync($"api/establecimientos/{id}", d)).IsSuccessStatusCode;

        public async Task<bool> DeleteEstablecimientoAsync(int id) =>
            (await _http.DeleteAsync($"api/establecimientos/{id}")).IsSuccessStatusCode;

        // Centros Poblados
        private static readonly JsonSerializerOptions _cpOpts = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public async Task<List<CentroPobladoDto>> GetCentrosPobladosAsync(string? search = null)
        {
            var url = string.IsNullOrWhiteSpace(search) ? "api/centrospoblados" : $"api/centrospoblados?search={Uri.EscapeDataString(search)}";
            return await _http.GetFromJsonAsync<List<CentroPobladoDto>>(url, _cpOpts) ?? new();
        }

        public async Task<CentroPobladoFullDto?> GetCentroPobladoAsync(int id)
        {
            var r = await _http.GetAsync($"api/centrospoblados/{id}");
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadFromJsonAsync<CentroPobladoFullDto>(_cpOpts);
        }

        public async Task<bool> PostCentroPobladoAsync(CentroPobladoRequestDto dto)
        {
            var r = await _http.PostAsJsonAsync("api/centrospoblados", dto, _cpOpts);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> PutCentroPobladoAsync(int id, CentroPobladoRequestDto dto)
        {
            var r = await _http.PutAsJsonAsync($"api/centrospoblados/{id}", dto, _cpOpts);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCentroPobladoAsync(int id)
        {
            var r = await _http.DeleteAsync($"api/centrospoblados/{id}");
            return r.IsSuccessStatusCode;
        }
    }

    public class DiresaDto { public int IdDiresa { get; set; } public int Codigo { get; set; } public string Nombre { get; set; } = ""; public string? Departamento { get; set; } public string? Provincia { get; set; } public string? Distrito { get; set; } public string? Ubigeo { get; set; } }
    public class RedDto { public int IdRed { get; set; } public int IdDiresa { get; set; } public string? Diresa { get; set; } public int Codigo { get; set; } public string Nombre { get; set; } = ""; public string? Departamento { get; set; } public string? Provincia { get; set; } public string? Distrito { get; set; } public string? Ubigeo { get; set; } }
    public class MicroRedDto { public int IdMicroRed { get; set; } public int IdRed { get; set; } public string? Red { get; set; } public int Codigo { get; set; } public string Nombre { get; set; } = ""; public string? Departamento { get; set; } public string? Provincia { get; set; } public string? Distrito { get; set; } public string? Ubigeo { get; set; } }
    public class EstablecimientoListDto { public int IdEstablecimiento { get; set; } public string? Diresa { get; set; } public string? Red { get; set; } public string? MicroRed { get; set; } public string? QuintilRegional { get; set; } public string? Ubigeo { get; set; } public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; }
    public class EstablecimientoDto
    {
        public int IdEstablecimiento { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? QuintilRegional { get; set; }
        public string? Ubigeo { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public bool TieneTelefono { get; set; }
        public bool TieneRadio { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public List<CentroPobladoItemDto>? CentrosPoblados { get; set; }
        public List<int>? CentrosPobladoIds { get; set; }
    }
    public class GeografiaItemDto { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; }
    public class ProvinciaItemDto { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public string CodigoDepartamento { get; set; } = ""; }
    public class DistritoItemDto { public string Ubigeo { get; set; } = ""; public string Nombre { get; set; } = ""; public string CodigoProvincia { get; set; } = ""; }
    public class CentroPobladoItemDto { public int IdCentroPoblado { get; set; } public string? Ubigeo { get; set; } public string? UbigeoCcp { get; set; } public string? Departamento { get; set; } public string? Provincia { get; set; } public string? Distrito { get; set; } public string? CentroPoblado { get; set; } public string? Ambito { get; set; } public string? Quintil { get; set; } }
    public class CentroPobladoDto { public int IdCentroPoblado { get; set; } public string? Ubigeo { get; set; } public string? UbigeoCcp { get; set; } public string? Departamento { get; set; } public string? Provincia { get; set; } public string? Distrito { get; set; } public string? CentroPoblado { get; set; } public int? IdEstablecimiento { get; set; } public string? EstablecimientoNombre { get; set; } public string? Ambito { get; set; } public string? Quintil { get; set; } }
    public class CentroPobladoFullDto
    {
        public int IdCentroPoblado { get; set; }
        public string? Ubigeo { get; set; }
        public string? UbigeoCcp { get; set; }
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? CentroPoblado { get; set; }
        public int? IdEstablecimiento { get; set; }
        public EstablecimientoResumenDto? Establecimiento { get; set; }
        public string? Ambito { get; set; }
        public string? Quintil { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? PoblacionTotal { get; set; }
        public int? PoblacionServida { get; set; }
        public int? PoblacionVigilada { get; set; }
        public int? ElectricidadHrs { get; set; }
        public string? TelefonoTipo { get; set; }
        public string? TelefonoNumero { get; set; }
        public bool RadioEmisora { get; set; }
        public bool RadioESS { get; set; }
        public bool SenalTV { get; set; }
        public bool Internet { get; set; }
        public bool LimpiezaPublica { get; set; }
        public bool Agua { get; set; }
        public bool Letrinas { get; set; }
        public bool DesagueAlcantarillado { get; set; }
        public bool SistEliminacionExcretas { get; set; }
        public string? Vertimientos { get; set; }
        public decimal? TempMinima { get; set; }
        public decimal? TempMaxima { get; set; }
        public List<AccesibilidadItemDto>? Accesibilidades { get; set; }
        public List<CentroEducativoItemDto>? CentrosEducativos { get; set; }
        public List<AutoridadItemDto>? Autoridades { get; set; }
    }
    public class EstablecimientoResumenDto { public int IdEstablecimiento { get; set; } public string? Codigo { get; set; } public string? Nombre { get; set; } public string? Departamento { get; set; } public string? Provincia { get; set; } public string? Distrito { get; set; } public string? Ubigeo { get; set; } }
    public class CentroPobladoRequestDto
    {
        public string? Ubigeo { get; set; }
        public string? UbigeoCcp { get; set; }
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? CentroPoblado { get; set; }
        public int? IdEstablecimiento { get; set; }
        public string? Ambito { get; set; }
        public string? Quintil { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? PoblacionTotal { get; set; }
        public int? PoblacionServida { get; set; }
        public int? PoblacionVigilada { get; set; }
        public int? ElectricidadHrs { get; set; }
        public string? TelefonoTipo { get; set; }
        public string? TelefonoNumero { get; set; }
        public bool RadioEmisora { get; set; }
        public bool RadioESS { get; set; }
        public bool SenalTV { get; set; }
        public bool Internet { get; set; }
        public bool LimpiezaPublica { get; set; }
        public bool Agua { get; set; }
        public bool Letrinas { get; set; }
        public bool DesagueAlcantarillado { get; set; }
        public bool SistEliminacionExcretas { get; set; }
        public string? Vertimientos { get; set; }
        public decimal? TempMinima { get; set; }
        public decimal? TempMaxima { get; set; }
        public List<AccesibilidadItemDto>? Accesibilidades { get; set; }
        public List<CentroEducativoItemDto>? CentrosEducativos { get; set; }
        public List<AutoridadItemDto>? Autoridades { get; set; }
    }
    public class AccesibilidadItemDto { public int IdAccesibilidad { get; set; } public string? Desde { get; set; } public string? Hasta { get; set; } public decimal? DistanciaKm { get; set; } public int? TiempoMin { get; set; } public string? TipoVia { get; set; } public string? MedioTransporte { get; set; } }
    public class CentroEducativoItemDto { public int IdCentroEducativo { get; set; } public string? TipoCentroEducativo { get; set; } public string? NombreCentroEducativo { get; set; } }
    public class AutoridadItemDto { public int IdAutoridad { get; set; } public string? TipoAutoridad { get; set; } public string? NombreAutoridad { get; set; } }
    public class ImportResultDto { public int Insertados { get; set; } public int Duplicados { get; set; } public List<string> Errores { get; set; } = new(); }
    public class ImportPreviewDto
    {
        [JsonPropertyName("filas")] public List<ImportPreviewRowDto>? Filas { get; set; }
        [JsonPropertyName("columnas")] public List<string>? Columnas { get; set; }
        [JsonPropertyName("errores")] public List<string> Errores { get; set; } = new();
        [JsonPropertyName("duplicados")] public int Duplicados { get; set; }
        [JsonPropertyName("tieneErrores")] public bool TieneErrores { get; set; }
    }
    public class ImportPreviewRowDto
    {
        [JsonPropertyName("codigo")] public string? Codigo { get; set; }
        [JsonPropertyName("nombre")] public string? Nombre { get; set; }
        [JsonPropertyName("departamento")] public string? Departamento { get; set; }
        [JsonPropertyName("provincia")] public string? Provincia { get; set; }
        [JsonPropertyName("distrito")] public string? Distrito { get; set; }
        [JsonPropertyName("ubigeo")] public string? Ubigeo { get; set; }
        [JsonPropertyName("estado")] public string? Estado { get; set; }
        [JsonPropertyName("mensaje")] public string? Mensaje { get; set; }
    }
}
