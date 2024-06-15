using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinigolfFriday.Domain.Serialization;

public class CamelCaseStringEnumConverter()
    : JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false);
