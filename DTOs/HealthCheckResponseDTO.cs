namespace DTO;

public class HealthCheckResponseDTO
{
    public string Status { get; set; } = "Healthy";
    public Dictionary<string, string> Services { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
