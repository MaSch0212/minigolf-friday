using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using MaSch.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events;

// / <param name="EventId">The id of the event.</param>
// / <param name="Commit">set true if you want to commit the event and send a notification to the user</param>
public record UpdateEventRequest(
    [property: Required] string EventId,
    bool? Commit,
    string? ExternalUri
);

public class UpdateEventRequestValidator : Validator<UpdateEventRequest>
{
    public UpdateEventRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
        RuleFor(x => x)
            .Must(x => x.Commit != null || x.ExternalUri != null)
            .WithMessage("Either Commit or ExternalUri must be set");
    }
}

/// <summary>Update a new event.</summary>
public class UpdateEventEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService,
    IUserPushSubscriptionMapper userPushSubscriptionMapper,
    IWebPushService webPushService
) : Endpoint<UpdateEventRequest>
{
    public override void Configure()
    {
        Patch("{eventId}");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(EndpointErrors.EventNotFound, EndpointErrors.EventNotStaged);
    }

    public override async Task HandleAsync(UpdateEventRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var eventQuery = databaseContext.Events.Where(x => x.Id == eventId);
        var eventInfo = await eventQuery.FirstOrDefaultAsync(ct);

        if (eventInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (req.Commit == false && !eventInfo.Staged)
        {
            Logger.LogWarning(EndpointErrors.EventNotStaged, eventId);
            await this.SendErrorAsync(
                EndpointErrors.EventNotStaged,
                idService.Event.Encode(eventId),
                ct
            );
            return;
        }

        var updateBuilder = DbUpdateBuilder.Create(eventQuery);

        if (req.Commit == true)
        {
            updateBuilder.With(x => x.SetProperty(x => x.Staged, false));
        }

        if (req.ExternalUri != null)
        {
            updateBuilder.With(x => x.SetProperty(x => x.ExternalUri, req.ExternalUri));
        }

        ThrowIfAnyErrors();

        await updateBuilder.ExecuteAsync(ct);
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
                RealtimeEventChangeType.Created
            ),
            ct
        );

        if (req.Commit == true)
        {
            var pushSubscriptions = await databaseContext
                .UserPushSubscriptions.Where(x =>
                    x.User.Settings == null
                    || (x.User.Settings.EnableNotifications && x.User.Settings.NotifyOnEventPublish)
                )
                .Select(userPushSubscriptionMapper.MapUserPushSubscriptionExpression)
                .ToListAsync(ct);
            await webPushService.SendAsync(
                pushSubscriptions,
                new PushNotificationData.EventPublished(
                    idService.Event.Encode(eventId),
                    eventInfo.Date
                ),
                ct
            );
        }
    }
}
