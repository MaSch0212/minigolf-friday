using FastEndpoints;

namespace MinigolfFriday.Host.Endpoints.Notifications;

public class NotificationsGroup : Group
{
    public NotificationsGroup()
    {
        Configure(
            "notifications",
            x =>
            {
                x.Description(x => x.WithTags("Notifications"));
            }
        );
    }
}
