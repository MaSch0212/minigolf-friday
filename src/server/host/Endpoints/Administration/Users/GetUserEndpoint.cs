using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Users;

/// <param name="UserId">The id of the user to retrieve.</param>
public record GetUserRequest([property: Required] string UserId);

/// <param name="User">The retrieved user.</param>
public record GetuserResponse([property: Required] User User);

public class GetUserRequestValidator : Validator<GetUserRequest>
{
    public GetUserRequestValidator(IIdService idService)
    {
        RuleFor(x => x.UserId).NotEmpty().ValidSqid(idService.User);
    }
}

/// <summary>Get a user.</summary>
public class GetUserEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IUserMapper userMapper
) : Endpoint<GetUserRequest, GetuserResponse>
{
    public override void Configure()
    {
        Get("{userId}");
        Group<UserAdministrationGroup>();
        this.ProducesError(EndpointErrors.UserNotFound);
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var userId = idService.User.DecodeSingle(req.UserId);
        var user = await databaseContext
            .Users.Where(x => x.Id == userId && x.LoginToken != null)
            .Select(userMapper.MapUserExpression)
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            Logger.LogWarning(EndpointErrors.UserNotFound, userId);
            await this.SendErrorAsync(EndpointErrors.UserNotFound, req.UserId, ct);
            return;
        }

        await SendAsync(new(user), cancellation: ct);
    }
}
