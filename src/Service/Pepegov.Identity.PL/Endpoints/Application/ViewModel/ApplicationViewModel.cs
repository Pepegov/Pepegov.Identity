using System.Diagnostics.CodeAnalysis;

namespace Pepegov.Identity.PL.Endpoints.Application.ViewModel;

public class ApplicationViewModel
{
    /// <summary>
    /// Unique identifier associated with the current application.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Client identifier associated with the current application.
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Client secret associated with the current application.
    /// Note: depending on the application manager used to create this instance,
    /// this property may be hashed or encrypted for security reasons.
    /// </summary>
    public string? ClientSecret { get; set; }
    
    /// <summary>
    /// Consent type associated with the current application.
    /// </summary>
    public string? ConsentType { get; set; }
    
    /// <summary>
    /// Display name associated with the current application.
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Gets or sets the permissions associated with the
    /// current application, serialized as a JSON array.
    /// </summary>
    public PermissionsViewModel? Permissions { get; set; }
    
    /// <summary>
    /// Redirect URIs associated with the
    /// current application, serialized as a JSON array.
    /// </summary>
    public List<string>? RedirectUris { get; set; }
    
    /// <summary>
    /// Application type associated with the current application.
    /// </summary>
    public string? Type { get; set; }
}