using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.IdentityModel.Tokens;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;
using MinigolfFriday.Host.Utilities;

namespace MinigolfFriday.Host.Endpoints.Notifications;

public record SendNotificationRequest(string? UserId, string? Title, string? Body);

public class SendNotificationRequestValidator : Validator<SendNotificationRequest>
{
    public SendNotificationRequestValidator(IIdService idService)
    {
        // RuleFor(x => x.UserId).NotNull().ValidSqid(idService.User);
    }
}

public class SendNotificationEndpoint(
    DatabaseContext databaseContext,
    IJwtService jwtService,
    IIdService idService,
    IUserPushSubscriptionMapper userPushSubscriptionMapper,
    IWebPushService webPushService
) : Endpoint<SendNotificationRequest>
{
    public override void Configure()
    {
        Post("");
        Group<NotificationsGroup>();
        this.ProducesErrors(EndpointErrors.UserNotFound, EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(SendNotificationRequest req, CancellationToken ct)
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }
        if (!req.UserId.IsNullOrEmpty())
        {
            // TODO: check if we are admin - if not, this must not be possible
            userId = idService.User.DecodeSingle(req.UserId);
        }

        var user = null as UserEntity;
        try
        {
            user = await databaseContext
                .Users.Include(x => x.Settings)
                .FirstAsync(x => x.Id == userId, ct);
        }
        catch (System.Exception)
        {
            Logger.LogWarning(EndpointErrors.UserNotFound, req.UserId);
            await this.SendErrorAsync(EndpointErrors.UserNotFound, req.UserId, ct);
            return;
        }

        Logger.LogWarning("send to userId: {} ", user.Alias);

        var notifications = await databaseContext
            .Users.Where(u => u.Id == userId)
            .Select(u => new
            {
                Subscriptions = u.PushSubscriptions.Select(x => new UserPushSubscription(
                    x.Id,
                    x.UserId,
                    x.Lang,
                    x.Endpoint,
                    x.P256DH,
                    x.Auth
                )),
                NotificationData = new PushNotificationData.TestNotification(
                    req.Title.IsNullOrEmpty() ? "Body" : req.Title,
                    req.Body.IsNullOrEmpty() ? "Test" : req.Body
                )
            })
            .ToArrayAsync(ct);
        foreach (var notification in notifications)
            await webPushService.SendAsync(
                notification.Subscriptions,
                notification.NotificationData,
                ct
            );

        // var user = await databaseContext
        //     .Users.Include(x => x.Settings)
        //     .FirstAsync(x => x.Id == userId, ct);

        // if (user.Settings == null)
        // {
        //     user.Settings = new()
        //     {
        //         EnableNotifications = req.EnableNotifications ?? true,
        //         NotifyOnEventPublish = req.NotifyOnEventPublish ?? true,
        //         NotifyOnEventStart = req.NotifyOnEventStart ?? true,
        //         NotifyOnEventUpdated = req.NotifyOnEventUpdated ?? true,
        //         NotifyOnTimeslotStart = req.NotifyOnTimeslotStart ?? true,
        //         SecondsToNotifyBeforeTimeslotStart = req.SecondsToNotifyBeforeTimeslotStart ?? 600
        //     };
        // }
        // else
        // {
        //     user.Settings.EnableNotifications =
        //         req.EnableNotifications ?? user.Settings.EnableNotifications;
        //     user.Settings.NotifyOnEventPublish =
        //         req.NotifyOnEventPublish ?? user.Settings.NotifyOnEventPublish;
        //     user.Settings.NotifyOnEventStart =
        //         req.NotifyOnEventStart ?? user.Settings.NotifyOnEventStart;
        //     user.Settings.NotifyOnEventUpdated =
        //         req.NotifyOnEventUpdated ?? user.Settings.NotifyOnEventUpdated;
        //     user.Settings.NotifyOnTimeslotStart =
        //         req.NotifyOnTimeslotStart ?? user.Settings.NotifyOnTimeslotStart;
        //     user.Settings.SecondsToNotifyBeforeTimeslotStart =
        //         req.SecondsToNotifyBeforeTimeslotStart
        //         ?? user.Settings.SecondsToNotifyBeforeTimeslotStart;
        // }

        // await databaseContext.SaveChangesAsync(ct);
        await SendOkAsync(ct);

        // await realtimeEventsService.SendEventAsync(
        //     new RealtimeEvent.UserSettingsChanged(idService.User.Encode(userId)),
        //     ct
        // );
    }
}
