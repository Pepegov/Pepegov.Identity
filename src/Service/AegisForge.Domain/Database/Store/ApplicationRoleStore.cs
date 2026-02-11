using AegisForge.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AegisForge.Domain.Database.Store;

/// <summary>
/// Application store for user
/// </summary>
public class ApplicationRoleStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null)
    : RoleStore<ApplicationRole, ApplicationDbContext, Guid>(context,
        describer);