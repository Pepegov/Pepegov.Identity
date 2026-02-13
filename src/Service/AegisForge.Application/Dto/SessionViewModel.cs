using AegisForge.Domain.Enum;

namespace AegisForge.Application.Dto;

public class SessionViewModel
{
    public Guid Id { get; set; }
    public string SessionId { get; set; }
    public SessionStatusType StatusType { get; set; }
    public UserConnectionInfoDto UserConnectionInfo { get; set; }
}