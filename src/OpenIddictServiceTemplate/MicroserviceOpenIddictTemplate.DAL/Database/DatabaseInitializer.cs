using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.DAL.Models.Options;
using MicroserviceOpenIddictTemplate.Identity.Base.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceOpenIddictTemplate.DAL.Database
{
    public class DatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        public DatabaseInitializer(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
        }
        
        public async Task Seed()
        {
            await _context!.Database.EnsureCreatedAsync();
            var pending = await _context.Database.GetPendingMigrationsAsync(); //Асинхронно получает все миграции, определенные в сборке, но не примененные к целевой базе данных.
            if (pending.Any())
            {
                await _context!.Database.MigrateAsync();
            }

            await SeedRoles();
            await SeedUsers();
        }

        private async Task SeedUsers()
        {
            using var scope = _serviceProvider.CreateScope();
            
            if (_context.Users.Any())
            {
                return;
            }
            
            var adminFromConfig = scope.ServiceProvider.GetService<IOptions<AdminUser>>()!.Value;
            if (adminFromConfig is null)
            {
                throw new ArgumentNullException("AdminUser не найден в appsetting.json");
            }

            var admin = new ApplicationUser
            {
                Email = adminFromConfig.Email,
                UserName = adminFromConfig.UserName,
                FirstName = adminFromConfig.FirstName,
                LastName = adminFromConfig.LastName,
                PhoneNumber = adminFromConfig.PhoneNumber,
                EmailConfirmed = adminFromConfig.EmailConfirmed,
                PhoneNumberConfirmed = adminFromConfig.PhoneNumberConfirmed,
                BirthDate = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };

            if (!_context!.Users.Any(u => u.UserName == admin.UserName)) //Проверка на то, что админ уже существует
            {
                var password = new PasswordHasher<ApplicationUser>();
                admin.PasswordHash = password.HashPassword(admin, adminFromConfig.Password);
                
                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var userResult = await userManager.CreateAsync(admin);
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException($"Cannot create account {userResult.Errors.FirstOrDefault()?.Description}");
                }
                
                var roleResult = await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Cannot add roles to account");
                }

                await _context.SaveChangesAsync();
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
    }
}
