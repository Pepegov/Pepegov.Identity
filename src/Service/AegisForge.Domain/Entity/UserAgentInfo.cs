namespace AegisForge.Domain.Entity;

public class UserAgentInfo
{
    public Guid Id { get; init; }
    public string UserAgent { get; init; } = null!;
    public string Browser { get; init; } = null!;
    public string Os { get; init; } = null!;
    public string Device { get; init; } = null!;
}