namespace IPRESS.Domain.Dtos
{
    public class AtencionCreateDto
    {
        public string TipoDocumento { get; set; } = string.Empty; // "DNI" o "CE"
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public int IdTipoTramite { get; set; }
        public string? Observacion { get; set; }
        public bool EsPreferencial { get; set; }
        public int? IdTipoPreferencial { get; set; }
    }
}
