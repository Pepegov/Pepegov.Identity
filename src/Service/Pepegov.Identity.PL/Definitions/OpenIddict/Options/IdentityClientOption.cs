namespace Pepegov.Identity.PL.Definitions.OpenIddict.Options;

public class IdentityClientOption
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Secret { get; set; } = null!;
    public string? ConsentType { get; set; }
    public List<string>? GrandTypes { get; set; }
    public List<string>? RedirectUris { get; set; }
    public List<string>? Scopes { get; set; }
}