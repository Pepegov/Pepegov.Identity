using AegisForge.Application.Service.Interfaces;
using AegisForge.Domain.Aggregate;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.Entityes;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.PL.Pages.Connect.SuperAdmin;

public class SuperAdminLoginModel : PageModel
{
    private readonly IAccountService _accountService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public SuperAdminLoginModel(
        IAccountService accountService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager, 
        IUnitOfWorkManager unitOfWorkManager)
    {
        _accountService = accountService;
        _signInManager = signInManager;
        _userManager = userManager;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    [BindProperty]
    public SuperAdminLoginViewModel Input { get; set; }
    
    public IPagedList<ApplicationUser> PagedUsers { get; set; }

    public int PageSize { get; set; } = 20;
    public int PageIndex { get; set; } = 0;
    public int NextPageIndex { get => PageIndex + 1; }
    public int PreviousPageIndex {
        get
        {
            if (PageIndex - 1 >= 0)
            {
                return PageIndex - 1;
            }

            return PageIndex;
        }
    }
    public int[] ListOfPageSize { get; } = { 1, 5, 20, 50, 100 };

    public async Task OnGet(string ReturnUrl, int PageIndex = 0, int PageSize = 5)
    {
        if(PageIndex >= 0)
            this.PageIndex = PageIndex;
        if (PageSize >= 1){}
            this.PageSize = PageSize;

        await GetUserPagedList();
    }

    public async Task ChangePageSize(int pageSize)
    {
        if (pageSize < 0)
        {
            return;
        }

        PageSize = pageSize;
        await GetUserPagedList();
    }

    public async Task<IActionResult> OnPostAsync(Guid Id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(Id.ToString());
        if (user is null)
        {
            await GetUserPagedList();
            ViewData["Error"] = $"User by id {Id} is not found";
            return Page();
        }
        
        if (await _signInManager.CanSignInAsync(user))
        {
            await _signInManager.SignInAsync(user, true);
            var principal = await _accountService.GetPrincipalByIdAsync(user.Id.ToString());
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                
            if (Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            return RedirectToPage("/swagger");
        }

        await GetUserPagedList();
        ViewData["Error"] = $"It is not possible to log in for a user by id {Id}";
        return Page();
    }

    public async Task GetUserPagedList()
    {
        var unitOfWorkInstance = _unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>();
        var userRepository = unitOfWorkInstance.GetRepository<ApplicationUser>();
        PagedUsers = await userRepository.GetPagedListAsync(pageSize: PageSize, pageIndex: PageIndex);
    }
}