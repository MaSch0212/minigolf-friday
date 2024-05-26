using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Users;

/// <param name="UserId">The id of the user to delete.</param>
public record DeleteUserEndpointRequest(string UserId);

public class DeleteUserEndpointRequestValidator : Validator<DeleteUserEndpointRequest>
{
    public DeleteUserEndpointRequestValidator(IIdService idService)
    {
        RuleFor(x => x.UserId).NotEmpty().ValidSqid(idService.User);
    }
}

/// <summary>Delete a user.</summary>
public class DeleteUserEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IJwtService jwtService
) : Endpoint<DeleteUserEndpointRequest>
{
    public override void Configure()
    {
        Delete("{userId}");
        Group<UserAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.UserNotFound,
            EndpointErrors.UserIdNotInClaims,
            EndpointErrors.CannotDeleteSelf
        );
    }

    public override async Task HandleAsync(DeleteUserEndpointRequest req, CancellationToken ct)
    {
        var userId = idService.User.DecodeSingle(req.UserId);
        var info = await databaseContext
            .Users
            .Where(x => x.Id == userId)
            .Select(user => new { User = user, HasParticipated = user.EventInstances.Count > 0 })
            .FirstOrDefaultAsync(ct);

        if (info == null)
        {
            Logger.LogWarning(EndpointErrors.UserNotFound, userId);
            await this.SendErrorAsync(EndpointErrors.UserNotFound, req.UserId, ct);
            return;
        }

        if (!jwtService.TryGetUserId(User, out var currentUserId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }

        if (currentUserId == userId)
        {
            Logger.LogWarning(EndpointErrors.CannotDeleteSelf);
            await this.SendErrorAsync(EndpointErrors.CannotDeleteSelf, ct);
            return;
        }

        if (info.HasParticipated)
        {
            info.User.Alias = null;
            info.User.LoginToken = null;
        }
        else
        {
            databaseContext.Users.Remove(databaseContext.UserById(userId));
        }

        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(null, cancellation: ct);
    }
}
