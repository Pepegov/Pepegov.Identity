namespace AegisForge.Application.Dto;

public class UserConnectionInfoDto
{
    public string Ip { get; init; } = null!;
    public GeoDataDto? GeoData { get; init; }
    public UserAgentDataDto UserAgent { get; init; } = null!;
}