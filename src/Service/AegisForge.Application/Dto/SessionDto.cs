using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.Dto;

public class SessionDto
{
    /// <summary>
    /// Session State Value
    /// </summary>
    public required string SessionId { get; set; } 
    /// <summary>
    /// Client ID
    /// </summary>
    public required string ClientId { get; set; }
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// User Agent
    /// </summary>
    public required string UserAgent { get; set; }
    /// <summary>
    /// Ip Address
    /// </summary>
    public string? IpAddress { get; set; }
    /// <summary>
    /// State of session
    /// </summary>
    public SessionStatusType SessionStatus { get; set; }   
}