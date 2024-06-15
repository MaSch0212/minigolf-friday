using System.Text.Json.Serialization;
using MinigolfFriday.Domain.Serialization;

namespace MinigolfFriday.Domain.Models.Push;

public record PushNotificationOnActionClick(
    string Url,
    [property: JsonConverter(typeof(CamelCaseStringEnumConverter))]
        PushNotificationOnActionClickOperation Operation =
        PushNotificationOnActionClickOperation.NavigateLastFocusedOrOpen
);
