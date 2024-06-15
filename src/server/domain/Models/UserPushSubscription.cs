using WebPush;

namespace MinigolfFriday.Domain.Models;

public class UserPushSubscription(
    long id,
    long userId,
    string lang,
    string endpoint,
    string? p256dh,
    string? auth
) : PushSubscription(endpoint, p256dh, auth)
{
    public long Id { get; set; } = id;
    public long UserId { get; set; } = userId;
    public string Lang { get; set; } = lang;
}
