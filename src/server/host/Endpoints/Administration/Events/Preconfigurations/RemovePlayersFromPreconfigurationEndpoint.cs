using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Preconfigurations;

/// <param name="PreconfigurationId">The id of the event instance preconfiguration to remove players from.</param>
/// <param name="PlayerIds">The ids of the players to remove from the event instance preconfiguration.</param>
public record RemovePlayersFromPreconfigurationRequest(
    [property: Required] string PreconfigurationId,
    [property: Required] string[] PlayerIds
);

public class RemovePlayersFromPreconfigurationRequestValidator
    : Validator<RemovePlayersFromPreconfigurationRequest>
{
    public RemovePlayersFromPreconfigurationRequestValidator(IIdService idService)
    {
        RuleFor(x => x.PreconfigurationId).NotEmpty().ValidSqid(idService.Preconfiguration);
        RuleFor(x => x.PlayerIds).NotEmpty().ForEach(y => y.NotEmpty().ValidSqid(idService.User));
    }
}

/// <summary>Remove players from an event instance preconfiguration.</summary>
public class RemovePlayersFromPreconfigurationEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService
) : Endpoint<RemovePlayersFromPreconfigurationRequest>
{
    public override void Configure()
    {
        Delete(":preconfigs/{preconfigurationId}/players");
        Group<EventAdministrationGroup>();
        Description(x =>
            x.ClearDefaultAccepts()
                .Accepts<RemovePlayersFromPreconfigurationRequest>("application/json")
        );
        this.ProducesErrors(
            EndpointErrors.PreconfigurationNotFound,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(
        RemovePlayersFromPreconfigurationRequest req,
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

        // TODO: Optimize
        var entity = await preconfigQuery.Include(x => x.Players).FirstAsync(ct);
        foreach (var playerId in req.PlayerIds)
            entity.Players.Remove(databaseContext.UserById(idService.User.DecodeSingle(playerId)));
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
