namespace IPRESS.Domain.Interfaces
{
    /// <summary>Provee el ID del usuario actual y la IP de origen para auditoría (implementado en API con JWT/HttpContext).</summary>
    public interface IAuditUserIdProvider
    {
        int? GetUserId();
        /// <summary>IP del cliente para registrar en IPRESS_Auditoria.IpOrigen (máx. 45 caracteres).</summary>
        string? GetClientIp();
    }
}
