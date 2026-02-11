using System.Security.Claims;
using AegisForge.Application.GrandType.Model;
using Microsoft.AspNetCore.Http;

namespace AegisForge.Application.GrandType.Infrastructure;

public interface IGrantTypeConnection
{
    Task<IResult> SignInAsync(AuthorizationContext context);
    Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context);
}