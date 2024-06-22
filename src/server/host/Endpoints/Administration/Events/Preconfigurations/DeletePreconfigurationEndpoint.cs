using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Preconfigurations;

/// <param name="PreconfigurationId">The id of the event instance preconfiguration to delete.</param>
public record DeletePreconfigurationRequest([property: Required] string PreconfigurationId);

public class DeletePreconfigurationRequestValidator : Validator<DeletePreconfigurationRequest>
{
    public DeletePreconfigurationRequestValidator(IIdService idService)
    {
        RuleFor(x => x.PreconfigurationId).NotEmpty().ValidSqid(idService.Preconfiguration);
    }
}

/// <summary>Delete an event instance preconfiguration.</summary>
public class DeletePreconfigurationEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService
) : Endpoint<DeletePreconfigurationRequest>
{
    public override void Configure()
    {
        Delete(":preconfigs/{preconfigurationId}");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.PreconfigurationNotFound,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(DeletePreconfigurationRequest req, CancellationToken ct)
    {
        var preconfigId = idService.Preconfiguration.DecodeSingle(req.PreconfigurationId);
        var preconfigQuery = databaseContext.EventInstancePreconfigurations.Where(x =>
            x.Id == preconfigId
        );
        var preconfigInfo = await preconfigQuery
            .Select(x => new
            {
                Started = x.EventTimeSlot.Event.StartedAt != null,
                x.EventTimeSlot.EventId,
                TimeslotId = x.EventTimeSlot.Id
            })
            .FirstOrDefaultAsync(ct);

        if (preconfigInfo == null)
        {
            Logger.LogWarning(EndpointErrors.PreconfigurationNotFound, preconfigId);
            await this.SendErrorAsync(
                EndpointErrors.PreconfigurationNotFound,
                req.PreconfigurationId,
                ct
            );
            return;
        }

        if (preconfigInfo.Started)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, preconfigInfo.EventId);
            await this.SendErrorAsync(
                EndpointErrors.EventAlreadyStarted,
                idService.Event.Encode(preconfigInfo.EventId),
                ct
            );
            return;
        }

        await preconfigQuery.ExecuteDeleteAsync(ct);
        await SendAsync(null, cancellation: ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.EventPreconfigurationChanged(
                idService.Event.Encode(preconfigInfo.EventId),
                idService.EventTimeslot.Encode(preconfigInfo.TimeslotId),
                idService.Preconfiguration.Encode(preconfigId),
                RealtimeEventChangeType.Deleted
            ),
            ct
        );
    }
}
