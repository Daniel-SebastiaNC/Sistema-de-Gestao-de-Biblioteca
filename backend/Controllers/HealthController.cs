using DataContext;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly BibliotecaContext _contextDb;
    private readonly ICacheService? _cacheService;

    public HealthController(
        BibliotecaContext contextDb,
        ICacheService? cacheService = null)
    {
        _contextDb = contextDb;
        _cacheService = cacheService;
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

        bool redisHealthy = true;
        if (_cacheService != null)
        {
            try
            {
                const string testKey = "health:ping";
                await _cacheService.SetAsync(testKey, "pong", TimeSpan.FromSeconds(30));
                var testVal = await _cacheService.GetAsync<string>(testKey);
                redisHealthy = testVal == "pong";
                services["Redis"] = redisHealthy ? "Healthy" : "Unhealthy (valor retornado incorreto)";
            }
            catch (Exception ex)
            {
                redisHealthy = false;
                services["Redis"] = $"Unhealthy ({ex.Message})";
            }
        }
        else
        {
            services["Redis"] = "NotConfigured";
        }

        var overallStatus = (dbHealthy && redisHealthy) ? "Healthy" : "Degraded";

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
