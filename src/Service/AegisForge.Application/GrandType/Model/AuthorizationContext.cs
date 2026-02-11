using AegisForge.Domain.Aggregate;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;

namespace AegisForge.Application.GrandType.Model;

public class AuthorizationContext
{
    public HttpContext? HttpContext { get; set; } = null!;
    public OpenIddictRequest? OpenIddictRequest { get; set; } = null!;
    public OpenIddictApplication? OpenIddictApplication { get; set; } = null!;
    public List<object> Authorizations { get; set; } = null!;
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public CancellationToken CancellationToken = default;
}