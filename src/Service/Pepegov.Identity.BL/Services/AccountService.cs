using System.Security.Claims;
using AutoMapper;
using Pepegov.Identity.DAL.Domain;
using Pepegov.Identity.DAL.Models.Identity;
using Pepegov.Identity.DAL.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Pepegov.Identity.BL.Services.Interfaces;
using Pepegov.MicroserviceFramework.Data.Exceptions;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.EntityFramework;

namespace Pepegov.Identity.BL.Services;

public class AccountService(
    UserManager<ApplicationUser> manager,
    RoleManager<ApplicationRole> roleManager,
    IUnitOfWorkManager unitOfWorkManager,
    ILogger<AccountService> logger,
    ApplicationUserClaimsPrincipalFactory claimsFactory,
    IHttpContextAccessor httpContext,
    IMapper mapper)
    : IAccountService
{
    private readonly IUnitOfWorkEntityFrameworkInstance _unitOfWorkEntityFrameworkInstance = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();

    public Guid GetCurrentUserId()
    {
        var identity = httpContext.HttpContext?.User.Identity;
        var identitySub = (identity is ClaimsIdentity claimsIdentity ? claimsIdentity.FindFirst("sub") : (Claim)null)
                          ?? throw new InvalidOperationException("sub claim is missing");

        Guid.TryParse(identitySub.Value, out var result);
        return result;
    }

    public async Task RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken)
    {
        var user = mapper.Map<ApplicationUser>(model);
        await _unitOfWorkEntityFrameworkInstance.BeginTransactionAsync(cancellationToken);
        var createResult = await manager.CreateAsync(user, model.Password);
        const string role = UserRoles.Client;

        if (createResult.Succeeded)
        {
            if (await roleManager.FindByNameAsync(role) is null)
            {
                throw new MicroserviceNotFoundException($"role \"{role}\" not found");
            }
            var roleResult = await manager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errorsRole = roleResult.Errors.Select(x => $"{x.Code}: {x.Description}").ToList();
                throw new MicroserviceInvalidOperationException(string.Join(", ", errorsRole));
            }
            
            var profile = new ApplicationUserProfile
            {
                Created = DateTime.UtcNow, 
                Updated = DateTime.UtcNow,
                Permissions = new List<ApplicationPermission>()
            };
            
            var profileRepository = _unitOfWorkEntityFrameworkInstance.GetRepository<ApplicationUserProfile>();
            
            await profileRepository.InsertAsync(profile, cancellationToken);
            
            await _unitOfWorkEntityFrameworkInstance.SaveChangesAsync(cancellationToken);
            if (_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.IsOk)
            {
                user.ApplicationUserProfile = profile;
                await manager.UpdateAsync(user);
                
                var stringAnswer = $"User registration: email:{model.Email} | {_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception}";

                await _unitOfWorkEntityFrameworkInstance.CommitTransactionAsync(cancellationToken);
                logger.LogInformation(stringAnswer);
                logger.LogInformation($"User by {user.FirstName} {user.LastName} with UserName {user.UserName} has be registered");
                return;
            }
        }
        await _unitOfWorkEntityFrameworkInstance.RollbackTransactionAsync(cancellationToken);
        
        var errors = createResult.Errors.Select(x => $"{x.Code}: {x.Description}").ToList();
        logger.LogInformation($"User dont register: email:{model.Email} | errors: {string.Join(", ", errors)} | {_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception} ");

        throw new MicroserviceInvalidOperationException(
            $"User dont register: email:{model.Email} | errors: {string.Join(", ", errors)} | {_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception} ");
    }

    public async Task<ClaimsPrincipal> GetPrincipalByIdAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentNullException(nameof(identifier));
        }
        
        var user = await manager.FindByIdAsync(identifier);
        if (user == null)
        {
            throw new MicroserviceNotFoundException($"user by id {identifier} not found");
        }

        var defaultClaims = await claimsFactory.CreateAsync(user);
        return defaultClaims;
    }
    
    public async Task<UserAccountViewModel> GetByIdAsync(Guid id)
    {
        var user = await manager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            throw new MicroserviceNotFoundException($"User not found. id: {id}");
        }
        
        return mapper.Map<UserAccountViewModel>(user);
    }

    public async Task<UserAccountViewModel> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        var user = await manager.FindByIdAsync(userId.ToString());
        return mapper.Map<UserAccountViewModel>(user);
    }

    public async Task<IEnumerable<UserAccountViewModel>> GetUsersByEmailsAsync(IEnumerable<string> emails)
    {
        var userManager = manager;
        var result = new List<ApplicationUser>();
        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null && !result.Contains(user))
            {
                result.Add(user);
            }
        }
        return await Task.FromResult(mapper.Map<IEnumerable<UserAccountViewModel>>(result));
    }

    public async Task<IEnumerable<UserAccountViewModel>> GetUsersInRoleAsync(string roleName)
    {
        var userManager = manager;
        var result =await userManager.GetUsersInRoleAsync(roleName);
        return mapper.Map<IEnumerable<UserAccountViewModel>>(result);
    }
}