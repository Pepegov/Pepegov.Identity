using System.ComponentModel.DataAnnotations;
using AegisForge.Domain.Entity;
using SessionStatusType = AegisForge.Domain.Enum.SessionStatusType;

namespace AegisForge.Domain.Aggregate;

public class ApplicationSession
{
    /// <summary>
    /// Ef constructor
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ApplicationSession() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    
    public ApplicationSession(
        string sessionId, 
        string sessionState, 
        string origin, 
        string salt,
        string clientId, 
        Guid userId, 
        UserConnectionInfo userConnectionInfo, 
        SessionStatusType sessionStatusType = SessionStatusType.Active)
    {
        SessionId = sessionId;
        SessionState = sessionState;
        ClientId = clientId;
        Origin = origin;
        Salt = salt;
        ApplicationUserId = userId;
        UserConnectionInfo = userConnectionInfo;
        SessionStatusType = sessionStatusType;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; set; }
    /// <summary>
    /// Session Id
    /// </summary>
    public string SessionId { get; private set; }
    /// <summary>
    /// Client ID
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    /// Origin that request session
    /// </summary>
    public string Origin { get; private set; } = null!;
    /// <summary>
    /// Salt that has in session_state 
    /// </summary>
    public string Salt { get; private set; } = null!;
    /// <summary>
    /// Session State Value
    /// </summary>
    public string SessionState { get; private set; }
    /// <summary>
    /// User ID
    /// </summary>
    public Guid ApplicationUserId { get; private set; }
    public ApplicationUser ApplicationUser { get; private set; } = null!;

    public UserConnectionInfo UserConnectionInfo { get; private set; }

    /// <summary>
    /// State of session
    /// </summary>
    public SessionStatusType SessionStatusType { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public void Update(string sessionId, string sessionState, string origin, string salt)
    {
        SessionId = sessionId;
        SessionState = sessionState;
        Origin = origin;
        Salt = salt;
        UpdatedAt = DateTime.UtcNow;
    }
}