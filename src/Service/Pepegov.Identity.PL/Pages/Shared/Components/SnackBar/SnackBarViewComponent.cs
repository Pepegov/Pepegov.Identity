using Microsoft.AspNetCore.Mvc;

namespace Pepegov.Identity.PL.Pages.Shared.Components.SnackBar;

public class SnackBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string errorDescription, string? errorTitle)
    {
        if (errorTitle is null)
        {
            errorTitle = "Error";
        }

        ViewData["Error"] = errorDescription;
        ViewData["ErrorTitle"] = errorTitle;
        
        return View();
    }

}