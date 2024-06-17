using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.User.Settings;

/// <param name="Settings">The settings of the user.</param>
public record GetUserSettingsResponse(UserSettings Settings);

public class GetUserSettingsEndpoint(
    DatabaseContext databaseContext,
    IJwtService jwtService,
    IUserSettingsMapper userSettingsMapper
) : EndpointWithoutRequest<GetUserSettingsResponse>
{
    public override void Configure()
    {
        Get("");
        Group<UserSettingsGroup>();
        this.ProducesErrors(EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }

        var settings = await databaseContext
            .Users.Where(x => x.Id == userId)
            .Select(userSettingsMapper.MapUserToUserSettingsExpression)
            .FirstOrDefaultAsync(ct);
        await SendAsync(new(settings ?? new()), cancellation: ct);
    }
}
