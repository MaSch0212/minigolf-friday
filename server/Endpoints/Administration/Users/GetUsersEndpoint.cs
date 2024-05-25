using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Users;

/// <param name="users">All users in the system.</param>
public record GetUsersResponse(User[] users);

/// <summary>Get all users.</summary>
public class GetUsersEndpoint(DatabaseContext databaseContext, IIdService idService)
    : EndpointWithoutRequest<GetUsersResponse>
{
    public override void Configure()
    {
        Get("");
        Group<UserAdministrationGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await databaseContext
            .Users
            .Where(x => x.IsActive)
            .Select(
                u =>
                    new User(
                        idService.User.Encode(u.Id),
                        u.Alias,
                        u.Roles.Select(x => x.Id).ToArray(),
                        new(
                            u.Avoid.Select(x => idService.User.Encode(x.Id)).ToArray(),
                            u.Prefer.Select(x => idService.User.Encode(x.Id)).ToArray()
                        )
                    )
            )
            .ToArrayAsync(ct);

        await SendAsync(new(users), cancellation: ct);
    }
}
