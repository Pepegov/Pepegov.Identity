﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.DAL.Models.Options;
using Pepegov.MicroserviceFramework.Exceptions;
using Pepegov.MicroserviceFramework.Infrastructure.Extensions;

namespace Pepegov.Identity.DAL.Database
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
            await SeedPermissions();
            await SeedUsers();
        }

        private async Task SeedUsers()
        {
            using var scope = _serviceProvider.CreateScope();
            
            var adminFromConfig = scope.ServiceProvider.GetService<IOptions<AdminProfileOption>>()!.Value;
            if (adminFromConfig is null)
            {
                throw new MicroserviceArgumentNullException("Admin Profile не найден в appsetting.json");
            }

            var currentAdmin = _context.Users
                .Include(x => x.ApplicationUserProfile)
                .ThenInclude(x => x.Permissions)
                .FirstOrDefault(x => x.UserName == adminFromConfig.UserName && x.Email == adminFromConfig.Email);
            
            if (currentAdmin is not null)
            {
                foreach (var permission in _context.Permissions.ToList())
                {
                    if (currentAdmin.ApplicationUserProfile.Permissions.Any(x => x.ApplicationPermissionId == permission.ApplicationPermissionId))
                    {
                        continue;
                    }
                    
                    currentAdmin.ApplicationUserProfile.Permissions.Add(permission);
                }

                await _context.SaveChangesAsync();
                return;
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
                ApplicationUserProfile = new ApplicationUserProfile()
                {
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                }
            };

            if (!_context!.Users.Any(u => u.UserName == admin.UserName))
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
                
                var permissions = _context.Permissions.ToList();
                var profile = await _context.Profiles.Where(x => x.Id == admin.ApplicationUserProfileId).FirstOrDefaultAsync();
                
                profile.Permissions = permissions;
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
