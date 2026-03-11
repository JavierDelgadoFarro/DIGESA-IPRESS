namespace IPRESS.Client.Globals
{
    public static class AppConfig
    {
        public const int DefaultAreaId = 144;
        
        // HTTP local - sin certificado (evita errores de confianza)
        // API en http://localhost:5116
        public const string ApiBaseUrl = "http://localhost:5116";
    }
}
