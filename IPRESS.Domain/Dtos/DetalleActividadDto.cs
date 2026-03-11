namespace IPRESS.Domain.Dtos
{
    public class DetalleActividadDto
    {
        public int IdDetalleActividad { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
