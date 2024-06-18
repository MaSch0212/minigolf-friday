using FastEndpoints;
using MaSch.Core.Extensions;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Dev;

public class PushNotificationToAllUsersEndpoint(
    DatabaseContext databaseContext,
    IUserPushSubscriptionMapper userPushSubscriptionMapper,
    IWebPushService webPushService
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("pushnotificationtoallusers");
        Group<DevGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var pushSubscription = await databaseContext
            .UserPushSubscriptions.Where(x =>
                x.User.Settings == null || x.User.Settings.EnableNotifications
            )
            .Select(userPushSubscriptionMapper.MapUserPushSubscriptionExpression)
            .ToListAsync(ct);
        await webPushService.SendAsync(
            pushSubscription,
            new DebugPushNotification("This is a test message"),
            ct
        );
    }

    private record DebugPushNotification(string Message) : IPushNotificationData
    {
        public string Type => "debug";

        public Dictionary<string, PushNotificationOnActionClick> OnActionClick =>
            new() { { "default", new($"/") } };

        public string GetTitle(string lang) => "Debug Message";

        public string GetBody(string lang) => Message;
    }
}
