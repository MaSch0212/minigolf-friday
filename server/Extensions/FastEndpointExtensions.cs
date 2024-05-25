using FastEndpoints;

namespace MinigolfFriday.Extensions;

public static class FastEndpointExtensions
{
    public static void AppendResponseDescription(
        this EndpointSummary summary,
        int statusCode,
        string description
    )
    {
        if (summary.Responses.TryGetValue(statusCode, out var existingDesc))
        {
            description = $"""
                {existingDesc}
                -or-
                {description}
                """;
        }

        summary.Responses[statusCode] = description;
    }
}
