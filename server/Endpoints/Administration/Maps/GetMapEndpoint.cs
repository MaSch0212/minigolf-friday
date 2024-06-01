using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Maps;

/// <param name="MapId">The id of the map to retrieve.</param>
public record GetMapRequest(string MapId);

/// <param name="Map">The retrieved map.</param>
public record GetMapResponse(MinigolfMap Map);

public class GetMapRequestValidator : Validator<GetMapRequest>
{
    public GetMapRequestValidator(IIdService idService)
    {
        RuleFor(x => x.MapId).NotEmpty().ValidSqid(idService.Map);
    }
}

/// <summary>Get a map.</summary>
public class GetMapEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IMinigolfMapMapper minigolfMapMapper
) : Endpoint<GetMapRequest, GetMapResponse>
{
    public override void Configure()
    {
        Get("{mapId}");
        Group<MapAdministrationGroup>();
        this.ProducesError(EndpointErrors.MapNotFound);
    }

    public override async Task HandleAsync(GetMapRequest req, CancellationToken ct)
    {
        var mapId = idService.Map.DecodeSingle(req.MapId);
        var map = await databaseContext
            .Maps.Where(x => x.Id == mapId)
            .Select(minigolfMapMapper.MapMinigolfMapExpression)
            .FirstOrDefaultAsync(ct);

        if (map == null)
        {
            Logger.LogWarning(EndpointErrors.MapNotFound, mapId);
            await this.SendErrorAsync(EndpointErrors.MapNotFound, req.MapId, ct);
            return;
        }

        await SendAsync(new(map), cancellation: ct);
    }
}
