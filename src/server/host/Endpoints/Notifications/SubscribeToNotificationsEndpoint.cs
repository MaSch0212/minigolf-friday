using FastEndpoints;
using FluentValidation;
using MaSch.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Notifications;

/// <param name="Lang">The language for the notification to sent to this subscription.</param>
/// <param name="Endpoint">The endpoint from the service worker subscription.</param>
/// <param name="P256DH">The public key from the service worker subscription.</param>
/// <param name="Auth">The auth key from the service worker subscription.</param>
public record SubscribeToNotificationsRequest(
    string Lang,
    string Endpoint,
    string? P256DH,
    string? Auth
);

public class SubscribeToNotificationsRequestValidator : Validator<SubscribeToNotificationsRequest>
{
    public SubscribeToNotificationsRequestValidator()
    {
        RuleFor(x => x.Lang).NotEmpty().Matches(@"^[a-z]{2}(-[a-zA-Z]+)?$");
        RuleFor(x => x.Endpoint).NotEmpty();
    }
}

/// <summary>Subscribe to notifications.</summary>
public class SubscribeToNotificationsEndpoint(
    DatabaseContext databaseContext,
    IJwtService jwtService
) : Endpoint<SubscribeToNotificationsRequest>
{
    public override void Configure()
    {
        Put("");
        Group<NotificationsGroup>();
        this.ProducesErrors(EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(
        SubscribeToNotificationsRequest req,
        CancellationToken ct
    )
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }

        var existingSubscription = await databaseContext.UserPushSubscriptions.FirstOrDefaultAsync(
            x => x.Endpoint == req.Endpoint,
            ct
        );

        if (existingSubscription is null)
        {
            databaseContext.UserPushSubscriptions.Add(
                new()
                {
                    UserId = userId,
                    Lang = req.Lang,
                    Endpoint = req.Endpoint,
                    P256DH = req.P256DH,
                    Auth = req.Auth
                }
            );
            await databaseContext.SaveChangesAsync(ct);
        }
        else
        {
            var dbUpdateBuilder = DbUpdateBuilder.Create(
                databaseContext.UserPushSubscriptions.Where(x => x.Id == existingSubscription.Id)
            );
            if (existingSubscription.UserId != userId)
                dbUpdateBuilder.With(x => x.SetProperty(x => x.UserId, userId));
            if (existingSubscription.Lang != req.Lang)
                dbUpdateBuilder.With(x => x.SetProperty(x => x.Lang, req.Lang));
            if (existingSubscription.P256DH != req.P256DH)
                dbUpdateBuilder.With(x => x.SetProperty(x => x.P256DH, req.P256DH));
            if (existingSubscription.Auth != req.Auth)
                dbUpdateBuilder.With(x => x.SetProperty(x => x.Auth, req.Auth));
            await dbUpdateBuilder.ExecuteAsync(ct);
        }

        await SendOkAsync(ct);
    }
}
