using System.Text.Json.Serialization;

namespace AegisForge.Domain.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SessionStatusType
{
    /// <summary>
    /// Сессия активна
    /// </summary>
    Active,
    
    /// <summary>
    /// Сессия изменена (например, пользователь вышел в другом месте)
    /// </summary>
    Changed,
    
    /// <summary>
    /// Сессия отозвана
    /// </summary>
    Revoked,
}