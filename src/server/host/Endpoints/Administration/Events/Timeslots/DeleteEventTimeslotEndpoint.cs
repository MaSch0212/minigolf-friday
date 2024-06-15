using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Timeslots;

/// <param name="TimeslotId">The id of the event timeslot to delete.</param>
public record DeleteEventTimeslotRequest([property: Required] string TimeslotId);

public class DeleteEventTimeslotRequestValidator : Validator<DeleteEventTimeslotRequest>
{
    public DeleteEventTimeslotRequestValidator(IIdService idService)
    {
        RuleFor(x => x.TimeslotId).NotEmpty().ValidSqid(idService.EventTimeslot);
    }
}

/// <summary>Delete an event timeslot.</summary>
public class DeleteEventTimeslotEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<DeleteEventTimeslotRequest>
{
    public override void Configure()
    {
        Delete(":timeslots/{timeslotId}");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.EventTimeslotNotFound,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(DeleteEventTimeslotRequest req, CancellationToken ct)
    {
        var timeslotId = idService.EventTimeslot.DecodeSingle(req.TimeslotId);
        var timeslotQuery = databaseContext.EventTimeslots.Where(x => x.Id == timeslotId);
        var timeslotInfo = await timeslotQuery
            .Select(x => new { EventStarted = x.Event.StartedAt != null, x.EventId })
            .FirstOrDefaultAsync(ct);

        if (timeslotInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventTimeslotNotFound, timeslotId);
            await this.SendErrorAsync(EndpointErrors.EventTimeslotNotFound, req.TimeslotId, ct);
            return;
        }

        if (timeslotInfo.EventStarted)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, timeslotInfo.EventId);
            await this.SendErrorAsync(
                EndpointErrors.EventAlreadyStarted,
                idService.Event.Encode(timeslotInfo.EventId),
                ct
            );
            return;
        }

        await timeslotQuery.ExecuteDeleteAsync(ct);
        await SendAsync(null, cancellation: ct);
    }
}
