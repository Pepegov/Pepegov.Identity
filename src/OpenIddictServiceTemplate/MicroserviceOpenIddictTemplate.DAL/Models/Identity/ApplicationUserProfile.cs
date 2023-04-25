namespace MicroserviceOpenIddictTemplate.DAL.Models.Identity;

public class ApplicationUserProfile
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Account
    /// </summary>
    public virtual ApplicationUser? ApplicationUser { get; set; }

    /// <summary>
    /// Application permission for policy-based authorization
    /// </summary>
    public List<ApplicationPermission>? Permissions { get; set; }
}