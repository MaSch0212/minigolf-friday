using System.ComponentModel.DataAnnotations;

namespace MinigolfFriday.Domain.Models;

public record UserSettings(
    [property: Required] bool EnableNotifications,
    [property: Required] bool NotifyOnEventPublish,
    [property: Required] bool NotifyOnEventStart,
    [property: Required] bool NotifyOnEventUpdated,
    [property: Required] bool NotifyOnTimeslotStart,
    [property: Required] int SecondsToNotifyBeforeTimeslotStart
)
{
    public UserSettings()
        : this(true, true, true, true, true, 600) { }
}
