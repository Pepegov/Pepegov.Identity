using Microsoft.AspNetCore.Identity;

namespace Pepegov.Identity.DAL.Models.Identity;

/// <summary>
/// Application role
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
