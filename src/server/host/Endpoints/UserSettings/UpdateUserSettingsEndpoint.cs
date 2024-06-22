using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.UserSettings;

/// <param name="EnableNotifications">Whether to enable notifications.</param>
/// <param name="NotifyOnEventPublish">Whether to notify on event publish.</param>
/// <param name="NotifyOnEventStart">Whether to notify on event start.</param>
/// <param name="NotifyOnEventUpdated">Whether to notify on event updated.</param>
/// <param name="NotifyOnTimeslotStart">Whether to notify on timeslot start.</param>
/// <param name="SecondsToNotifyBeforeTimeslotStart">The number of seconds to notify before a timeslot starts.</param>
public record UpdateUserSettingsRequest(
    bool? EnableNotifications,
    bool? NotifyOnEventPublish,
    bool? NotifyOnEventStart,
    bool? NotifyOnEventUpdated,
    bool? NotifyOnTimeslotStart,
    int? SecondsToNotifyBeforeTimeslotStart
);

public class UpdateUserSettingsRequestValidator : Validator<UpdateUserSettingsRequest>
{
    public UpdateUserSettingsRequestValidator(IIdService idService)
    {
        When(
            x => x.SecondsToNotifyBeforeTimeslotStart.HasValue,
            () =>
                RuleFor(x => x.SecondsToNotifyBeforeTimeslotStart!.Value).InclusiveBetween(0, 3600)
        );
    }
}

public class UpdateUserSettingsEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IJwtService jwtService,
    IIdService idService
) : Endpoint<UpdateUserSettingsRequest>
{
    public override void Configure()
    {
        Post("");
        Group<UserSettingsGroup>();
        this.ProducesErrors(EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(UpdateUserSettingsRequest req, CancellationToken ct)
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }

        var user = await databaseContext
            .Users.Include(x => x.Settings)
            .FirstAsync(x => x.Id == userId, ct);

        if (user.Settings == null)
        {
            user.Settings = new()
            {
                EnableNotifications = req.EnableNotifications ?? true,
                NotifyOnEventPublish = req.NotifyOnEventPublish ?? true,
                NotifyOnEventStart = req.NotifyOnEventStart ?? true,
                NotifyOnEventUpdated = req.NotifyOnEventUpdated ?? true,
                NotifyOnTimeslotStart = req.NotifyOnTimeslotStart ?? true,
                SecondsToNotifyBeforeTimeslotStart = req.SecondsToNotifyBeforeTimeslotStart ?? 600
            };
        }
        else
        {
            user.Settings.EnableNotifications =
                req.EnableNotifications ?? user.Settings.EnableNotifications;
            user.Settings.NotifyOnEventPublish =
                req.NotifyOnEventPublish ?? user.Settings.NotifyOnEventPublish;
            user.Settings.NotifyOnEventStart =
                req.NotifyOnEventStart ?? user.Settings.NotifyOnEventStart;
            user.Settings.NotifyOnEventUpdated =
                req.NotifyOnEventUpdated ?? user.Settings.NotifyOnEventUpdated;
            user.Settings.NotifyOnTimeslotStart =
                req.NotifyOnTimeslotStart ?? user.Settings.NotifyOnTimeslotStart;
            user.Settings.SecondsToNotifyBeforeTimeslotStart =
                req.SecondsToNotifyBeforeTimeslotStart
                ?? user.Settings.SecondsToNotifyBeforeTimeslotStart;
        }

        await databaseContext.SaveChangesAsync(ct);
        await SendOkAsync(ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.UserSettingsChanged(idService.User.Encode(userId)),
            ct
        );
    }
}
