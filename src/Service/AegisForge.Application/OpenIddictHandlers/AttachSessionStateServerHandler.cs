using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AegisForge.Application.Dto;
using AegisForge.Application.Service;
using AegisForge.Application.Service.Interfaces;
using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Entity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Pepegov.Identity.BL.OpenIddictHandlers;
using Pepegov.MicroserviceFramework.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.OpenIddictHandlers;

/// <summary>
/// A handler that is triggered when a response to an authorization request is generated, for example, when an authorization code or token is successfully issued (/connect/authorize endpoint).
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class AttachSessionStateServerHandler(
    ILogger<AttachSessionStateServerHandler> logger,
    IServiceProvider serviceProvider)
    : IOpenIddictServerHandler<OpenIddictServerEvents.ApplyAuthorizationResponseContext>
{
    public async ValueTask HandleAsync(OpenIddictServerEvents.ApplyAuthorizationResponseContext context)
    {
        var httpRequest = context.Transaction.GetHttpRequest();
        
        // If request has a clientId, an absolute redirectUri and a sessionId in the Cookie  
        if (context.Request?.ClientId is { } clientId
            && Uri.TryCreate(context.Request.RedirectUri, UriKind.Absolute, out var redirectUri)
            && httpRequest?.GetSessionId() is { } sessionId)
        {
            var origin = redirectUri.GetLeftPart(UriPartial.Authority);
            var salt = RandomNumberGenerator.GetHexString(8);
            
            //Generating data for a hash
            var utf8Bytes = Encoding.UTF8.GetBytes(clientId + origin + sessionId + salt);
            var hashBytes = SHA256.HashData(utf8Bytes);
            var hashBase64Url = Base64UrlTextEncoder.Encode(hashBytes);
            
            // Note: The session_state parameter is used so that the frontend client (an SPA or another browser client)
            // can track whether the user's session has changed on the server (for example, if the user has logged out).
            var sessionState = hashBase64Url + "." + salt;
            context.Response.SetParameter("session_state", sessionState);

            //Because the handler is singleton
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            var userConnectInfoService = scope.ServiceProvider.GetRequiredService<IUserConnectInfoService>();

            //Getting data from httpContext
            var contextData = await GetDataFromHttpContext(userConnectInfoService, httpRequest.HttpContext);
            if (!contextData.HasValue)
                return;
                 
            //Save session
            await SaveSessionOnDbAsync(unitOfWorkManager, sessionId, sessionState, clientId, origin, salt, contextData.Value.Item1, contextData.Value.Item2, httpRequest.HttpContext.RequestAborted);
        }
    }

    private async Task<(Guid, UserConnectionInfoDto)?> GetDataFromHttpContext(IUserConnectInfoService userConnectInfoService, HttpContext httpContext)
    {
        // Injecting UserId from user auth
        var sub = httpContext.User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (sub is null || !Guid.TryParse(sub, out var userId))
        {
            logger.LogError("Cannot find {claimName} in request claims", nameof(ClaimTypes.NameIdentifier));
            return null;
        }

        // Getting all required user connection info from http connect
        (userConnectInfoService as UserConnectInfoService)!.SetHttpContext(httpContext);
        var userConnectionInfoResult = await userConnectInfoService.GetUserConnectionInfoAsync();   
        
        return new ValueTuple<Guid, UserConnectionInfoDto>(userId, userConnectionInfoResult);
    }

    private async Task SaveSessionOnDbAsync(IUnitOfWorkManager unitOfWorkManager, string sessionId, string sessionState, string clientId, string origin, string salt, Guid userId, UserConnectionInfoDto userConnectionInfoResult, CancellationToken cancellationToken = default)
    {
        var userConnectionInfo = new UserConnectionInfo()
        {
            Ip = userConnectionInfoResult.Ip,
            UserAgent = new UserAgentInfo()
            {
                Browser = userConnectionInfoResult.UserAgent.Browser,
                Device = userConnectionInfoResult.UserAgent.Device,
                UserAgent = userConnectionInfoResult.UserAgent.UserAgent,
                Os = userConnectionInfoResult.UserAgent.Os,
            }
        };
        if (userConnectionInfoResult.GeoData is not null)
        {
            userConnectionInfo.GeoData = new GeoInfo()
            {
                City = userConnectionInfoResult.GeoData.City,
                Country = userConnectionInfoResult.GeoData.Country,
                CountryCode = userConnectionInfoResult.GeoData.CountryCode,
                Region = userConnectionInfoResult.GeoData.Region,
            };
        }
        
        //Saving session in db
        var unitOfWorkEntityFrameworkInstance = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        var sessionRepository = unitOfWorkEntityFrameworkInstance.GetRepository<ApplicationSession>();

        if(!await sessionRepository.ExistsAsync(selector: x => x.SessionId == sessionId, cancellationToken: cancellationToken))
        {
            var currentSession = await sessionRepository.GetFirstOrDefaultAsync(
                predicate: x => x.ApplicationUserId == userId &&
                                x.ClientId == clientId &&
                                x.SessionStatusType == SessionStatusType.Active &&
                                x.UserConnectionInfo.Ip == userConnectionInfoResult.Ip &&
                                x.UserConnectionInfo.UserAgent.UserAgent == userConnectionInfoResult.UserAgent.UserAgent,
                include: x => x.Include(x => x.UserConnectionInfo).ThenInclude(x => x.UserAgent),
                cancellationToken: cancellationToken);

            if (currentSession != null)
            {
                currentSession.Update(sessionId, sessionState, origin, salt);
                sessionRepository.Update(currentSession);
            }
            else
            {
                var newSession = new ApplicationSession(sessionId, sessionState, origin, salt, clientId, userId, userConnectionInfo);
                await sessionRepository.InsertAsync(newSession, cancellationToken);
            }
            
            await unitOfWorkEntityFrameworkInstance.SaveChangesAsync(cancellationToken);
            if (!unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.IsOk)
            {
                var exceptionMessage = $"Unable to save changes to database | errors: " +
                                       (unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception?.Message ??
                                        unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception?.InnerException?.Message ??
                                        "Unable to save changes to database");
                logger.LogError(exceptionMessage);
                throw new MicroserviceDatabaseException(exceptionMessage);
            }   
        }
    }
}