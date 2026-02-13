using AegisForge.Domain.Aggregate;

namespace AegisForge.Application.Service.Interfaces;

public interface ISessionService
{
    public Task<bool?> IsSessionTerminated(CancellationToken cancellationToken = default);
    public Task<bool?> IsSessionTerminated(string sessionId, Guid? userId = null, CancellationToken cancellationToken = default);
    public Task<bool?> IsSessionTerminated(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    public Task<ApplicationSession?> GetSession(string sessionId, Guid? userId = null, CancellationToken cancellationToken = default);
    public Task<ApplicationSession?> GetSession(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    public Task TerminateSession(string sessionId, Guid? userId = null, CancellationToken cancellationToken = default);
    public Task TerminateSession(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    public Task TerminateSession(ApplicationSession? session, CancellationToken cancellationToken = default);
}