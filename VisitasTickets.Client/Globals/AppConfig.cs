namespace VisitasTickets.Client.Globals
{
    public static class AppConfig
    {
        public const int DefaultAreaId = 144;
        
        // Para acceso local: "https://localhost:7248"
        // Para acceso desde celular en la misma red: "https://192.168.1.217:7248" (usa tu IP local)
        // Para obtener tu IP: ipconfig en CMD, busca "Dirección IPv4" de tu adaptador de red activo
        public const string ApiBaseUrl = "https://192.168.1.217:7248";
    }
}
