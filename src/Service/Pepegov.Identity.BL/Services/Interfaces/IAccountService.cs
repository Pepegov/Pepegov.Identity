using System.Security.Claims;
using Pepegov.Identity.DAL.ViewModel;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace Pepegov.Identity.BL.Services.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<UserAccountViewModel>> GetUsersByEmailsAsync(IEnumerable<string> emails);
    Guid GetCurrentUserId();

    Task RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken);
    /// <summary>
    /// Get user view model by account id
    /// </summary>
    /// <param name="id">Account id</param>
    /// <exception cref="MicroserviceNotFoundException"></exception>
    Task<UserAccountViewModel> GetByIdAsync(Guid id);
    Task<ClaimsPrincipal> GetPrincipalByIdAsync(string identifier);
    Task<UserAccountViewModel> GetCurrentUserAsync();
    Task<IEnumerable<UserAccountViewModel>> GetUsersInRoleAsync(string roleName);
}