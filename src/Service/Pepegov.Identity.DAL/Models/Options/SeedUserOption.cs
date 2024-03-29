namespace Pepegov.Identity.DAL.Models.Options;

public class SeedUserOption
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string[]? Roles { get; set; } 
    public string[]? Permissions { get; set; }
}