using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Options;
using MinigolfFriday.Host.Services;
using MinigolfFriday.Host.Utilities;

namespace MinigolfFriday.Host.Endpoints.Administration.Users;

/// <param name="Alias">The alias that is used to display the user in the UI.</param>
/// <param name="Roles">The assigned roles to the user.</param>
/// <param name="PlayerPreferences">Preferences regarding other players.</param>
public record CreateUserRequest(
    [property: Required] string Alias,
    [property: Required] Role[] Roles,
    [property: Required] PlayerPreferences PlayerPreferences
);

/// <param name="User">The created user.</param>
/// <param name="LoginToken">The token the user can use to login.</param>
public record CreateUserResponse(
    [property: Required] User User,
    [property: Required] string LoginToken
);

public class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator(IIdService idService)
    {
        RuleFor(x => x.Alias).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).IsInEnum();
        RuleFor(x => x.PlayerPreferences)
            .NotNull()
            .ChildRules(x =>
            {
                x.RuleFor(x => x.Avoid)
                    .NotNull()
                    .ForEach(x => x.NotEmpty().ValidSqid(idService.User));
                x.RuleFor(x => x.Prefer)
                    .NotNull()
                    .ForEach(x => x.NotEmpty().ValidSqid(idService.User));
            });
    }
}

/// <summary>Create a new user.</summary>
public class CreateUserEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService,
    IUserMapper userMapper
) : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("");
        Group<UserAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<CreateUserResponse>(201));
        this.ProducesError(EndpointErrors.UserExists);
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var existentUser = await databaseContext
            .Users.Where(x => x.Alias == req.Alias)
            .Select(userMapper.MapUserExpression)
            .FirstOrDefaultAsync(ct);
        if (existentUser != null)
        {
            await this.SendErrorAsync(EndpointErrors.UserExists, existentUser.Alias, ct);
            return;
        }

        var user = new UserEntity()
        {
            Alias = req.Alias,
            LoginToken = LoginTokenGenerator.GetLoginToken(),
            Roles = req.Roles.Select(databaseContext.RoleById).ToList(),
            Avoid = req
                .PlayerPreferences.Avoid.Select(x =>
                    databaseContext.UserById(idService.User.DecodeSingle(x))
                )
                .ToList(),
            Prefer = req
                .PlayerPreferences.Prefer.Select(x =>
                    databaseContext.UserById(idService.User.DecodeSingle(x))
                )
                .ToList()
        };
        databaseContext.Users.Add(user);
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(
            new(
                new(
                    idService.User.Encode(user.Id),
                    req.Alias,
                    req.Roles,
                    req.PlayerPreferences,
                    false
                ),
                user.LoginToken
            ),
            201,
            ct
        );

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.UserChanged(
                idService.User.Encode(user.Id),
                RealtimeEventChangeType.Created
            ),
            ct
        );
    }
}
