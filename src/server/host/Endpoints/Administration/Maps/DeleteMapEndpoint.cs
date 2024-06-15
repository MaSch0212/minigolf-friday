using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Maps;

/// <param name="MapId">The id of the map to delete.</param>
public record DeleteMapRequest([property: Required] string MapId);

public class DeleteMapRequestValidator : Validator<DeleteMapRequest>
{
    public DeleteMapRequestValidator(IIdService idService)
    {
        RuleFor(x => x.MapId).NotEmpty().ValidSqid(idService.Map);
    }
}

/// <summary>Delete a map.</summary>
public class DeleteMapEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<DeleteMapRequest>
{
    public override void Configure()
    {
        Delete("{mapId}");
        Group<MapAdministrationGroup>();
        this.ProducesError(EndpointErrors.MapNotFound);
    }

    public override async Task HandleAsync(DeleteMapRequest req, CancellationToken ct)
    {
        var mapId = idService.Map.DecodeSingle(req.MapId);
        var info = await databaseContext
            .Maps.Where(x => x.Id == mapId)
            .Select(map => new { Map = map, HasBeenPlayed = map.EventTimeslots.Count > 0 })
            .FirstOrDefaultAsync(ct);
        if (info == null)
        {
            Logger.LogWarning(EndpointErrors.MapNotFound, mapId);
            await this.SendErrorAsync(EndpointErrors.MapNotFound, req.MapId, ct);
            return;
        }

        if (info.HasBeenPlayed)
            info.Map.IsActive = false;
        else
            databaseContext.Maps.Remove(info.Map);

        await databaseContext.SaveChangesAsync(ct);
        await SendOkAsync(ct);
    }
}
