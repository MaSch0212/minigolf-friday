using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Instances;

/// <param name="EventId">The id of the event to mark for being edited.</param>
/// <param name="IsEditing">Whether the user intents to edit the event instances.</param>
public record SetEventInstancesEditingRequest(
    [property: Required] string EventId,
    [property: Required] bool IsEditing
);

public class SetEventInstancesEditingRequestValidator : Validator<SetEventInstancesEditingRequest>
{
    public SetEventInstancesEditingRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Marks an event for being edited by the current user.</summary>
public class SetEventInstancesEditingEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IJwtService jwtService,
    IRealtimeEventsService realtimeEventsService
) : Endpoint<SetEventInstancesEditingRequest>
{
    public override void Configure()
    {
        Put("{eventId}/instances:editing");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(EndpointErrors.EventNotFound, EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(
        SetEventInstancesEditingRequest req,
        CancellationToken ct
    )
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }

        var eventId = idService.Event.DecodeSingle(req.EventId);
        var eventQuery = databaseContext.Events.Where(x => x.Id == eventId);
        var eventInfo = await eventQuery
            .Select(x => new { x.UserIdEditingInstances })
            .FirstOrDefaultAsync(ct);

        if (eventInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (req.IsEditing || eventInfo.UserIdEditingInstances == userId)
        {
            await eventQuery.ExecuteUpdateAsync(
                x => x.SetProperty(x => x.UserIdEditingInstances, req.IsEditing ? userId : null),
                ct
            );
            await realtimeEventsService.SendEventAsync(
                new RealtimeEvent.EventInstancesEditorChanged(
                    idService.Event.Encode(eventId),
                    req.IsEditing ? idService.User.Encode(userId) : null
                ),
                ct
            );
        }

        await SendOkAsync(ct);
    }
}
