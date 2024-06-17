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
/// <param name="TimeslotRegistrations">The registrations to change to.</param>
public record UpdateUserSettingsRequest(
    [property: Required] string UserId,
    bool EnableNotifications,
    bool NotifyOnEventPublish,
    bool NotifyOnEventStart,
    bool NotifyOnTimeslotStart,
    bool SecondsToNotifyBeforeTimeslotStart
);

public class UpdateUserSettingsRequestValidator : Validator<UpdateUserSettingsRequest>
{
    public UpdateUserSettingsRequestValidator(IIdService idService)
    {
        // RuleFor(x => x.UserId).NotEmpty().ValidSqid(idService.Event);
        // RuleFor(x => x.TimeslotRegistrations)
        //     .ForEach(x =>
        //         x.ChildRules(x =>
        //         {
        //             x.RuleFor(x => x.TimeslotId).NotEmpty().ValidSqid(idService.EventTimeslot);
        //             x.When(
        //                 x => x.FallbackTimeslotId != null,
        //                 () =>
        //                     x.RuleFor(x => x.FallbackTimeslotId!).ValidSqid(idService.EventTimeslot)
        //             );
        //         })
        //     );
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
        Post("usersettings");
        Group<UserSettingsGroup>();
        // this.ProducesErrors(
        //     EndpointErrors.UserIdNotInClaims,
        //     EndpointErrors.EventNotFound,
        //     EndpointErrors.EventRegistrationElapsed,
        //     EndpointErrors.EventAlreadyStarted
        // );
    }

    public override async Task HandleAsync(UpdateUserSettingsRequest req, CancellationToken ct)
    {
        // console.log("update settings");
        // if (!jwtService.TryGetUserId(User, out var userId))
        // {
        //     Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
        //     await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
        //     return;
        // }

        // var eventId = idService.Event.DecodeSingle(req.EventId);
        // var eventInfo = await databaseContext
        //     .Events.Where(x => x.Id == eventId)
        //     .Select(x => new { Started = x.StartedAt != null, x.RegistrationDeadline })
        //     .FirstOrDefaultAsync(ct);

        // if (eventInfo == null)
        // {
        //     Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
        //     await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
        //     return;
        // }

        // if (eventInfo.RegistrationDeadline < DateTimeOffset.Now)
        // {
        //     Logger.LogWarning(
        //         EndpointErrors.EventRegistrationElapsed,
        //         eventId,
        //         eventInfo.RegistrationDeadline
        //     );
        //     await this.SendErrorAsync(
        //         EndpointErrors.EventRegistrationElapsed,
        //         req.EventId,
        //         eventInfo.RegistrationDeadline,
        //         ct
        //     );
        //     return;
        // }

        // if (eventInfo.Started)
        // {
        //     Logger.LogWarning(EndpointErrors.EventAlreadyStarted, eventId);
        //     await this.SendErrorAsync(EndpointErrors.EventAlreadyStarted, req.EventId, ct);
        //     return;
        // }

        // var registrations = await databaseContext
        //     .EventTimeslotRegistrations.Where(x =>
        //         x.Player.Id == userId && x.EventTimeslot.EventId == eventId
        //     )
        //     .ToArrayAsync(ct);
        // var targetRegistrations = req
        //     .TimeslotRegistrations.Select(x => new
        //     {
        //         TimeslotId = idService.EventTimeslot.DecodeSingle(x.TimeslotId),
        //         FallbackTimeslotId = x.FallbackTimeslotId == null
        //             ? null
        //             : (long?)idService.EventTimeslot.DecodeSingle(x.FallbackTimeslotId)
        //     })
        //     .ToArray();
        // databaseContext.EventTimeslotRegistrations.RemoveRange(
        //     registrations.Where(x =>
        //         !targetRegistrations.Any(y => y.TimeslotId == x.EventTimeslotId)
        //     )
        // );
        // foreach (var reg in targetRegistrations)
        // {
        //     var existing = registrations.FirstOrDefault(x => x.EventTimeslotId == reg.TimeslotId);
        //     var fallbackTimeslot =
        //         reg.FallbackTimeslotId == null
        //             ? null
        //             : databaseContext.EventTimeslotById(reg.FallbackTimeslotId.Value);
        //     if (existing is null)
        //     {
        //         databaseContext.EventTimeslotRegistrations.Add(
        //             new EventTimeslotRegistrationEntity
        //             {
        //                 EventTimeslot = databaseContext.EventTimeslotById(reg.TimeslotId),
        //                 Player = databaseContext.UserById(userId),
        //                 FallbackEventTimeslot = fallbackTimeslot
        //             }
        //         );
        //     }
        //     else
        //     {
        //         existing.FallbackEventTimeslot = fallbackTimeslot;
        //     }
        // }
        // await databaseContext.SaveChangesAsync(ct);
        // await SendAsync(null, cancellation: ct);
    }
}
