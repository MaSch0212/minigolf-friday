using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Timeslots;

/// <param name="TimeslotId">The id of the timeslot to update.</param>
/// <param name="MapId">The id of the map that should be played.</param>
/// <param name="IsFallbackAllowed">Determines whether players can define a fallback timeslot. Players will participate in a fallback timeslot if this timeslot does not take place due to too few players.</param>
public record UpdateEventTimeslotRequest(
    [property: Required] string TimeslotId,
    string? MapId,
    bool? IsFallbackAllowed
);

public class UpdateEventTimeslotRequestValidator : Validator<UpdateEventTimeslotRequest>
{
    public UpdateEventTimeslotRequestValidator(IIdService idService)
    {
        RuleFor(x => x.TimeslotId).NotEmpty().ValidSqid(idService.EventTimeslot);
        When(x => x.MapId != null, () => RuleFor(x => x.MapId!).ValidSqid(idService.Map));
    }
}

/// <summary>Update an event timeslot.</summary>
public class UpdateEventTimeslotEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<UpdateEventTimeslotRequest>
{
    public override void Configure()
    {
        Patch(":timeslots/{timeslotId}");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.EventTimeslotNotFound,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(UpdateEventTimeslotRequest req, CancellationToken ct)
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

        var updateBuilder = DbUpdateBuilder.Create(timeslotQuery);

        if (req.MapId != null)
        {
            var mapId = idService.Map.DecodeSingle(req.MapId);
            var mapExists = await databaseContext.Maps.AnyAsync(x => x.Id == mapId, ct);
            if (mapExists)
            {
                updateBuilder.With(x => x.SetProperty(x => x.MapId, mapId));
            }
            else
            {
                Logger.LogWarning(EndpointErrors.MapNotFound, mapId);
                ValidationFailures.Add(
                    new ValidationFailure(nameof(req.MapId), "Map does not exist.")
                );
            }
        }

        if (req.IsFallbackAllowed != null)
            updateBuilder.With(x => x.SetProperty(x => x.IsFallbackAllowed, req.IsFallbackAllowed));

        ThrowIfAnyErrors();

        await updateBuilder.ExecuteAsync(ct);
        await SendAsync(null, cancellation: ct);
    }
}
