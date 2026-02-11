using System.Reflection;
using AegisForge.Domain.Aggregate;
using AegisForge.Domain.Entity;
using AegisForge.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AegisForge.Domain.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    
    public DbSet<UserAgentInfo> UserAgentInfos { get; set; }
    public DbSet<UserConnectionInfo> UserConnectionInfos { get; set; }
    public DbSet<GeoInfo> GeoInfos { get; set; }
    public DbSet<ApplicationSession> Sessions { get; set; }
    public DbSet<ApplicationUserProfile> Profiles { get; set; }
    public DbSet<ApplicationPermission> Permissions { get; set; }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseOpenIddict<Guid>();
        base.OnModelCreating(builder);

        var applyGenericMethod = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(x => x.Name == "ApplyConfiguration");
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
        {
            foreach (var item in type.GetInterfaces())
            {
                if (!item.IsConstructedGenericType || item.GetGenericTypeDefinition() != typeof(IEntityTypeConfiguration<>))
                {
                    continue;
                }

                var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(item.GenericTypeArguments[0]);
                applyConcreteMethod.Invoke(builder, [Activator.CreateInstance(type)]);
                break;
            }
        }
    }
}

/*public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.("Server=<SQL>;Database=<DatabaseName>;User ID=<UserName>;Password=<Password>");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}*/