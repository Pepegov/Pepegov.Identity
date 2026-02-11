using Microsoft.AspNetCore.Identity;

namespace AegisForge.Domain.Entity;

/// <summary>
/// Application role
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
