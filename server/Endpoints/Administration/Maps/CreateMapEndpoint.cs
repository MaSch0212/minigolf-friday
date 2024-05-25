using FastEndpoints;
using FluentValidation;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Maps;

/// <param name="Name">The name of the minigolf map.</param>
public record AddMapRequest(string Name);

/// <param name="Map">The map that has been created.</param>
public record AddMapResponse(MinigolfMap Map);

public class AddMapRequestValidator : Validator<AddMapRequest>
{
    public AddMapRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

/// <summary>Create a new map.</summary>
public class CreateMapEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<AddMapRequest, AddMapResponse>
{
    public override void Configure()
    {
        Post("");
        Group<MapAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<AddMapResponse>(201));
    }

    public override async Task HandleAsync(AddMapRequest req, CancellationToken ct)
    {
        var map = new MinigolfMapEntity { Name = req.Name };
        databaseContext.Maps.Add(map);
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(new(new(idService.Map.Encode(map.Id), req.Name)), 201, ct);
    }
}
