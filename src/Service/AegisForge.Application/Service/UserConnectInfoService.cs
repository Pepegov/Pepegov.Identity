using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AegisForge.Application.Dto;
using AegisForge.Application.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Wangkanai.Detection.Services;

namespace AegisForge.Application.Service;

public class UserConnectInfoService : IUserConnectInfoService
{
    private HttpContext? _httpContext;
    private readonly IDetectionService _detectionService;
    private readonly ILogger<UserConnectInfoService> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public UserConnectInfoService(IHttpContextAccessor httpContextAccessor, IDetectionService detectionService, ILogger<UserConnectInfoService> logger, IHttpClientFactory clientFactory)
    {
        if(httpContextAccessor.HttpContext is not null)
            _httpContext = httpContextAccessor.HttpContext;
        _detectionService = detectionService;
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public void SetHttpContext(HttpContext httpContext)
    {
        _httpContext = httpContext;
    }
    
    public string GetClientIp()
    {
        ArgumentNullException.ThrowIfNull(_httpContext);
        
        var ip = _httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
        {
            ip = _httpContext.Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
        }

        if (ip.Contains(','))
        {
            ip = ip.Split(',')[0];
        }

        return ip;
    }

    public Task<GeoDataDto?> GetLocationAsync() => GetLocationAsync(GetClientIp());
    
    public async Task<GeoDataDto?> GetLocationAsync(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);
        if (ip is "" or "::1" or "127.0.0.1")
        {
            return null;
        }

        using var httpClient = _clientFactory.CreateClient("IpApiClient");
        try
        {
            var response = (await httpClient.GetFromJsonAsync<IpApiResponse>(ip))!;
            if (response.Status == "fail")
            {
                _logger.LogError("Cannot get geo info by ip {query}", response.Query);
                return null;
            }
            
            return new GeoDataDto()
            {
                City = response.City!,
                Country = response.Country!,
                CountryCode = response.CountryCode!,
                Region = response.Region!,
            };
        }
        catch (Exception e)
        {
            _logger.LogCritical("It isn't possible to get geo data by ip address. Error: {errorMessage}", e.Message);
            return null;
        }
    }

    public UserAgentDataDto GetUserAgentInfo()
    {
        ArgumentNullException.ThrowIfNull(_httpContext);
        
        var userAgent = _httpContext.Request.Headers["User-Agent"].ToString();
    
        var browser = $"{_detectionService.Browser.Name} {_detectionService.Platform.Version}";
        var os = $"{_detectionService.Platform.Name} {_detectionService.Platform.Version}";
        var device = _detectionService.Device.Type.ToString();
    
        return new UserAgentDataDto(){
            UserAgent = userAgent,
            Browser = browser,
            Os = os,
            Device = device
        };
        
    }

    public async Task<UserConnectionInfoDto> GetUserConnectionInfoAsync()
    {
        var userAgentData = GetUserAgentInfo();
        var ip = GetClientIp();
        var geoData = await GetLocationAsync(ip);

        return new UserConnectionInfoDto()
        {
            Ip = ip,
            GeoData = geoData,
            UserAgent = userAgentData
        };
    }

    class IpApiResponse
    {
        [JsonPropertyName("status")] 
        public string Status { get; set; } = null!;
    
        [JsonPropertyName("country")]
        public string? Country { get; set; }
    
        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    
        [JsonPropertyName("region")]
        public string? Region { get; set; }
    
        [JsonPropertyName("regionName")]
        public string? RegionName { get; set; }
    
        [JsonPropertyName("city")]
        public string? City { get; set; }
    
        [JsonPropertyName("zip")]
        public string? Zip { get; set; }
    
        [JsonPropertyName("lat")]
        public double? Lat { get; set; }
    
        [JsonPropertyName("lon")]
        public double? Lon { get; set; }
    
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    
        [JsonPropertyName("isp")]
        public string? Isp { get; set; }
    
        [JsonPropertyName("org")]
        public string? Org { get; set; }
    
        [JsonPropertyName("as")]
        public string? As { get; set; }
    
        [JsonPropertyName("query")]
        public string? Query { get; set; }
    }
}