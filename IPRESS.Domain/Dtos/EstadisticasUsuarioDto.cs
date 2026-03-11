namespace IPRESS.Domain.Dtos
{
    public class EstadisticasUsuarioDto
    {
        public int AtencionesHoy { get; set; }
        public int AtencionesMes { get; set; }
        public int AtencionesPausadas { get; set; }
        public string TiempoPromedioAtencion { get; set; } = "0min";
        public int TotalAtenciones { get; set; }
    }
}
