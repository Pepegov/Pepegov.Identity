using System.Security.Claims;
using AegisForge.Application.Dto;
using Pepegov.MicroserviceFramework.Data.Exceptions;

namespace AegisForge.Application.Service.Interfaces;

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