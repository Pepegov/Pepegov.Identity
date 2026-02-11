using System.Text.Json.Serialization;
using AegisForge.Domain.Models.Identity;

namespace AegisForge.Domain.Entity;

/// <summary>
/// User permission for application
/// </summary>
public class ApplicationPermission
{
    /// <summary>
    /// Application Permission indentifer
    /// </summary>
    public Guid ApplicationPermissionId { get; set; }

    /// <summary>
    /// Application User Profile
    /// </summary>
    [JsonIgnore]
    public virtual List<ApplicationUserProfile>? ApplicationUserProfiles { get; set; }

    /// <summary>
    /// Authorize attribute policy name
    /// </summary>
    public string PolicyName { get; set; } = null!;

    /// <summary>
    /// Description for current permission
    /// </summary>
    public string Description { get; set; } = null!;
}