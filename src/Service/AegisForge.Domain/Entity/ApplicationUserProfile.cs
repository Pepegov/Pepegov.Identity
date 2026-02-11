using System.Text.Json.Serialization;
using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Entity;

namespace AegisForge.Domain.Models.Identity;

public class ApplicationUserProfile
{
    /// <summary>
    /// Profile Id
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Account
    /// </summary>
    [JsonIgnore]
    public virtual ApplicationUser? ApplicationUser { get; set; }

    /// <summary>
    /// Field creation date
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// Field update date
    /// </summary>
    public DateTime Updated { get; set; }
    
    /// <summary>
    /// Application permission for policy-based authorization
    /// </summary>
    public List<ApplicationPermission>? Permissions { get; set; }
}