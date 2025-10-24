namespace Pepegov.Identity.BL.Services.Interfaces;

/// <summary>
/// Service for managing user tokens and authorizations
/// </summary>
public interface ITokenManagementService
{
    /// <summary>
    /// Revokes all tokens and authorizations for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all tokens and authorizations for a specific application
    /// </summary>
    /// <param name="applicationId">Application id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task RevokeApplicationTokensAsync(string applicationId, CancellationToken cancellationToken = default);
}
