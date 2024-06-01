using System.Net;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Models;
using MinigolfFriday.Utilities;

namespace MinigolfFriday.Services;

[GenerateAutoInterface]
public class EventInstanceService(DatabaseContext databaseContext, IIdService idService)
    : IEventInstanceService
{
    const int MAX_GROUP_SIZE = 5;

    public async Task<
        Result<(EventEntity Event, EventTimeslotInstances[] Instances)>
    > BuildEventInstancesAsync(long eventId, CancellationToken cancellation)
    {
        var @event = await databaseContext
            .Events.Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .ThenInclude(x => x.Player.Avoid)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .ThenInclude(x => x.Player.Prefer)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Preconfigurations)
            .ThenInclude(x => x.Players)
            .FirstOrDefaultAsync(x => x.Id == eventId, cancellation);
        if (@event is null)
            return Result.Fail(
                new Error("Event not found.").WithStatusCode(HttpStatusCode.NotFound)
            );

        var allPlayers = GetAllEventPlayersAsync(@event);
        var timeslotPlayers = @event.Timeslots.ToDictionary(
            x => x,
            x => x.Registrations.Select(x => x.Player.Id).ToList()
        );

        foreach (
            var timeslot in @event.Timeslots.Where(x =>
                x.IsFallbackAllowed && x.Registrations.Count < MAX_GROUP_SIZE
            )
        )
        {
            timeslotPlayers[timeslot].Clear();
            foreach (var registration in timeslot.Registrations)
            {
                if (registration.FallbackEventTimeslot is not null)
                    timeslotPlayers[registration.FallbackEventTimeslot].Add(registration.Player.Id);
            }
        }

        return (
            @event,
            timeslotPlayers
                .Select(x => new EventTimeslotInstances(
                    idService.EventTimeslot.Encode(x.Key.Id),
                    GenerateEventInstances(x.Value, x.Key.Preconfigurations, allPlayers).ToArray()
                ))
                .ToArray()
        );
    }

    public async Task PersistEventInstancesAsync(
        EventTimeslotInstances[] eventInstances,
        CancellationToken cancellation
    )
    {
        var instanceEntities = new Queue<EventInstanceEntity>();
        foreach (var (strTimeslotId, instances) in eventInstances)
        {
            var timeslotId = idService.EventTimeslot.DecodeSingle(strTimeslotId);
            databaseContext.EventInstances.RemoveRange(
                databaseContext.EventInstances.Where(x => x.EventTimeslot.Id == timeslotId)
            );
            foreach (var instance in instances)
            {
                var entity = new EventInstanceEntity
                {
                    GroupCode = instance.GroupCode,
                    EventTimeslot = databaseContext.EventTimeslotById(timeslotId),
                };
                instanceEntities.Enqueue(entity);
                databaseContext.EventInstances.Add(entity);
                foreach (
                    var playerId in instance.PlayerIds.Select(x => idService.User.DecodeSingle(x))
                )
                {
                    var user =
                        databaseContext
                            .ChangeTracker.Entries<UserEntity>()
                            .FirstOrDefault(x => x.Entity.Id == playerId)
                            ?.Entity ?? databaseContext.UserById(playerId);
                    entity.Players.Add(user);
                }
            }
        }

        await databaseContext.SaveChangesAsync(cancellation);
        foreach (var instance in eventInstances.SelectMany(x => x.Instances))
        {
            instance.Id = idService.EventInstance.Encode(instanceEntities.Dequeue().Id);
        }
    }

    private static Dictionary<long, Player> GetAllEventPlayersAsync(EventEntity @event)
    {
        var players = new Dictionary<long, Player>();
        var allPlayers = @event.Timeslots.SelectMany(x => x.Registrations).Select(x => x.Player);
        foreach (var player in allPlayers)
        {
            if (!players.ContainsKey(player.Id))
            {
                players[player.Id] = new Player(player.Id);
            }
        }
        foreach (var player in allPlayers)
        {
            var p = players[player.Id];
            foreach (var avoid in player.Avoid)
            {
                if (players.TryGetValue(avoid.Id, out var ap))
                    p.Avoid.Add(ap);
            }
            foreach (var prefer in player.Prefer)
            {
                if (players.TryGetValue(prefer.Id, out var pp))
                    p.Prefer.Add(pp);
            }
        }

        return players;
    }

    private List<EventInstance> GenerateEventInstances(
        IEnumerable<long> playerIds,
        IEnumerable<EventInstancePreconfigurationEntity> preconfigs,
        Dictionary<long, Player> allPlayers
    )
    {
        var players = playerIds.Select(x => allPlayers[x]).ToList();
        var preconfig = preconfigs
            .Select(x => x.Players.Select(y => allPlayers[y.Id]).ToArray())
            .ToArray();

        // Calculate number of groups. In case a preconfigured group exceeds the MAX_GROUP_SIZE substract
        // the number of overflowing players from these preconfigured groups
        var overflowingPlayers = preconfig.Sum(x => Math.Max(0, x.Length - MAX_GROUP_SIZE));
        var groupCount = (int)
            Math.Ceiling((players.Count - overflowingPlayers) / (double)MAX_GROUP_SIZE);

        // Create groups with preinitialized Lists
        var groups = new List<Player>[groupCount];
        for (int i = 0; i < groups.Length; i++)
            groups[i] = [];

        // Apply preconfigured group combinations
        var remainingPc = new Queue<Player[]>(preconfig);
        while (remainingPc.Count > 0)
        {
            // Add players to the group with the lowest player count
            groups.MinBy(x => x.Count)!.AddRange(remainingPc.Dequeue());
        }

        // Shuffle players and exclude already added players
        var remaining = new LinkedList<Player>(
            players.Except(preconfig.SelectMany(x => x)).OrderBy(x => Random.Shared.Next())
        );

        // Add all remaining players to the groups
        while (remaining.Count > 0)
        {
            // Get the group with the least amount of players
            var group = groups.MinBy(x => x.Count)!;

            // Calculate scores for all remaining players for the group and take the one with the highest score
            group.Add(
                remaining.PopMaxScore(x =>
                    group.Aggregate(0, (acc, y) => acc + getPlayerScore(x, y))
                )
            );
        }

        return groups
            .Select(x => new EventInstance(
                "",
                GroupCodeGenerator.Generate(),
                x.Select(y => idService.User.Encode(y.Id)).ToArray()
            ))
            .ToList();
    }

    private static int getPlayerScore(Player a, Player b)
    {
        var score = 0;
        if (a.Avoid.Contains(b))
            score -= 2;
        if (b.Avoid.Contains(a))
            score -= 2;
        if (a.Prefer.Contains(b))
            score++;
        if (b.Prefer.Contains(a))
            score++;
        return score;
    }

    private class Player(long id)
    {
        public long Id { get; set; } = id;
        public List<Player> Avoid { get; } = [];
        public List<Player> Prefer { get; } = [];
    }
}
