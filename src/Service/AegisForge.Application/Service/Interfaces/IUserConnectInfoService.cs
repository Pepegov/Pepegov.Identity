using AegisForge.Application.Dto;

namespace AegisForge.Application.Service.Interfaces;

public interface IUserConnectInfoService
{
    string GetClientIp();

    Task<GeoDataDto?> GetLocationAsync();
    Task<GeoDataDto?> GetLocationAsync(string ip);

    UserAgentDataDto GetUserAgentInfo();
    
    Task<UserConnectionInfoDto> GetUserConnectionInfoAsync();
}