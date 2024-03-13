using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Handlers.Queries;

public class AddApplicationCommand : IRequest<ApiResult>
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ConsentType { get; set; } = null!;
    public List<string>? Scopes { get; set; }
    public List<string>? GrandTypes { get; set; } 
    public List<string>? RedirectUris { get; set; }
}