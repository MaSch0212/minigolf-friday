using System.ComponentModel.DataAnnotations;
using System.Linq;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;
using WebPush;

namespace MinigolfFriday.Host.Endpoints.Administration.Events;

/// <param name="EventId">The id of the event to start.</param>
public record StartEventRequest([property: Required] string EventId);

public class StartEventRequestValidator : Validator<StartEventRequest>
{
    public StartEventRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Starts an event.</summary>
public class StartEventEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService,
    IWebPushService webPushService
) : Endpoint<StartEventRequest>
{
    public override void Configure()
    {
        Post("{eventId}/start");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.EventNotFound,
            EndpointErrors.EventRegistrationNotElapsed,
            EndpointErrors.EventHasNoInstances,
            EndpointErrors.EventAlreadyStarted,
            EndpointErrors.EventMissingMapOnStart
        );
    }

    public override async Task HandleAsync(StartEventRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var info = await databaseContext
            .Events.Where(x => x.Id == eventId)
            .Select(x => new
            {
                x.RegistrationDeadline,
                HasInstances = x.Timeslots.Any(t => t.Instances.Any()),
                IsStarted = x.StartedAt != null,
                HasMissingMap = x
                    .Timeslots.Where(t => t.Instances.Count > 0)
                    .Any(x => x.MapId == null)
            })
            .FirstOrDefaultAsync(ct);

        if (info == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (info.RegistrationDeadline >= DateTimeOffset.Now)
        {
            Logger.LogWarning(
                EndpointErrors.EventRegistrationNotElapsed,
                eventId,
                info.RegistrationDeadline
            );
            await this.SendErrorAsync(
                EndpointErrors.EventRegistrationNotElapsed,
                req.EventId,
                info.RegistrationDeadline.ToString("O"),
                ct
            );
            return;
        }

        if (!info.HasInstances)
        {
            Logger.LogWarning(EndpointErrors.EventHasNoInstances, eventId);
            await this.SendErrorAsync(EndpointErrors.EventHasNoInstances, req.EventId, ct);
            return;
        }

        if (info.IsStarted)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, eventId);
            await this.SendErrorAsync(EndpointErrors.EventAlreadyStarted, req.EventId, ct);
            return;
        }

        if (info.HasMissingMap)
        {
            Logger.LogWarning(EndpointErrors.EventMissingMapOnStart, eventId);
            await this.SendErrorAsync(EndpointErrors.EventMissingMapOnStart, req.EventId, ct);
            return;
        }

        var now = DateTimeOffset.Now;
        await databaseContext
            .Events.Where(x => x.Id == eventId)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.StartedAt, now), ct);
        await SendAsync(null, cancellation: ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.EventChanged(
                idService.Event.Encode(eventId),
                RealtimeEventChangeType.Updated
            ),
            ct
        );
        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.PlayerEventChanged(
                idService.Event.Encode(eventId),
                RealtimeEventChangeType.Updated
            ),
            ct
        );

        var notifications = await databaseContext
            .Users.Where(u => u.EventInstances.Any(i => i.EventTimeslot.EventId == eventId))
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
                NotificationData = new PushNotificationData.EventStarted(
                    idService.Event.Encode(eventId),
                    u.EventInstances.Where(i => i.EventTimeslot.EventId == eventId)
                        .Select(i => new NotificationTimeslotInfo(
                            i.EventTimeslot.Time,
                            i.GroupCode,
                            i.EventTimeslot.Map!.Name,
                            i.Players.Count
                        ))
                        .ToArray()
                )
            })
            .ToArrayAsync(ct);
        foreach (var notification in notifications)
            await webPushService.SendAsync(
                notification.Subscriptions,
                notification.NotificationData,
                ct
            );
    }
}
