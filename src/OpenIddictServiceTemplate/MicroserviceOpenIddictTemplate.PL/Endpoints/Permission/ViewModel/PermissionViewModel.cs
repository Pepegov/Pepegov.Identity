namespace MicroserviceOpenIddictTemplate.PL.Endpoints.Permission.ViewModel;

public class PermissionViewModel
{
    /// <summary>
    /// Authorize attribute policy name
    /// </summary>
    public string PolicyName { get; set; } = null!;

    /// <summary>
    /// Description for current permission
    /// </summary>
    public string Description { get; set; } = null!;
}