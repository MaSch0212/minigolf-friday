using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Maps;

/// <param name="MapId">The id of the map to update.</param>
/// <param name="Name">The new name. Omit to leave unchanged.</param>
public record UpdateMapRequest([property: Required] string MapId, string? Name);

public class UpdateMapRequestValidator : Validator<UpdateMapRequest>
{
    public UpdateMapRequestValidator(IIdService idService)
    {
        RuleFor(x => x.MapId).NotEmpty().ValidSqid(idService.Map);
        RuleFor(x => x.Name)
            .Must(x => x == null || !string.IsNullOrWhiteSpace(x))
            .WithMessage("'Name' must not be empty.");
    }
}

/// <summary>Update a map.</summary>
public class UpdateMapEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<UpdateMapRequest>
{
    public override void Configure()
    {
        Patch("{mapId}");
        Group<MapAdministrationGroup>();
        this.ProducesError(EndpointErrors.MapNotFound);
    }

    public override async Task HandleAsync(UpdateMapRequest req, CancellationToken ct)
    {
        var mapId = idService.Map.DecodeSingle(req.MapId);
        var map = await databaseContext.Maps.FirstOrDefaultAsync(x => x.Id == mapId, ct);
        if (map == null)
        {
            Logger.LogWarning(EndpointErrors.MapNotFound, mapId);
            await this.SendErrorAsync(EndpointErrors.MapNotFound, req.MapId, ct);
            return;
        }

        map.Name = req.Name ?? map.Name;
        await databaseContext.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }
}
