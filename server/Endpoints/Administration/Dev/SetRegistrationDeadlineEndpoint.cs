using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Dev;

public record SetRegistrationDeadlineRequest(string EventId, DateTimeOffset Deadline);

public class SetRegistrationDeadlineEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<SetRegistrationDeadlineRequest>
{
    public override void Configure()
    {
        Post("setdeadline");
        Group<DevGroup>();
    }

    public override async Task HandleAsync(SetRegistrationDeadlineRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);

        await databaseContext.Events.ExecuteUpdateAsync(
            x => x.SetProperty(x => x.RegistrationDeadline, req.Deadline),
            ct
        );
        await SendAsync(null, cancellation: ct);
    }
}
