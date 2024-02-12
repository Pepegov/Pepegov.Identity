namespace Pepegov.Identity.PL.Definitions.OpenIddict.Options;

public class IdentityAddressOption
{
    public string Authority { get; set; } = null!;
    public string? Audience { get; set; }
}