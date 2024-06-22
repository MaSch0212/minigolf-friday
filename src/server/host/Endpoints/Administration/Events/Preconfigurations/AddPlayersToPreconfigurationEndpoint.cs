using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Preconfigurations;

/// <param name="PreconfigurationId">The id of the event instance preconfiguration to add players to.</param>
/// <param name="PlayerIds">The ids of the players to add.</param>
public record AddPlayersToPreconfigurationRequest(
    [property: Required] string PreconfigurationId,
    [property: Required] string[] PlayerIds
);

public class AddPlayersToPreconfigurationRequestValidator
    : Validator<AddPlayersToPreconfigurationRequest>
{
    public AddPlayersToPreconfigurationRequestValidator(IIdService idService)
    {
        RuleFor(x => x.PreconfigurationId).NotEmpty().ValidSqid(idService.Preconfiguration);
        RuleFor(x => x.PlayerIds).NotEmpty().ForEach(y => y.NotEmpty().ValidSqid(idService.User));
    }
}

/// <summary>Add players to an event instance preconfiguration.</summary>
public class AddPlayersToPreconfigurationEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService
) : Endpoint<AddPlayersToPreconfigurationRequest>
{
    public override void Configure()
    {
        Post(":preconfigs/{preconfigurationId}/players");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.PreconfigurationNotFound,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(
        AddPlayersToPreconfigurationRequest req,
        CancellationToken ct
    )
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

        var entity = databaseContext.PreconfigurationById(preconfigId);
        entity.Players.AddRange(
            req.PlayerIds.Select(x => databaseContext.UserById(idService.User.DecodeSingle(x)))
        );
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(null, cancellation: ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.EventPreconfigurationChanged(
                idService.Event.Encode(preconfigInfo.EventId),
                idService.EventTimeslot.Encode(preconfigInfo.TimeslotId),
                idService.Preconfiguration.Encode(preconfigId),
                RealtimeEventChangeType.Updated
            ),
            ct
        );
    }
}
