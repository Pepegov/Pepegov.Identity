namespace MicroserviceOpenIddictTemplate.DAL.Models.Identity;

public class ApplicationPermission
{
    /// <summary>
    /// Application Permission indentifer
    /// </summary>
    public Guid ApplicationPermissionId { get; set; }
    /// <summary>
    /// Application User profile identifier
    /// </summary>
    public Guid ApplicationUserProfileId { get; set; }

    /// <summary>
    /// Application User Profile
    /// </summary>
    public virtual ApplicationUserProfile? ApplicationUserProfile { get; set; }

    /// <summary>
    /// Authorize attribute policy name
    /// </summary>
    public string PolicyName { get; set; } = null!;

    /// <summary>
    /// Description for current permission
    /// </summary>
    public string Description { get; set; } = null!;
}