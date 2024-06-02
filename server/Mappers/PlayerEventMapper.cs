using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Mappers;

[GenerateAutoInterface]
public class PlayerEventMapper(IIdService idService) : IPlayerEventMapper
{
    public PlayerEvent Map(EventEntity entity, long userId)
    {
        return new PlayerEvent(
            idService.Event.Encode(entity.Id),
            entity.Date,
            entity.RegistrationDeadline,
            entity
                .Timeslots.OrderBy(x => x.Time)
                .Select(timeslot => Map(timeslot, userId))
                .ToArray(),
            entity.StartedAt != null
        );
    }

    public PlayerEventTimeslot Map(EventTimeslotEntity entity, long userId)
    {
        return new PlayerEventTimeslot(
            idService.EventTimeslot.Encode(entity.Id),
            entity.Time,
            entity.IsFallbackAllowed,
            entity.Registrations.Any(reg => reg.PlayerId == userId),
            entity
                .Registrations.Where(reg => reg.PlayerId == userId)
                .Select(reg =>
                    reg.FallbackEventTimeslot != null
                        ? idService.EventTimeslot.Encode(reg.FallbackEventTimeslot.Id)
                        : null
                )
                .FirstOrDefault(),
            entity.Event.StartedAt != null
                ? entity
                    .Instances.Where(i => i.Players.Any(p => p.Id == userId))
                    .Select(Map)
                    .FirstOrDefault()
                : null
        );
    }

    public PlayerEventInstance Map(EventInstanceEntity entity)
    {
        return new(
            idService.EventInstance.Encode(entity.Id),
            entity.GroupCode,
            Map(entity.EventTimeslot.Map)
        );
    }

    public MinigolfMap Map(MinigolfMapEntity entity)
    {
        return new(idService.Map.Encode(entity.Id), entity.Name);
    }

    public IQueryable<EventEntity> AddIncludes(IQueryable<EventEntity> events)
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
