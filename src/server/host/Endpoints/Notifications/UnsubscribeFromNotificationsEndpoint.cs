using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

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
public class UnsubscribeFromNotificationsEndpoint(
    DatabaseContext databaseContext,
    IJwtService jwtService,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService
) : Endpoint<UnsubscribeFromNotificationsRequest>
{
    public override void Configure()
    {
        Delete("");
        Group<NotificationsGroup>();
        Description(x =>
            x.ClearDefaultAccepts().Accepts<UnsubscribeFromNotificationsRequest>("application/json")
        );
        this.ProducesErrors(EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(
        UnsubscribeFromNotificationsRequest req,
        CancellationToken ct
    )
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }
        await databaseContext
            .UserPushSubscriptions.Where(x => x.Endpoint == req.Endpoint)
            .ExecuteDeleteAsync(ct);
        await SendOkAsync(ct);
        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.UserChanged(
                idService.User.Encode(userId),
                RealtimeEventChangeType.Updated
            ),
            ct
        );
    }
}
