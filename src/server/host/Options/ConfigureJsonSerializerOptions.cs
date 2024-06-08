using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MinigolfFriday.Serialization;

namespace MinigolfFriday.Options;

public class ConfigureJsonSerializerOptions : IConfigureOptions<JsonSerializerOptions>
{
    public void Configure(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower, false));
        options.Converters.Add(new DateOnlyJsonConverter());
        options.Converters.Add(new TimeOnlyJsonConverter());
    }
}
