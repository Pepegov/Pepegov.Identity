namespace Pepegov.Identity.DAL.Models.Options;

public class IdentityAddressOption
{
    public string Authority { get; set; } = null!;
    public string? Audience { get; set; }
}