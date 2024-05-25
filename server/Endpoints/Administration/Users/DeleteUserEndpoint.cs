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
public class DeleteUserEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<DeleteUserEndpointRequest>
{
    public override void Configure()
    {
        Delete("{userId}");
        Group<UserAdministrationGroup>();
        this.ProducesError(EndpointErrors.UserNotFound);
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

        if (info.HasParticipated)
        {
            info.User.IsActive = false;
            info.User.Alias = "";
            info.User.LoginToken = "";
        }
        else
        {
            databaseContext.Users.Remove(databaseContext.UserById(userId));
        }

        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(null, cancellation: ct);
    }
}
