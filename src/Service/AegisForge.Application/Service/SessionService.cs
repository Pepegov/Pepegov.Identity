using System.Linq.Expressions;
using AegisForge.Application.Service.Interfaces;
using AegisForge.Domain.Aggregate;
using AegisForge.Infrastructure.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.MicroserviceFramework.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;
using Pepegov.UnitOfWork.EntityFramework.Repository;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.Service;

public class SessionService : ISessionService
{
    private const string SessionCachePrefix = "session_";
    private static string GetSessionCacheKey(string sessionId) => $"{SessionCachePrefix}{sessionId}";
    public static TimeSpan CacheDuration => TimeSpan.FromMinutes(4);
    
    
    private readonly IUnitOfWorkEntityFrameworkInstance _unitOfWorkEntityFrameworkInstance;
    private readonly IRepositoryEntityFramework<ApplicationSession> _sessionRepository;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IUnitOfWorkManager unitOfWorkManager, IHttpContextAccessor httpContextAccessor, IMemoryCache cache, ILogger<SessionService> logger)
    {
        _unitOfWorkEntityFrameworkInstance = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        _sessionRepository = _unitOfWorkEntityFrameworkInstance.GetRepository<ApplicationSession>();
        _cache = cache;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool?> IsSessionTerminated(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_httpContextAccessor.HttpContext);
        _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(SessionIdExtensions.CookieName, out var sessionId);
        
        return sessionId == null ? null: await IsSessionTerminated(sessionId, cancellationToken: cancellationToken);
    }

    public async Task<bool?> IsSessionTerminated(string sessionId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var session = await GetSession(sessionId, cancellationToken: cancellationToken);
        if (session is null)
            return null;

        return session is not { SessionStatusType: SessionStatusType.Active };
    }
    
    public async Task<bool?> IsSessionTerminated(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var session = await GetSession(id, userId, cancellationToken);
        if(session is null)
            return null;
        
        return session is not { SessionStatusType: SessionStatusType.Active };
    }

    public async Task<ApplicationSession?> GetSession(string sessionId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue<ApplicationSession>(GetSessionCacheKey(sessionId), out var session);
        
        if (userId is not null && session?.ApplicationUserId != userId)
            return null;
        if (session is not null) 
            return session;
        
        Expression<Func<ApplicationSession, bool>> predicate = x => x.SessionId == sessionId;
        if (userId != null)
        {
            predicate.AndAlso(x => x.ApplicationUserId == userId);
        }
        
        session = await _sessionRepository.GetFirstOrDefaultAsync(predicate: predicate, cancellationToken: cancellationToken);
        _cache.Set(GetSessionCacheKey(sessionId), session, CacheDuration);

        return session;
    }

    public async Task<ApplicationSession?> GetSession(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        Expression<Func<ApplicationSession, bool>> predicate = x => x.Id == id;
        if (userId != null)
        {
            predicate.AndAlso(x => x.ApplicationUserId == userId);
        }
        
        return await _sessionRepository.GetFirstOrDefaultAsync(predicate: predicate, cancellationToken: cancellationToken);
    }

    public async Task TerminateSession(string sessionId, Guid? userId = null, CancellationToken cancellationToken = default)
        => await TerminateSession(await GetSession(sessionId, userId, cancellationToken: cancellationToken), cancellationToken);

    public async Task TerminateSession(Guid id, Guid? userId = null,  CancellationToken cancellationToken = default)
        => await TerminateSession(await GetSession(id, userId, cancellationToken), cancellationToken);

    public async Task TerminateSession(ApplicationSession? session, CancellationToken cancellationToken = default)
    {
        if (session is null)
        {
            throw new MicroserviceNotFoundException("Session not found");
        }
        
        session.SessionStatusType = SessionStatusType.Revoked;
        _sessionRepository.Update(session);
        await _unitOfWorkEntityFrameworkInstance.SaveChangesAsync(cancellationToken);
        if (!_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.IsOk)
        {
            var exceptionMessage = $"Unable to save changes to database | errors: " +
                                   (_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception?.Message ??
                                    _unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception?.InnerException?.Message ??
                                    "Unable to save changes to database");
            _logger.LogError(exceptionMessage);
            throw new MicroserviceDatabaseException(exceptionMessage);
        }   
        
        _cache.Set(GetSessionCacheKey(session.SessionId), session, CacheDuration);
    }
}