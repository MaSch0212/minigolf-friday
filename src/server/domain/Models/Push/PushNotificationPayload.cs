namespace MinigolfFriday.Domain.Models.Push;

public class PushNotificationPayload
{
    public required string Title { get; set; }
    public required string Body { get; set; }
    public required string Lang { get; set; }
    public required IPushNotificationData Data { get; set; }
    public string Icon { get; } = "icons/icon-512x512.png";
    public int[] Vibrate { get; } = [100, 50, 100];

    private PushNotificationPayload() { }

    public static PushNotificationPayload Create<TData>(string lang, TData data)
        where TData : IPushNotificationData
    {
        return new PushNotificationPayload
        {
            Title = data.GetTitle(lang),
            Body = data.GetBody(lang),
            Lang = lang,
            Data = data,
        };
    }
}
