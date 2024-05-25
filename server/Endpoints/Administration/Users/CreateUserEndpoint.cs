using FastEndpoints;
using FluentValidation;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Models;
using MinigolfFriday.Services;
using MinigolfFriday.Utilities;

namespace MinigolfFriday.Endpoints.Administration.Users;

/// <param name="Alias">The alias that is used to display the user in the UI.</param>
/// <param name="Roles">The assigned roles to the user.</param>
/// <param name="PlayerPreferences">Preferences regarding other players.</param>
public record CreateUserRequest(string Alias, Role[] Roles, PlayerPreferences PlayerPreferences);

/// <param name="User">The created user.</param>
/// <param name="LoginToken">The token the user can use to login.</param>
public record CreateUserResponse(User User, string LoginToken);

public class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator(IIdService idService)
    {
        RuleFor(x => x.Alias).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).IsInEnum();
        RuleFor(x => x.PlayerPreferences).NotNull();
        RuleFor(x => x.PlayerPreferences.Avoid)
            .NotNull()
            .ForEach(x => x.NotEmpty().ValidSqid(idService.User));
        RuleFor(x => x.PlayerPreferences.Prefer)
            .NotNull()
            .ForEach(x => x.NotEmpty().ValidSqid(idService.User));
    }
}

/// <summary>Create a new user.</summary>
public class CreateUserEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<CreateUserRequest, CreateUserResponse>
{
    public override void Configure()
    {
        Post("");
        Group<UserAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<CreateUserResponse>(201));
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var user = new UserEntity()
        {
            Alias = req.Alias,
            LoginToken = LoginTokenGenerator.GetLoginToken(),
            Roles = req.Roles.Select(databaseContext.RoleById).ToList(),
            Avoid = req.PlayerPreferences
                .Avoid
                .Select(x => databaseContext.UserById(idService.User.DecodeSingle(x)))
                .ToList(),
            Prefer = req.PlayerPreferences
                .Prefer
                .Select(x => databaseContext.UserById(idService.User.DecodeSingle(x)))
                .ToList()
        };
        databaseContext.Users.Add(user);
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(
            new(
                new(idService.User.Encode(user.Id), req.Alias, req.Roles, req.PlayerPreferences),
                user.LoginToken
            ),
            201,
            ct
        );
    }
}
