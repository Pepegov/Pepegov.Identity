using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.DAL.Models.Options;
using Pepegov.MicroserviceFramework.Infrastructure.Extensions;

namespace Pepegov.Identity.DAL.Database
{
    public class DatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;
        private readonly ApplicationDbContext _context;

        public DatabaseInitializer(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _logger = serviceProvider.GetRequiredService<ILogger<DatabaseInitializer>>();
        }
        
        public async Task Seed()
        {
            await _context!.Database.EnsureCreatedAsync();
            var pending = await _context.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                await _context!.Database.MigrateAsync();
            }

            await SeedRoles();
            await SeedPermissions();
            await SeedUsers();
        }

        private async Task SeedUsers()
        {
            using var scope = _serviceProvider.CreateScope();
            
            var seedUsers = scope.ServiceProvider.GetService<IOptions<List<SeedUserOption>>>()!.Value;
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if (seedUsers is null || seedUsers.Count == 0)
            {
                _logger.LogWarning("SeedUsers not found in appsetting.json");
                return;
            }

            foreach (var seedUser in seedUsers)
            {
                var user = new ApplicationUser
                {
                    Email = seedUser.Email,
                    UserName = seedUser.UserName,
                    FirstName = seedUser.FirstName,
                    LastName = seedUser.LastName,
                    PhoneNumber = seedUser.PhoneNumber,
                    EmailConfirmed = seedUser.EmailConfirmed,
                    PhoneNumberConfirmed = seedUser.PhoneNumberConfirmed,
                    BirthDate = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ApplicationUserProfile = new ApplicationUserProfile()
                    {
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow,
                        Permissions = new List<ApplicationPermission>()
                    }
                };

                await SeedUserProfile(user, seedUser);
                await SeedRolesToProfile(user, seedUser.Roles);
                await SeedPermissionsToProfile(user, seedUser.Permissions);
            }
            
            async Task SeedPermissionsToProfile(ApplicationUser user, string[]? permissions)
            {
                if (permissions is null || permissions.Length == 0)
                {
                    return;
                }
                
                var profile = await _context.Users
                    .Where(x => x.UserName == user.UserName)
                    .Include(i => i.ApplicationUserProfile)
                    .ThenInclude(ti => ti.Permissions)
                    .FirstOrDefaultAsync();

                if (profile is null)
                {
                    throw new InvalidSeedException($"User by name {user.UserName} not found");
                }

                foreach (var permissionName in permissions)
                {
                    if (!profile.ApplicationUserProfile!.Permissions!.Select(x => x.PolicyName).Contains(permissionName))
                    {
                        profile.ApplicationUserProfile.Permissions!.Add(new ApplicationPermission()
                        {
                            PolicyName = permissionName,
                            Description = "automatically seed permission"
                        });
                    }
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully seed permission by names {string.Join(", ", permissions)} to user by name {user.UserName}");
            }
            
            async Task SeedRolesToProfile(ApplicationUser user, string[]? roles)
            {
                if (roles is null || roles.Length == 0)
                {
                    return;
                }
                
                var currentUser = await userManager.Users.FirstOrDefaultAsync(x =>
                    x.UserName == user.UserName);
                if (currentUser is null)
                {
                    throw new InvalidSeedException($"User by name {user.UserName} not found");
                }

                foreach (var role in roles)
                {
                    if (await userManager.IsInRoleAsync(currentUser, role))
                    {
                        continue;
                    }
                    
                    var roleResult = await userManager!.AddToRoleAsync(currentUser, role);
                    if (!roleResult.Succeeded)
                    {
                        var roleErrorMessage = string.Join(", ", roleResult.Errors.Select(x => $" Code:{x.Code} Description:{x.Description}"));
                        throw new InvalidSeedException($"Cannot add roles {string.Join(", ", roles)} to account by name {user.UserName}. Errors: {roleErrorMessage}");
                    }
                    
                    _logger.LogInformation($"Successfully add role {role} to user by name {user.UserName}");
                }
            }

            async Task SeedUserProfile(ApplicationUser user, SeedUserOption seedUser)
            {
                if (_context!.Users.Any(u => u.UserName == seedUser.UserName || u.Email == seedUser.Email || u.PhoneNumber == seedUser.PhoneNumber))
                {
                    return;
                }
                
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, seedUser.Password);
                
                var userResult = await userManager!.CreateAsync(user);
                if (!userResult.Succeeded)
                {
                    throw new InvalidSeedException($"Cannot create account {userResult.Errors.FirstOrDefault()?.Description}");
                }
                
                _logger.LogInformation($"Successfully seed user with: Login {user.UserName} Email {user.Email} Phone {user.PhoneNumber}");
            }
        }

        private async Task SeedRoles()
        {
            using var scope = _serviceProvider.CreateScope();

            var roles = typeof(UserRoles).GetAllPublicConstantValues<string>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var role in roles)
            {
                bool check = await roleManager.RoleExistsAsync(role);
                if (!check)
                {
                    await roleManager.CreateAsync(new ApplicationRole(role));
                }
            }

            await _context.SaveChangesAsync();
        }
        
        private async Task SeedPermissions()
        {
            using var scope = _serviceProvider.CreateScope();
            var identityConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppData.IdentitySettingPath)
                .Build();
            var options = identityConfiguration.GetSection("Permissions").Get<List<IdentityPermissionOption>>();

            foreach (var option in options)
            {
                var permission = _context.Permissions.Where(x => x.PolicyName == option.Name).ToList();
                if (permission.Any())
                {
                    continue;
                }
                await _context.Permissions.AddAsync(new ApplicationPermission() { PolicyName = option.Name, Description = option.Description});
            }
            await _context.SaveChangesAsync();
        }
    }
}
