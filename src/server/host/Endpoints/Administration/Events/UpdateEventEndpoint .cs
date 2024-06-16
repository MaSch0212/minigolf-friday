using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events;

// / <param name="EventId">The id of the event.</param>
// / <param name="Commit">set true if you want to commit the event and send a notification to the user</param>
public record UpdateEventRequest(
    [property: Required] string EventId,
    [property: Required] bool Commit
);

/// <param name="Event">The Updated event.</param>
public record UpdateEventResponse([property: Required] Event Event);

public class UpdateEventRequestValidator : Validator<UpdateEventRequest>
{
    public UpdateEventRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
        RuleFor(x => x.Commit);
    }
}

/// <summary>Update a new event.</summary>
public class UpdateEventEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IEventMapper eventMapper,
    IUserPushSubscriptionMapper userPushSubscriptionMapper,
    IWebPushService webPushService
) : Endpoint<UpdateEventRequest, UpdateEventResponse>
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

        if (!eventInfo.Staged)
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

        updateBuilder.With(x => x.SetProperty(x => x.Staged, false));

        ThrowIfAnyErrors();

        await updateBuilder.ExecuteAsync(ct);
        await SendAsync(null, cancellation: ct);

        var pushSubscriptions = await databaseContext
            .UserPushSubscriptions.Select(
                userPushSubscriptionMapper.MapUserPushSubscriptionExpression
            )
            .ToListAsync(ct);
        await webPushService.SendAsync(
            pushSubscriptions,
            new PushNotificationData.EventPublished(
                idService.Event.Encode(eventId),
                eventInfo.Date
            ),
            ct
        );

        await SendAsync(new(eventMapper.Map(eventInfo)), 201, ct);
    }
}
