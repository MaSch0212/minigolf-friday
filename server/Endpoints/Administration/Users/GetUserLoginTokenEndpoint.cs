using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Users;

/// <param name="UserId">The id of the user.</param>
public record GetUserLoginTokenRequest(string UserId);

/// <param name="LoginToken">The token the user can use to login.</param>
public record GetUserLoginTokenResponse(string LoginToken);

public class GetUserLoginTokenRequestValidator : Validator<GetUserLoginTokenRequest>
{
    public GetUserLoginTokenRequestValidator(IIdService idService)
    {
        RuleFor(x => x.UserId).NotEmpty().ValidSqid(idService.User);
    }
}

/// <summary>Get a user's login token.</summary>
public class GetUserLoginTokenEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<GetUserLoginTokenRequest, GetUserLoginTokenResponse>
{
    public override void Configure()
    {
        Get("{userId}/loginToken");
        Group<UserAdministrationGroup>();
        this.ProducesError(EndpointErrors.UserNotFound);
    }

    public override async Task HandleAsync(GetUserLoginTokenRequest req, CancellationToken ct)
    {
        var userId = idService.User.DecodeSingle(req.UserId);
        var user = await databaseContext
            .Users.Where(x => x.Id == userId)
            .Select(x => new { x.LoginToken })
            .FirstOrDefaultAsync(ct);

        if (user == null || user.LoginToken == null)
        {
            Logger.LogWarning(EndpointErrors.UserNotFound, userId);
            await this.SendErrorAsync(EndpointErrors.UserNotFound, req.UserId, ct);
            return;
        }

        await SendAsync(new(user.LoginToken), cancellation: ct);
    }
}
