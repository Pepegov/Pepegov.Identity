using System.Security.Claims;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.DAL.ViewModel;

namespace MicroserviceOpenIddictTemplate.BL.Services.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<UserAccountViewModel>> GetUsersByEmailsAsync(IEnumerable<string> emails);
    Guid GetCurrentUserId();

    Task RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken);
    Task<UserAccountViewModel> GetByIdAsync(Guid id);
    Task<ClaimsPrincipal> GetPrincipalByIdAsync(string identifier);
    Task<UserAccountViewModel> GetCurrentUserAsync();
    Task<IEnumerable<UserAccountViewModel>> GetUsersInRoleAsync(string roleName);
}