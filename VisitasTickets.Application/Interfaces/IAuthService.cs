namespace VisitasTickets.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string username, string password);
        Task<object> GetAccesosAsync(int usuarioId);
    }
}
