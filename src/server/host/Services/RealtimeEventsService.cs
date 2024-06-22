using FastEnumUtility;
using Microsoft.AspNetCore.SignalR;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Hubs;

namespace MinigolfFriday.Host.Services;

[GenerateAutoInterface]
public class RealtimeEventsService(IHubContext<RealtimeEventsHub> hubContext)
    : IRealtimeEventsService
{
    public async Task SendEventAsync<T>(T @event, CancellationToken ct = default)
        where T : IRealtimeEvent
    {
        if (@event is IGroupRealtimeEvent groupEvent)
        {
            await hubContext
                .Clients.Group(FastEnum.GetName(groupEvent.Group)!)
                .SendAsync(T.MethodName, groupEvent, ct);
        }
        else if (@event is IUserRealtimeEvent userEvent)
        {
            await hubContext.Clients.User(userEvent.UserId).SendAsync(T.MethodName, userEvent, ct);
        }
        else
        {
            throw new ArgumentException("Event must be either a group or user event.");
        }
    }
}
