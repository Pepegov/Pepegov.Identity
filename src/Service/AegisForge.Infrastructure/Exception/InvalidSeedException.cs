using System.Runtime.Serialization;

namespace AegisForge.Infrastructure.Exception;

[Serializable]
public class InvalidSeedException : System.Exception
{
    public InvalidSeedException() {}

    public InvalidSeedException(string message) : base(message) {}

    public InvalidSeedException(string message, System.Exception innerException) : base(message, innerException) {}

    protected InvalidSeedException(SerializationInfo info,  StreamingContext context) : base(info, context) {}
}