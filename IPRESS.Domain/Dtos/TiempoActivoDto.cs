namespace IPRESS.Domain.Dtos
{
    public class TiempoActivoDto
    {
        public int TiempoActivoMinutos { get; set; }
        public int TiempoActivoSegundos { get; set; }
        public string? EstadoActual { get; set; }
        public bool EstadoActivo { get; set; }
        public int? OrdenEstado { get; set; }
    }
}
