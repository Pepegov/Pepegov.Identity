using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Application.Services;

public interface IAccountService
{
    Task<IEnumerable<ApplicationUser>> GetUsersByEmailsAsync(IEnumerable<string> emails);
    Guid GetCurrentUserId();

    Task<ResultWrapper<UserAccountViewModel>> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken);
    Task<ResultWrapper<ApplicationUser>> GetByIdAsync(Guid id);
    Task<ClaimsPrincipal> GetPrincipalByIdAsync(string identifier);
    Task<ClaimsPrincipal> GetPrincipalForUserAsync(ApplicationUser user);
    Task<ApplicationUser> GetCurrentUserAsync();
    Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName);
}