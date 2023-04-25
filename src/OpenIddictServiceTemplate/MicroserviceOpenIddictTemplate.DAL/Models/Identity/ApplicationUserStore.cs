using MicroserviceOpenIddictTemplate.DAL.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceOpenIddictTemplate.DAL.Models.Identity;

public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>
{
    public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
    {
    }

    public override Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        => Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken: cancellationToken)!;

    public override Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        => Users
            .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken: cancellationToken)!;
}
