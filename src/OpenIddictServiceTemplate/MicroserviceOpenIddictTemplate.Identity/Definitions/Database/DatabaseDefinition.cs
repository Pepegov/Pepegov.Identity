using MicroserviceOpenIddictTemplate.DAL.Database;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Base.Definition;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace MicroserviceOpenIddictTemplate.Identity.Definitions.Database
{
    public class DatabaseDefinition : Definition
    {
        public override void ConfigureServicesAsync(IServiceCollection services, WebApplicationBuilder builder)
        {
            string migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name!;
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=Microservice.Identity";

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly(migrationsAssembly));
                options.UseOpenIddict<Guid>();
            });
            
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
            });
            
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = false;
                })
                .AddSignInManager()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserStore<ApplicationUserStore>()
                .AddRoleStore<ApplicationRoleStore>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();

            services.AddTransient<ApplicationUserStore>();
        }
    }
}
