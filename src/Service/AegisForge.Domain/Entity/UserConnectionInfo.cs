namespace AegisForge.Domain.Entity;

public class UserConnectionInfo
{
    public Guid Id { get; set; }
    public string Ip { get; init; } = null!;
    public GeoInfo? GeoData { get; set; }
    public UserAgentInfo UserAgent { get; init; } = null!;
}