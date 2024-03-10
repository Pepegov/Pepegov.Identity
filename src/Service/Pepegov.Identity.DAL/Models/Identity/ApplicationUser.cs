using Microsoft.AspNetCore.Identity;

namespace Pepegov.Identity.DAL.Models.Identity;

/// <summary>
/// Default user for application.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// FirstName
    /// </summary>
    public string FirstName { get; set; } = null!;
    
    /// <summary>
    /// LastName
    /// </summary>
    public string LastName { get; set; } = null!;
    
    /// <summary>
    /// MiddleName
    /// </summary>
    public string? MiddleName { get; set; }
    
    /// <summary>
    /// BirthDate
    /// </summary>
    public DateTime BirthDate { get; set; }
    
    /// <summary>
    /// Gender
    /// </summary>
    public UserGender? Gender { get; set; }
    
    /// <summary>
    /// User Profile
    /// </summary>
    public virtual ApplicationUserProfile? ApplicationUserProfile { get; set; }
    
    /// <summary>
    /// User Profile Id
    /// </summary>
    public virtual Guid? ApplicationUserProfileId { get; set; }
}
