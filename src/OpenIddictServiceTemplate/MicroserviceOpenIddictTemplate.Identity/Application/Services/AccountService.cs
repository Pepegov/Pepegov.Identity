using System.Security.Claims;
using AutoMapper;
using MicroserviceOpenIddictTemplate.DAL.Database;
using MicroserviceOpenIddictTemplate.DAL.Domain;
using MicroserviceOpenIddictTemplate.DAL.Models.Identity;
using MicroserviceOpenIddictTemplate.Identity.Definitions.Identity;
using MicroserviceOpenIddictTemplate.Identity.Endpoints.Account.ViewModel;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Pepegov.MicroserviceFramerwork.Patterns.UnitOfWork;
using Pepegov.MicroserviceFramerwork.ResultWrapper;

namespace MicroserviceOpenIddictTemplate.Identity.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly ILogger<AccountService> _logger;
    private readonly ApplicationUserClaimsPrincipalFactory _claimsFactory;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager, 
        IUnitOfWork<ApplicationDbContext> unitOfWork,
        ILogger<AccountService> logger,
        ApplicationUserClaimsPrincipalFactory claimsFactory,
        IHttpContextAccessor httpContext,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _claimsFactory = claimsFactory;
        _httpContext = httpContext;
        _mapper = mapper;
    }
    
    public Guid GetCurrentUserId()
    {
        var identity = _httpContext.HttpContext?.User.Identity;
        var identitySub = (identity is ClaimsIdentity claimsIdentity ? claimsIdentity.FindFirst("sub") : (Claim)null)
                          ?? throw new InvalidOperationException("sub claim is missing");
        
        Guid result;
        Guid.TryParse(identitySub.Value, out result);
        return result;
    }

    public async Task<ResultWrapper<UserAccountViewModel>> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken)
    {
        var result = new ResultWrapper<UserAccountViewModel>();
        
        var user = _mapper.Map<ApplicationUser>(model);
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        var createResult = await _userManager.CreateAsync(user, model.Password);
        const string role = UserRoles.Client;

        if (createResult.Succeeded)
        {
            if (await _roleManager.FindByNameAsync(role) is null)
            {
                result.AddException(new ArgumentNullException($"role \"{role}\" not found"));
                return result;
            }
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errorsRole = roleResult.Errors.Select(x => $"{x.Code}: {x.Description}").ToList();
                result.AddMetadatas(new Metadata(string.Join(", ", errorsRole), MetadataType.Error));
            }
            //await AddClaimsToUser(user, role);


            var permission = new ApplicationPermission()
            {
                PolicyName = "User:View",
                Description = "Standart user polycy for minimal view"
            };
            var profile = new ApplicationUserProfile
            {
                Created = DateTime.UtcNow, 
                Updated = DateTime.UtcNow,
                Permissions = new List<ApplicationPermission>()
            };
            
            var permissionRepository = _unitOfWork.GetRepository<ApplicationPermission>();
            var profileRepository = _unitOfWork.GetRepository<ApplicationUserProfile>();

            var currentPermussion =
                await permissionRepository.GetFirstOrDefaultAsync(predicate: x =>
                    x.PolicyName == permission.PolicyName, disableTracking: false);
            profile.Permissions.Add(currentPermussion ?? permission);

            if (currentPermussion is null)
            {
                await permissionRepository.InsertAsync(profile.Permissions, cancellationToken);
            }
            
            await profileRepository.InsertAsync(profile, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync();
            if (_unitOfWork.LastSaveChangesResult.IsOk)
            {
                var stringAnswer = $"User registration: email:{model.Email} | {_unitOfWork.LastSaveChangesResult.Exception}";
                var principal = await _claimsFactory.CreateAsync(user);
                result.Message = _mapper.Map<UserAccountViewModel>(principal.Identity);
                result.AddMetadatas(new Metadata(stringAnswer, MetadataType.Success));
                
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation($"User registration: email:{model.Email} | {_unitOfWork.LastSaveChangesResult.Exception} ");
                return result;
            }
        }
        await transaction.RollbackAsync(cancellationToken);
        
        var errors = createResult.Errors.Select(x => $"{x.Code}: {x.Description}").ToList();
        _logger.LogInformation($"User dont register: email:{model.Email} | errors: {string.Join(", ", errors)} | {_unitOfWork.LastSaveChangesResult.Exception} ");

        result.AddException(new Exception($"User dont register: email:{model.Email} | errors: {string.Join(", ", errors)} | {_unitOfWork.LastSaveChangesResult.Exception} "));
        if (errors.Any())
        {
            result.AddMetadatas(new Metadata(string.Join(", ", errors), MetadataType.Error));
        }
        
        return result;
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
            throw new Exception("user not found");
        }

        var defaultClaims = await _claimsFactory.CreateAsync(user);
        return defaultClaims;
    }

    public Task<ClaimsPrincipal> GetPrincipalForUserAsync(ApplicationUser user) => _claimsFactory.CreateAsync(user);

    public async Task<ResultWrapper<ApplicationUser>> GetByIdAsync(Guid id)
    {
        var result = new ResultWrapper<ApplicationUser>();
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            result.AddException(new ArgumentNullException($"User not found. id: {id}"));
        }
        result.Message = user;
        
        return result;
    }

    public async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var userManager = _userManager;
        var userId = GetCurrentUserId().ToString();
        var user = await userManager.FindByIdAsync(userId);
        return user;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersByEmailsAsync(IEnumerable<string> emails)
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
        return await Task.FromResult(result);
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName)
    {
        var userManager = _userManager;
        return await userManager.GetUsersInRoleAsync(roleName);
    }
    
    private async Task AddClaimsToUser(ApplicationUser user, string role)
    {
        await _userManager.AddClaimAsync(user, new Claim(OpenIddictConstants.Claims.Name, user.UserName));
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, user.FirstName));
        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, user.LastName));
        await _userManager.AddClaimAsync(user, new Claim(OpenIddictConstants.Claims.Email, user.Email));
        await _userManager.AddClaimAsync(user, new Claim(OpenIddictConstants.Claims.Role, role));
    }
}