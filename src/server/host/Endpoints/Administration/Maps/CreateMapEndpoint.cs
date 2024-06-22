using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Maps;

/// <param name="Name">The name of the minigolf map.</param>
public record CreateMapRequest([property: Required] string Name);

/// <param name="Map">The map that has been created.</param>
public record CreateMapResponse([property: Required] MinigolfMap Map);

public class CreateMapRequestValidator : Validator<CreateMapRequest>
{
    public CreateMapRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

/// <summary>Create a new map.</summary>
public class CreateMapEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService
) : Endpoint<CreateMapRequest, CreateMapResponse>
{
    public override void Configure()
    {
        Post("");
        Group<MapAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<CreateMapResponse>(201));
    }

    public override async Task HandleAsync(CreateMapRequest req, CancellationToken ct)
    {
        var map = new MinigolfMapEntity { Name = req.Name };
        databaseContext.Maps.Add(map);
        await databaseContext.SaveChangesAsync(ct);
        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.MapChanged(
                idService.Map.Encode(map.Id),
                RealtimeEventChangeType.Created
            ),
            ct
        );
        await SendAsync(new(new(idService.Map.Encode(map.Id), req.Name)), 201, ct);
    }
}
