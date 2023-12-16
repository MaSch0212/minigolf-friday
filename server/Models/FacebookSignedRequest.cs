using System.Text.Json.Serialization;
using MinigolfFriday.Utilities;

namespace MinigolfFriday;

public class FacebookSignedRequest
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }

    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; set; }

    [JsonPropertyName("issued_at")]
    [JsonConverter(typeof(UnixDateTimeJsonConverter))]
    public required DateTime IssuedAt { get; set; }
}
