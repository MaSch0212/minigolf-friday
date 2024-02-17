using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Models;

namespace MinigolfFriday.Mappers;

public static class EventMapper
{
    public static Event ToModel(this EventEntity entity)
    {
        return new Event(
            entity.Id.ToString(),
            entity.Date,
            entity.RegistrationDeadline,
            entity.Timeslots?.Select(ToModel) ?? [],
            entity.IsStarted
        );
    }

    public static EventTimeslot ToModel(this EventTimeslotEntity entity)
    {
        return new EventTimeslot(
            entity.Id.ToString(),
            entity.Time,
            entity.MapId.ToString(),
            entity.IsFallbackAllowed,
            entity.Preconfigurations.Select(ToModel),
            entity.Registrations.Select(x => x.PlayerId.ToString()).Distinct(),
            entity.Instances.Select(ToModel)
        );
    }

    public static EventInstancePreconfiguration ToModel(
        this EventInstancePreconfigurationEntity entity
    )
    {
        return new EventInstancePreconfiguration(
            entity.Id.ToString(),
            entity.Players.Select(x => x.Id.ToString())
        );
    }

    public static EventInstance ToModel(this EventInstanceEntity entity)
    {
        return new EventInstance(
            entity.Id.ToString(),
            entity.GroupCode,
            entity.Players.Select(x => x.Id.ToString())
        );
    }

    public static IQueryable<EventEntity> WithIncludes(this IQueryable<EventEntity> events)
    {
        return events
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Preconfigurations)
            .ThenInclude(x => x.Players)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Instances)
            .ThenInclude(x => x.Players)
            .AsSplitQuery();
    }
}
