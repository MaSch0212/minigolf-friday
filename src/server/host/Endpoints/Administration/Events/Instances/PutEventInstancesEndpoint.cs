using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using MaSch.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Instances;

/// <param name="EventId">The id of the event to set the instances.</param>
/// <param name="Instances">The instances to set for the event.</param>
public record PutEventInstancesRequest(
    [property: Required] string EventId,
    [property: Required] EventTimeslotInstances[] Instances
);

public class PutEventInstancesRequestValidator : Validator<PutEventInstancesRequest>
{
    public PutEventInstancesRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
        RuleFor(x => x.Instances).NotEmpty();
    }
}

/// <summary>Set the instances of an event.</summary>
public class PutEventInstancesEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService,
    IEventInstanceService eventInstanceService,
    IWebPushService webPushService
) : Endpoint<PutEventInstancesRequest>
{
    public override void Configure()
    {
        Put("{eventId}/instances");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.EventNotFound,
            EndpointErrors.EventRegistrationNotElapsed
        );
    }

    public override async Task HandleAsync(PutEventInstancesRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var eventInfo = await databaseContext
            .Events.Where(x => x.Id == eventId)
            .Select(e => new
            {
                Instances = e
                    .Timeslots.Select(t => new EventTimeslotInstances(
                        idService.EventTimeslot.Encode(t.Id),
                        t.Instances.Select(i => new EventInstance(
                                idService.EventInstance.Encode(i.Id),
                                i.GroupCode,
                                i.Players.Select(y => idService.User.Encode(y.Id)).ToArray()
                            ))
                            .ToArray()
                    ))
                    .ToArray(),
                e.RegistrationDeadline,
                e.StartedAt
            })
            .FirstOrDefaultAsync(ct);

        if (eventInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (eventInfo.RegistrationDeadline >= DateTimeOffset.Now)
        {
            Logger.LogWarning(
                EndpointErrors.EventRegistrationNotElapsed,
                eventId,
                eventInfo.RegistrationDeadline
            );
            await this.SendErrorAsync(
                EndpointErrors.EventRegistrationNotElapsed,
                req.EventId,
                eventInfo.RegistrationDeadline,
                ct
            );
            return;
        }

        await eventInstanceService.PersistEventInstancesAsync(req.Instances, ct);

        await SendOkAsync(ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.EventInstancesChanged(idService.Event.Encode(eventId)),
            ct
        );

        if (eventInfo.StartedAt != null)
        {
            var updatedPlayers = GetUpdatedPlayerIds(eventInfo.Instances, req.Instances);
            await SendUpdateNotifications(updatedPlayers.ToArray(), eventId, ct);
        }
    }

    private IEnumerable<long> GetUpdatedPlayerIds(
        IEnumerable<EventTimeslotInstances> oldInstances,
        IEnumerable<EventTimeslotInstances> newInstances
    )
    {
        var oldGroups = oldInstances
            .SelectMany(x =>
                x.Instances.Select(y => new { Group = (x.TimeslotId, y.GroupCode), y.PlayerIds })
            )
            .ToArray();
        var newGroups = newInstances
            .SelectMany(x =>
                x.Instances.Select(y => new { Group = (x.TimeslotId, y.GroupCode), y.PlayerIds })
            )
            .ToArray();
        return oldGroups
            .SelectMany(x =>
            {
                var existing = newGroups.FirstOrDefault(y => y.Group == x.Group);
                return existing != null
                    ? x
                        .PlayerIds.Except(existing.PlayerIds)
                        .Union(existing.PlayerIds.Except(x.PlayerIds))
                    : x.PlayerIds;
            })
            .Union(
                newGroups.SelectMany(x =>
                    oldGroups.FirstOrDefault(y => y.Group == x.Group) == null
                        ? x.PlayerIds
                        : Enumerable.Empty<string>()
                )
            )
            .Distinct()
            .Select(x => idService.User.DecodeSingle(x));
    }

    private async Task SendUpdateNotifications(
        ICollection<long> playerIds,
        long eventId,
        CancellationToken ct
    )
    {
        var notifications = await databaseContext
            .Users.Where(u =>
                playerIds.Contains(u.Id)
                && (
                    u.Settings == null
                    || u.Settings.EnableNotifications && u.Settings.NotifyOnEventUpdated
                )
            )
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
                NotificationData = new PushNotificationData.EventInstanceUpdated(
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
