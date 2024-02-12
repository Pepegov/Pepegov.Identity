namespace Pepegov.Identity.BL.GrandType.Infrastructure;

public class GrantTypeAttribute : Attribute
{
    public string GrantTypeName { get; set; }

    public GrantTypeAttribute(string grantTypeName)
    {
        GrantTypeName = grantTypeName;
    }
}