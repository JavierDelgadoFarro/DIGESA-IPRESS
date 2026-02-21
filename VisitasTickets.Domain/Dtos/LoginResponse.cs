namespace VisitasTickets.Domain.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public bool RequiereCambio { get; set; }
        public int UsuarioId { get; set; }
        public string Message { get; set; }
    }
}
