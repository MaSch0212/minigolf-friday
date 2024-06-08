using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Mappers;

namespace MinigolfFriday.Endpoints.Administration.Users;

/// <param name="Users">All users in the system.</param>
public record GetUsersResponse([property: Required] User[] Users);

/// <summary>Get all users.</summary>
public class GetUsersEndpoint(DatabaseContext databaseContext, IUserMapper userMapper)
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
            .Users.Where(x => x.LoginToken != null)
            .Select(userMapper.MapUserExpression)
            .ToArrayAsync(ct);

        await SendAsync(new(users), cancellation: ct);
    }
}
