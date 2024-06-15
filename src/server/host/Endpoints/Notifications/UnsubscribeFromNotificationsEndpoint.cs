using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Host.Endpoints.Notifications;

/// <param name="Endpoint">The endpoint from the service worker subscription.</param>
public record UnsubscribeFromNotificationsRequest(string Endpoint);

public class UnsubscribeFromNotificationsRequestValidator
    : Validator<UnsubscribeFromNotificationsRequest>
{
    public UnsubscribeFromNotificationsRequestValidator()
    {
        RuleFor(x => x.Endpoint).NotEmpty();
    }
}

/// <summary>Unsubscribe from notifications.</summary>
public class UnsubscribeFromNotificationsEndpoint(DatabaseContext databaseContext)
    : Endpoint<UnsubscribeFromNotificationsRequest>
{
    public override void Configure()
    {
        Delete("");
        Group<NotificationsGroup>();
    }

    public override async Task HandleAsync(
        UnsubscribeFromNotificationsRequest req,
        CancellationToken ct
    )
    {
        await databaseContext
            .UserPushSubscriptions.Where(x => x.Endpoint == req.Endpoint)
            .ExecuteDeleteAsync(ct);
        await SendOkAsync(ct);
    }
}
