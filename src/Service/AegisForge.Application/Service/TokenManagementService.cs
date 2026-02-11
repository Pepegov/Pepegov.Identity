using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.Services.Interfaces;

namespace Pepegov.Identity.BL.Services;

/// <inheritdoc/>
public class TokenManagementService(
    IOpenIddictTokenManager tokenManager,
    ILogger<TokenManagementService> logger)
    : ITokenManagementService
{
    /// <inheritdoc/>
    public async Task DeleteUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("Attempted to revoke tokens for null or empty userId");
            return;
        }
        
        var tokens = new List<object>();
        await foreach (var token in tokenManager.FindBySubjectAsync(userId, cancellationToken))
        {
            // Check if this token belongs to our authorization
            await tokenManager.GetAuthorizationIdAsync(token, cancellationToken);
            tokens.Add(token);
        }

        // Delete all tokens for this authorization
        foreach (var token in tokens)
        {
            try
            {
                await tokenManager.UpdateAsync(token, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete token for user: {UserId}", userId);
            }
        }
        
        logger.LogInformation("Delete {Count} tokens for user: {UserId}", tokens.Count, userId);
    }
    
    /// <inheritdoc/>
    public async Task DeleteApplicationTokensAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(applicationId))
        {
            logger.LogWarning("Attempted to revoke tokens for null or empty applicationId");
            return;
        }
        
        var tokens = new List<object>();
        await foreach (var token in tokenManager.FindByApplicationIdAsync(applicationId, cancellationToken))
        {
            // Check if this token belongs to our authorization
            await tokenManager.GetAuthorizationIdAsync(token, cancellationToken);
            tokens.Add(token);
        }

        // Delete all tokens for this authorization
        foreach (var token in tokens)
        {
            try
            {
                await tokenManager.DeleteAsync(token, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete token for application: {ApplicationId}", applicationId);
            }
        }
        
        logger.LogInformation("Delete {Count} tokens for application: {applicationId}", tokens.Count, applicationId);
    }
}