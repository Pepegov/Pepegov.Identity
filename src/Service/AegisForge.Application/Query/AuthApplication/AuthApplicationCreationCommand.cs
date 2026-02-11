using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.AuthApplication;

public class AuthApplicationCreationCommand : IRequest<ApiResult>
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ConsentType { get; set; } = null!;
    public List<string>? Scopes { get; set; }
    public List<string>? GrandTypes { get; set; } 
    public List<string>? RedirectUris { get; set; }
}