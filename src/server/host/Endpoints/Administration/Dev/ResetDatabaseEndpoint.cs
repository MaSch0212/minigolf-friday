using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Endpoints.Administration.Dev;

/// <summary>Reset the database.</summary>
public class ResetDatabaseEndpoint(DatabaseContext databaseContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("resetdb");
        Group<DevGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await databaseContext.Database.EnsureDeletedAsync(ct);
        await databaseContext.Database.MigrateAsync(ct);
        await SendAsync(null, cancellation: ct);
    }
}
