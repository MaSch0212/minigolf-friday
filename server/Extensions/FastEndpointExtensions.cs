using FastEndpoints;

namespace MinigolfFriday.Extensions;

public static class FastEndpointExtensions
{
    public static void AppendResponseDescription(
        this EndpointSummary summary,
        int statusCode,
        string description,
        bool insertBeforeExisting = false
    )
    {
        if (summary.Responses.TryGetValue(statusCode, out var existingDesc))
        {
            description = insertBeforeExisting
                ? $"{description}\n-or-\n{existingDesc}"
                : $"{existingDesc}\n-or-\n{description}";
        }

        summary.Responses[statusCode] = description;
    }
}
