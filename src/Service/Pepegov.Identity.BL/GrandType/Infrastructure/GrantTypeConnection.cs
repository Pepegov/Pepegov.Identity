using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Pepegov.Identity.BL.GrandType.Model;

namespace Pepegov.Identity.BL.GrandType.Infrastructure;

public interface IGrantTypeConnection
{
    Task<IResult> SignInAsync(AuthorizationContext context);
    Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthorizationContext context);
}