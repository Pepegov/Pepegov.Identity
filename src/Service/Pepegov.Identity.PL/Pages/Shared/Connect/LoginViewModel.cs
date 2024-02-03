using System.ComponentModel.DataAnnotations;

namespace Pepegov.Identity.PL.Pages.Shared.Connect;

public class LoginViewModel
{

    [Required]
    [Display(Name = "Name")]
    public string UserName { get; set; } = null!;

    [Required]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;

    [Required]
    public string ReturnUrl { get; set; } = null!;
}