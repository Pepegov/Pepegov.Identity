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

public class AccountService : IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly ApplicationUserClaimsPrincipalFactory _claimsFactory;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWorkEntityFrameworkInstance _unitOfWorkEntityFrameworkInstance;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager, 
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<AccountService> logger,
        ApplicationUserClaimsPrincipalFactory claimsFactory,
        IHttpContextAccessor httpContext,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _claimsFactory = claimsFactory;
        _httpContext = httpContext;
        _mapper = mapper;
        _unitOfWorkEntityFrameworkInstance = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
    }
    
    public Guid GetCurrentUserId()
    {
        var identity = _httpContext.HttpContext?.User.Identity;
        var identitySub = (identity is ClaimsIdentity claimsIdentity ? claimsIdentity.FindFirst("sub") : (Claim)null)
                          ?? throw new InvalidOperationException("sub claim is missing");

        Guid.TryParse(identitySub.Value, out var result);
        return result;
    }

    public async Task RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken)
    {
        var result = new UserAccountViewModel();
        
        var user = _mapper.Map<ApplicationUser>(model);
        await _unitOfWorkEntityFrameworkInstance.BeginTransactionAsync(cancellationToken);
        var createResult = await _userManager.CreateAsync(user, model.Password);
        const string role = UserRoles.Client;

        if (createResult.Succeeded)
        {
            if (await _roleManager.FindByNameAsync(role) is null)
            {
                throw new MicroserviceNotFoundException($"role \"{role}\" not found");
            }
            var roleResult = await _userManager.AddToRoleAsync(user, role);
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
                await _userManager.UpdateAsync(user);
                
                var stringAnswer = $"User registration: email:{model.Email} | {_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception}";

                await _unitOfWorkEntityFrameworkInstance.CommitTransactionAsync(cancellationToken);
                _logger.LogInformation(stringAnswer);
                _logger.LogInformation($"User by {user.FirstName} {user.LastName} with UserName {user.UserName} has be registered");
                return;
            }
        }
        await _unitOfWorkEntityFrameworkInstance.RollbackTransactionAsync(cancellationToken);
        
        var errors = createResult.Errors.Select(x => $"{x.Code}: {x.Description}").ToList();
        _logger.LogInformation($"User dont register: email:{model.Email} | errors: {string.Join(", ", errors)} | {_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception} ");

        throw new MicroserviceInvalidOperationException(
            $"User dont register: email:{model.Email} | errors: {string.Join(", ", errors)} | {_unitOfWorkEntityFrameworkInstance.LastSaveChangesResult.Exception} ");
    }

    public async Task<ClaimsPrincipal> GetPrincipalByIdAsync(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentNullException(nameof(identifier));
        }
        
        var user = await _userManager.FindByIdAsync(identifier);
        if (user == null)
        {
            throw new MicroserviceNotFoundException($"user by id {identifier} not found");
        }

        var defaultClaims = await _claimsFactory.CreateAsync(user);
        return defaultClaims;
    }
    
    public async Task<UserAccountViewModel> GetByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            throw new MicroserviceNotFoundException($"User not found. id: {id}");
        }
        
        return _mapper.Map<UserAccountViewModel>(user);
    }

    public async Task<UserAccountViewModel> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return _mapper.Map<UserAccountViewModel>(user);
    }

    public async Task<IEnumerable<UserAccountViewModel>> GetUsersByEmailsAsync(IEnumerable<string> emails)
    {
        var userManager = _userManager;
        var result = new List<ApplicationUser>();
        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null && !result.Contains(user))
            {
                result.Add(user);
            }
        }
        return await Task.FromResult(_mapper.Map<IEnumerable<UserAccountViewModel>>(result));
    }

    public async Task<IEnumerable<UserAccountViewModel>> GetUsersInRoleAsync(string roleName)
    {
        var userManager = _userManager;
        var result =await userManager.GetUsersInRoleAsync(roleName);
        return _mapper.Map<IEnumerable<UserAccountViewModel>>(result);
    }
}