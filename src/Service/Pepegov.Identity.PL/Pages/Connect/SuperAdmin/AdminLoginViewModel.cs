using System.ComponentModel.DataAnnotations;

namespace Pepegov.Identity.PL.Pages.Connect.SuperAdmin;

public class SuperAdminLoginViewModel
{
    [Required]
    public Guid Id { get; set; }
}