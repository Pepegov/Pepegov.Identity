using System.Reflection;

namespace AegisForge.Application.GrandType.Infrastructure;

public class GrantTypeConnectionManager
{
    private Dictionary<string, IGrantTypeConnection> handlerDictionary = new();
    
    public GrantTypeConnectionManager(IEnumerable<IGrantTypeConnection> connectionHandlers)
    {
        foreach (var connectionHandler in connectionHandlers)
        {
            var grantTypeName = connectionHandler.GetType().GetCustomAttribute<GrantTypeAttribute>()?.GrantTypeName;
            ArgumentNullException.ThrowIfNull(grantTypeName);
            handlerDictionary.Add(grantTypeName, connectionHandler);
        }
    }

    public bool TryGet(string grantType, out IGrantTypeConnection handler)
    {
        return handlerDictionary.TryGetValue(grantType, out handler!);
    }
}