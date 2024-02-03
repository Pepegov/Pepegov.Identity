using System.ComponentModel.DataAnnotations;

namespace Pepegov.Identity.DAL.ViewModel;

public class RegisterViewModel
{
    [Required]
    [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
    [Display(Name = "User name")]
    public string UserName { get; set; } = null!;
    [Required]
    [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;
    [Required]
    [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;
    [Required]
    [Display(Name = "Date of birth")]
    public DateTime DateOfBirth { get; set; }
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }
    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}