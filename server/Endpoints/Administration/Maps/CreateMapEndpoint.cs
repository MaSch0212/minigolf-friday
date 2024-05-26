using FastEndpoints;
using FluentValidation;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Maps;

/// <param name="Name">The name of the minigolf map.</param>
public record CreateMapRequest(string Name);

/// <param name="Map">The map that has been created.</param>
public record CreateMapResponse(MinigolfMap Map);

public class CreateMapRequestValidator : Validator<CreateMapRequest>
{
    public CreateMapRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

/// <summary>Create a new map.</summary>
public class CreateMapEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<CreateMapRequest, CreateMapResponse>
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
        await SendAsync(new(new(idService.Map.Encode(map.Id), req.Name)), 201, ct);
    }
}
