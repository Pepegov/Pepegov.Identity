using System.ComponentModel.DataAnnotations;

namespace AegisForge.PL.Pages.Connect.SuperAdmin;

public class SuperAdminLoginViewModel
{
    [Required]
    public Guid Id { get; set; }
}