using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;
using Settings = MinigolfFriday.Domain.Models.UserSettings;

namespace MinigolfFriday.Host.Endpoints.UserSettings;

/// <param name="Settings">The settings of the user.</param>
public record GetUserSettingsResponse([property: Required] Settings Settings);

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
