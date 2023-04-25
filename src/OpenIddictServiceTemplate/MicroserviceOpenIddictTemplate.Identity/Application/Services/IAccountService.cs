using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;

namespace MicroserviceOpenIddictTemplate.Identity.Application.Services;

public interface IAccountService
{
    Task<IEnumerable<ApplicationUser>> GetUsersByEmailsAsync(IEnumerable<string> emails);
    Guid GetCurrentUserId();

    Task<UserAccountViewModel> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken);
    Task<ApplicationUser> GetByIdAsync(Guid id);
    Task<ClaimsPrincipal> GetPrincipalByIdAsync(string identifier);
    Task<ClaimsPrincipal> GetPrincipalForUserAsync(ApplicationUser user);
    Task<ApplicationUser> GetCurrentUserAsync();
    Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName);
}