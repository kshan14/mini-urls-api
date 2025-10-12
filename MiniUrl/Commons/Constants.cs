using System.Text.Json;
using System.Text.Json.Serialization;

namespace MiniUrl.Commons;

public class Constants
{
    public static readonly string TinyUrlCreatedChannel = "tinyurl.created";
    public static readonly string TinyUrlApprovedChannel = "tinyurl.approved";
    public static readonly string TinyUrlRejectedChanel = "tinyurl.rejected";
    
    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // camelCase naming
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Ignore null values
    };
}
