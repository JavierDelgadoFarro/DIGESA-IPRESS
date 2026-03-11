using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPRESS.Infrastructure.Persistence;

namespace IPRESS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IpressDbContext _db;

        public HealthController(IpressDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

        /// <summary>Comprueba que la API responde y que la conexión a la base de datos funciona.</summary>
        [HttpGet("db")]
        public async Task<IActionResult> GetDb()
        {
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();
                return Ok(new { status = "healthy", database = canConnect ? "connected" : "error", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "unhealthy", database = "error", message = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}
