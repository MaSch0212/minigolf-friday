using System.Net;
using FluentResults;
using MaSch.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Utilities;

namespace MinigolfFriday.Host.Services;

[GenerateAutoInterface]
public class EventInstanceService(DatabaseContext databaseContext, IIdService idService)
    : IEventInstanceService
{
    const int MAX_GROUP_SIZE = 5;
    const int MIN_TIMESLOT_REGISTRATIONS = 3;

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
                x.IsFallbackAllowed && x.Registrations.Count < MIN_TIMESLOT_REGISTRATIONS
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

        var playerPairScores = new Dictionary<PlayerPair, int>();
        var instances = new List<EventTimeslotInstances>();
        foreach (var kv in timeslotPlayers)
        {
            var groupCodeGenerator = new GroupCodeGenerator();
            var timeslot = kv.Key;
            var players = kv.Value;
            var groups = GeneratePlayerGroups(
                playerPairScores,
                players,
                timeslot.Preconfigurations,
                allPlayers
            );

            instances.Add(
                new EventTimeslotInstances(
                    idService.EventTimeslot.Encode(timeslot.Id),
                    groups
                        .Select(group => new EventInstance(
                            "",
                            groupCodeGenerator.Generate(),
                            group.Select(playerId => idService.User.Encode(playerId)).ToArray()
                        ))
                        .ToArray()
                )
            );
        }

        return (@event, instances.ToArray());
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

    private long[][] GeneratePlayerGroups(
        Dictionary<PlayerPair, int> playerPairScores,
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
            var playerToAdd = remaining.PopMaxScore(x =>
                group.Aggregate(0, (acc, y) => acc + GetPlayerScore(playerPairScores, x, y))
            );

            foreach (var other in group)
                playerPairScores[PlayerPair.Create(playerToAdd.Id, other.Id)] -= 100;

            group.Add(playerToAdd);
        }

        return groups.Select(x => x.Select(y => y.Id).ToArray()).ToArray();
    }

    private static int GetPlayerScore(
        Dictionary<PlayerPair, int> playerPairScores,
        Player a,
        Player b
    )
    {
        var playerPair = PlayerPair.Create(a.Id, b.Id);
        if (playerPairScores.TryGetValue(new(a.Id, b.Id), out var score))
            return score;

        score = 0;
        if (a.Avoid.Contains(b))
            score -= 2;
        if (b.Avoid.Contains(a))
            score -= 2;
        if (a.Prefer.Contains(b))
            score++;
        if (b.Prefer.Contains(a))
            score++;
        playerPairScores[playerPair] = score;
        return score;
    }

    private class Player(long id)
    {
        public long Id { get; set; } = id;
        public List<Player> Avoid { get; } = [];
        public List<Player> Prefer { get; } = [];
    }

    private readonly record struct PlayerPair(long PlayerId1, long PlayerId2)
    {
        public static PlayerPair Create(long PlayerId1, long PlayerId2)
        {
            if (PlayerId1 < PlayerId2)
                return new(PlayerId1, PlayerId2);
            return new(PlayerId2, PlayerId1);
        }
    }
}
