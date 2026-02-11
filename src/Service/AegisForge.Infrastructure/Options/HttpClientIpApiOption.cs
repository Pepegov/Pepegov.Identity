namespace AegisForge.Infrastructure.Options;

public class HttpClientIpApiOption
{
    public string BaseUrl { get; set; } = null!;
    public long TimeoutFromMicroseconds { get; set; } 
}