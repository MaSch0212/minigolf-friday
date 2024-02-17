using System.Net;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Models;

namespace MinigolfFriday.Services;

public class EVentInstanceService(MinigolfFridayContext dbContext) : IEventInstanceService
{
    const int MAX_GROUP_SIZE = 5;

    private readonly MinigolfFridayContext _dbContext = dbContext;

    public async Task<Result<Dictionary<string, EventInstance[]>>> BuildEventInstancesAsync(
        Guid eventId
    )
    {
        var @event = await _dbContext
            .Events
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .ThenInclude(x => x.Player.Avoid)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Registrations)
            .ThenInclude(x => x.Player.Prefer)
            .Include(x => x.Timeslots)
            .ThenInclude(x => x.Preconfigurations)
            .ThenInclude(x => x.Players)
            .FirstOrDefaultAsync(x => x.Id == eventId);
        if (@event is null)
            return Result.Fail(
                new Error("Event not found.").WithStatusCode(HttpStatusCode.NotFound)
            );

        var allPlayers = GetAllEventPlayersAsync(@event);
        var timeslotPlayers = @event
            .Timeslots
            .ToDictionary(x => x, x => x.Registrations.Select(x => x.Player.Id).ToList());

        foreach (
            var timeslot in @event
                .Timeslots
                .Where(x => x.IsFallbackAllowed && x.Registrations.Count < MAX_GROUP_SIZE)
        )
        {
            timeslotPlayers[timeslot].Clear();
            foreach (var registration in timeslot.Registrations)
            {
                if (registration.FallbackEventTimeslot is not null)
                    timeslotPlayers[registration.FallbackEventTimeslot].Add(registration.Player.Id);
            }
        }

        return timeslotPlayers.ToDictionary(
            x => x.Key.Id.ToString(),
            x => GenerateEventInstances(x.Value, x.Key.Preconfigurations, allPlayers).ToArray()
        );
    }

    public async Task<Result> PersistEventInstancesAsync(
        Dictionary<string, EventInstance[]> eventInstances
    )
    {
        foreach (var (strTimeslotId, instances) in eventInstances)
        {
            var timeslotId = Guid.Parse(strTimeslotId);
            _dbContext
                .EventInstances
                .RemoveRange(_dbContext.EventInstances.Where(x => x.EventTimeslotId == timeslotId));
            foreach (var instance in instances)
            {
                var entity = new EventInstanceEntity
                {
                    Id = Guid.NewGuid(),
                    GroupCode = instance.GroupCode,
                    EventTimeslotId = timeslotId
                };
                instance.Id = entity.Id.ToString();
                _dbContext.EventInstances.Add(entity);
                foreach (var playerId in instance.PlayerIds)
                {
                    var user =
                        _dbContext
                            .ChangeTracker
                            .Entries<UserEntity>()
                            .FirstOrDefault(x => x.Entity.Id == Guid.Parse(playerId))
                            ?.Entity ?? UserEntity.ById(Guid.Parse(playerId));
                    entity.Players.Add(user);
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }

    private static Dictionary<Guid, Player> GetAllEventPlayersAsync(EventEntity @event)
    {
        var players = new Dictionary<Guid, Player>();
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

    private static List<EventInstance> GenerateEventInstances(
        IEnumerable<Guid> playerIds,
        IEnumerable<EventInstancePreconfigurationEntity> preconfigs,
        Dictionary<Guid, Player> allPlayers
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
            groups[i] =  [];

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
                remaining.PopMaxScore(
                    x => group.Aggregate(0, (acc, y) => acc + getPlayerScore(x, y))
                )
            );
        }

        return groups
            .Select(
                x =>
                    new EventInstance(
                        null,
                        GroupCodeGenerator.Generate(),
                        x.Select(y => y.Id.ToString())
                    )
            )
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

    private class Player
    {
        public Guid Id { get; set; }
        public List<Player> Avoid { get; } = [];
        public List<Player> Prefer { get; } = [];

        public Player(Guid id)
        {
            Id = id;
        }
    }
}
