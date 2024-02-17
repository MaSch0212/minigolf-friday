using FluentResults;
using MinigolfFriday.Models;

namespace MinigolfFriday.Services;

public interface IEventInstanceService
{
    Task<Result<Dictionary<string, EventInstance[]>>> BuildEventInstancesAsync(Guid eventId);
    Task<Result> PersistEventInstancesAsync(Dictionary<string, EventInstance[]> eventInstances);
}
