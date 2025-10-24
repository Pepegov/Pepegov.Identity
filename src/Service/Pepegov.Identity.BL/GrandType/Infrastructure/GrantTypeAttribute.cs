namespace Pepegov.Identity.BL.GrandType.Infrastructure;

public class GrantTypeAttribute(string grantTypeName) : Attribute
{
    public string GrantTypeName { get; set; } = grantTypeName;
}