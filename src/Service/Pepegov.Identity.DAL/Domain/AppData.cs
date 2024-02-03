namespace Pepegov.Identity.DAL.Domain;

/// <summary>
/// Data of Microservice
/// </summary>
public static class AppData
{
    /// <summary>
    /// Current service name
    /// </summary>
    public const string ServiceName = "Pepegov.Identity OpenIddict service";

    /// <summary>
    /// Current service description
    /// </summary>
    public const string ServiceDescription = "OpenIddict service with OpenID Connect server and Token Validation";
    
    /// <summary>
    /// Current service version
    /// </summary>
    public const string ServiceVersion = "f7v1.0.0";
    
    /// <summary>
    /// Name of cors police
    /// </summary>
    public const string PolicyName = "CorsPolicy";
    
    /// <summary>
    /// Path to identitysetting.json
    /// </summary>
    public const string IdentitySettingPath = "identitysetting.json";
}