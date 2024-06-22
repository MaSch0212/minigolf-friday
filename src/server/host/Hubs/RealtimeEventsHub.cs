using FastEnumUtility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;

namespace MinigolfFriday.Host.Hubs;

[Authorize]
public class RealtimeEventsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole(FastEnum.GetName(Role.Admin)!) == true)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                FastEnum.GetName(RealtimeEventGroup.Admin)!
            );
        }

        if (Context.User?.IsInRole(FastEnum.GetName(Role.Player)!) == true)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                FastEnum.GetName(RealtimeEventGroup.Player)!
            );
        }

        await base.OnConnectedAsync();
    }
}
