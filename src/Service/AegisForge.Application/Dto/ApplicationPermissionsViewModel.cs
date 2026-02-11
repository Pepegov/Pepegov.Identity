namespace AegisForge.Application.Dto;

public class ApplicationPermissionsViewModel
{
    /// <summary>
    /// <remarks>gt:</remarks>
    /// </summary>
    public List<string> GrantTypes { get; set; } = new List<string>();
    
    /// <summary>
    /// <remarks>ept:</remarks>
    /// </summary>
    public List<string> Endpoints { get; set; }  = new List<string>();
    
    /// <summary>
    /// <remarks>rst:</remarks>
    /// </summary>
    public List<string> ResponseTypes { get; set; } = new List<string>();

    /// <summary>
    /// <remarks>scp:</remarks>
    /// </summary>
    public List<string> Scopes { get; set; } = new List<string>();
}