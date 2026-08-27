using DataContext;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly BibliotecaContext _contextDb;

    public HealthController(BibliotecaContext contextDb)
    {
        _contextDb = contextDb;
    }

    [HttpGet]
    public async Task<ActionResult<HealthCheckResponseDTO>> CheckHealth()
    {
        var services = new Dictionary<string, string>
        {
            { "API", "Healthy" }
        };

        bool dbHealthy;
        try
        {
            dbHealthy = await _contextDb.Database.CanConnectAsync();
            services["PostgreSQL"] = dbHealthy ? "Healthy" : "Unhealthy";
        }
        catch (Exception ex)
        {
            dbHealthy = false;
            services["PostgreSQL"] = $"Unhealthy ({ex.Message})";
        }

        var overallStatus = dbHealthy ? "Healthy" : "Unhealthy";

        var response = new HealthCheckResponseDTO
        {
            Status = overallStatus,
            Services = services,
            Timestamp = DateTime.UtcNow
        };

        if (overallStatus == "Healthy")
        {
            return Ok(response);
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
