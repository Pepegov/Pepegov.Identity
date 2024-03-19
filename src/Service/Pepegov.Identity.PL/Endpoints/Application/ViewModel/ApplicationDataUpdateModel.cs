namespace Pepegov.Identity.PL.Endpoints.Application.ViewModel;

public class ApplicationDataUpdateModel
{
    public string? ClientId { get; set; }
    public string? ConsentType { get; set; }
    public string? DisplayName { get; set; }
    public List<string>? RedirectUris { get; set; }
    public string? Type { get; set; }
}