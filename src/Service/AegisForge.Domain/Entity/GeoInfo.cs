namespace AegisForge.Domain.Entity;

public class GeoInfo
{
    public Guid Id { get; init; }
    public string Country { get; init; } = null!;
    public string CountryCode { get; init; } = null!;
    public string Region { get; init; } = null!;
    public string City { get; init; } = null!;
}