using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using MaSch.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Users;

/// <param name="UserId">The id of the user to update.</param>
/// <param name="Alias">The new alias. Omit to leave unchanged.</param>
/// <param name="AddRoles">Roles to add to the user.</param>
/// <param name="RemoveRoles">Roles to remove from the user.</param>
/// <param name="PlayerPreferences">Changes to the player preferences.</param>
public record UpdateUserRequest(
    [property: Required] string UserId,
    string? Alias,
    Role[]? AddRoles,
    Role[]? RemoveRoles,
    UpdatePlayerPreferences? PlayerPreferences
);

/// <summary>
/// Represents changes to the player preferences
/// </summary>
/// <param name="AddAvoid">Ids of players to add to avoided players.</param>
/// <param name="RemoveAvoid">Ids of players to remove from avoided players.</param>
/// <param name="AddPrefer">Ids of players to add to preferred players.</param>
/// <param name="RemovePrefer">Ids of players to remove from preferred players.</param>
public record UpdatePlayerPreferences(
    string[]? AddAvoid,
    string[]? RemoveAvoid,
    string[]? AddPrefer,
    string[]? RemovePrefer
);

public class UpdateUserRequestValidator : Validator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(IIdService idService)
    {
        RuleFor(x => x.UserId).NotEmpty().ValidSqid(idService.User);
        RuleFor(x => x.Alias)
            .Must(x => x == null || !string.IsNullOrWhiteSpace(x))
            .WithMessage("'Alias' must not be empty.");
        RuleForEach(x => x.AddRoles).IsInEnum();
        RuleForEach(x => x.RemoveRoles).IsInEnum();
        RuleFor(x => x.PlayerPreferences)
            .ChildRules(x =>
            {
                x.RuleForEach(x => x!.AddAvoid).NotEmpty().ValidSqid(idService.User);
                x.RuleForEach(x => x!.AddPrefer).NotEmpty().ValidSqid(idService.User);
                x.RuleForEach(x => x!.RemoveAvoid).NotEmpty().ValidSqid(idService.User);
                x.RuleForEach(x => x!.RemovePrefer).NotEmpty().ValidSqid(idService.User);
            });
    }
}

/// <summary>Update a user.</summary>
public class UpdateUserEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService
) : Endpoint<UpdateUserRequest>
{
    public override void Configure()
    {
        Patch("{userId}");
        Group<UserAdministrationGroup>();
        this.ProducesError(EndpointErrors.UserNotFound);

        // TODO: Validate that user cannot remove admin role from themselfes
        // TODO: Validate Avoid and Prefer is not this user
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var userId = idService.User.DecodeSingle(req.UserId);
        var user = await databaseContext
            .Users.Include(x => x.Roles)
            .Include(x => x.Avoid)
            .Include(x => x.Prefer)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        if (user == null || user.LoginToken == null)
        {
            Logger.LogWarning(EndpointErrors.UserNotFound, userId);
            await this.SendErrorAsync(EndpointErrors.UserNotFound, req.UserId, ct);
            return;
        }

        user.Alias = req.Alias ?? user.Alias;

        req.AddRoles?.Select(databaseContext.RoleById).ForEach(user.Roles.Add);
        req.RemoveRoles?.Select(databaseContext.RoleById).ForEach(x => user.Roles.Remove(x));

        req.PlayerPreferences?.RemoveAvoid?.Select(x =>
                databaseContext.UserById(idService.User.DecodeSingle(x))
            )
            .ForEach(x => user.Avoid.Remove(x));
        req.PlayerPreferences?.RemovePrefer?.Select(x =>
                databaseContext.UserById(idService.User.DecodeSingle(x))
            )
            .ForEach(x => user.Prefer.Remove(x));

        req.PlayerPreferences?.AddAvoid?.Select(x =>
                databaseContext.UserById(idService.User.DecodeSingle(x))
            )
            .ForEach(x =>
            {
                if (user.Prefer.Any(y => y.Id == x.Id))
                    AddError(
                        $"User \"{idService.User.Encode(x.Id)}\" cannot be avoided as it is already preferred."
                    );
                else
                    user.Avoid.Add(x);
            });
        req.PlayerPreferences?.AddPrefer?.Select(x =>
                databaseContext.UserById(idService.User.DecodeSingle(x))
            )
            .ForEach(x =>
            {
                if (user.Avoid.Any(y => y.Id == x.Id))
                    AddError(
                        $"User \"{idService.User.Encode(x.Id)}\" cannot be preferred as it is already avoided."
                    );
                else
                    user.Prefer.Add(x);
            });

        ThrowIfAnyErrors();
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(null, cancellation: ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.UserChanged(
                idService.User.Encode(userId),
                RealtimeEventChangeType.Updated
            ),
            ct
        );
    }
}
