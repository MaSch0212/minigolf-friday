using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;

namespace MinigolfFriday.Endpoints.Administration.Maps;

/// <param name="Maps">All the maps in the system.</param>
public record GetMapsResponse(MinigolfMap[] Maps);

/// <summary>Get all maps.</summary>
public class GetMapsEndpoint(DatabaseContext databaseContext, IMinigolfMapMapper minigolfMapMapper)
    : EndpointWithoutRequest<GetMapsResponse>
{
    public override void Configure()
    {
        Get("");
        Group<MapAdministrationGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var maps = await databaseContext
            .Maps
            .Select(minigolfMapMapper.MapMinigolfMapExpression)
            .ToArrayAsync(ct);

        await SendAsync(new(maps), cancellation: ct);
    }
}
