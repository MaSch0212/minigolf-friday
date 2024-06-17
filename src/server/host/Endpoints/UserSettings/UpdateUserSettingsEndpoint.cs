using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Options;
using MinigolfFriday.Host.Services;
using MinigolfFriday.Host.Utilities;

namespace MinigolfFriday.Host.Endpoints.UserSettings;

/// <param name="UserId">The id of the user to change settings.</param>
/// <param name="EnableNotifications">Whether to enable notifications.</param>
/// <param name="NotifyOnEventPublish">Whether to notify on event publish.</param>
/// <param name="NotifyOnEventStart">Whether to notify on event start.</param>
/// <param name="NotifyOnTimeslotStart">Whether to notify on timeslot start.</param>
/// <param name="SecondsToNotifyBeforeTimeslotStart">The number of seconds to notify before a timeslot starts.</param>
public record UpdateUserSettingsRequest(
    [property: Required] string UserId,
    bool? EnableNotifications,
    bool? NotifyOnEventPublish,
    bool? NotifyOnEventStart,
    bool? NotifyOnTimeslotStart,
    int? SecondsToNotifyBeforeTimeslotStart
);

public class UpdateUserSettingsRequestValidator : Validator<UpdateUserSettingsRequest>
{
    public UpdateUserSettingsRequestValidator(IIdService idService)
    {
        RuleFor(x => x.UserId).NotEmpty().ValidSqid(idService.User);
        When(
            x => x.SecondsToNotifyBeforeTimeslotStart.HasValue,
            () => RuleFor(x => x.SecondsToNotifyBeforeTimeslotStart!.Value).InclusiveBetween(0, 60)
        );
    }
}

public class UpdateUserSettingsEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IJwtService jwtService
) : Endpoint<UpdateUserSettingsRequest>
{
    public override void Configure()
    {
        Post("");
        Group<UserSettingsGroup>();
        this.ProducesErrors(EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(UpdateUserSettingsRequest req, CancellationToken ct)
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }
    }
}
