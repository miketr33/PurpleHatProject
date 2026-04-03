namespace PurpleHatProject.Data;

public class HealthCheckEntry
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
