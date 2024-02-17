using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Models;

namespace MinigolfFriday.Mappers;

public static class PlayerEventMapper
{
    public static PlayerEvent ToPlayerModel(this EventEntity entity, Guid userId)
    {
        return new PlayerEvent(
            entity.Id.ToString(),
            entity.Date,
            entity.RegistrationDeadline,
            entity.Timeslots.Select(timeslot => timeslot.ToPlayerModel(userId)),
            entity.IsStarted
        );
    }

    public static PlayerEventTimeslot ToPlayerModel(this EventTimeslotEntity entity, Guid userId)
    {
        return new PlayerEventTimeslot(
            entity.Id.ToString(),
            entity.Time,
            entity.IsFallbackAllowed,
            entity.Registrations.Any(reg => reg.PlayerId == userId),
            entity
                .Registrations
                .Where(reg => reg.PlayerId == userId)
                .Select(reg => reg.FallbackEventTimeslotId?.ToString())
                .FirstOrDefault(),
            entity.Event.IsStarted
                ? entity
                    .Instances
                    .Where(i => i.Players.Any(p => p.Id == userId))
                    .Select(x => x.ToPlayerModel())
                    .FirstOrDefault()
                : null
        );
    }

    public static PlayerEventInstance ToPlayerModel(this EventInstanceEntity entity)
    {
        return new PlayerEventInstance(
            entity.Id.ToString(),
            entity.GroupCode,
            entity.EventTimeSlot.Map.ToModel(),
            entity.Players.Select(p => p.ToPlayerModel())
        );
    }

    public static Player ToPlayerModel(this UserEntity entity)
    {
        return new Player(entity.Id.ToString(), entity.Name);
    }

    public static IQueryable<EventEntity> WithPlayerIncludes(this IQueryable<EventEntity> events)
    {
        return events
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Instances)
            .ThenInclude(x => x.Players)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Map);
    }
}
