using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AegisForge.Domain.Database.Store;

/// <summary>
/// Application store for user
/// </summary>
public class ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null)
    : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>(context,
        describer)
{
    public override Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        => Users
            .Include(x => x.ApplicationUserProfile).ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken: cancellationToken)!;

    public override Task<ApplicationUser?> FindByNameAsync(string normalizedUserName,
        CancellationToken cancellationToken = default)
        => Users
            .Include(x => x.ApplicationUserProfile).ThenInclude(x => x.Permissions)
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName,
                cancellationToken: cancellationToken)!;
}