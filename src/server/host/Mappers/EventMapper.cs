using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Mappers;

[GenerateAutoInterface]
public class EventMapper(IIdService idService) : IEventMapper
{
    public Event Map(EventEntity entity)
    {
        return new Event(
            idService.Event.Encode(entity.Id),
            entity.Date,
            entity.RegistrationDeadline,
            entity.Timeslots?.Select(Map).ToArray() ?? [],
            entity.StartedAt
        );
    }

    public EventTimeslot Map(EventTimeslotEntity entity)
    {
        return new EventTimeslot(
            idService.EventTimeslot.Encode(entity.Id),
            entity.Time,
            idService.Map.Encode(entity.MapId),
            entity.IsFallbackAllowed,
            entity.Preconfigurations.Select(Map).ToArray(),
            entity
                .Registrations.Select(x => idService.User.Encode(x.PlayerId))
                .Distinct()
                .ToArray(),
            entity.Instances.Select(Map).ToArray()
        );
    }

    public EventInstancePreconfiguration Map(EventInstancePreconfigurationEntity entity)
    {
        return new EventInstancePreconfiguration(
            idService.Preconfiguration.Encode(entity.Id),
            entity.Players.Select(x => idService.User.Encode(x.Id)).ToArray()
        );
    }

    public EventInstance Map(EventInstanceEntity entity)
    {
        return new EventInstance(
            idService.EventInstance.Encode(entity.Id),
            entity.GroupCode,
            entity.Players.Select(x => idService.User.Encode(x.Id)).ToArray()
        );
    }

    public IQueryable<EventEntity> AddIncludes(IQueryable<EventEntity> events)
    {
        return events
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Preconfigurations)
            .ThenInclude(x => x.Players)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Instances)
            .ThenInclude(x => x.Players);
    }
}
