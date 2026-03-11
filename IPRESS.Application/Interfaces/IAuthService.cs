namespace IPRESS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string username, string password);
        /// <summary>Genera JWT para el usuario ya autenticado (evita reenviar contraseña).</summary>
        string GenerateToken(int usuarioId, string nombreUsuario, string nombreCompleto);
        Task<object> GetAccesosAsync(int usuarioId);
    }
}
