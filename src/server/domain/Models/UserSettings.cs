using System.ComponentModel.DataAnnotations;

namespace MinigolfFriday.Domain.Models;

public record UserSettings(
    // [property: Required] long Id,
    [property: Required]
        bool EnableNotifications,
    [property: Required] bool NotifyOnEventPublish,
    [property: Required] bool NotifyOnEventStart,
    [property: Required] bool NotifyOnTimeslotStart,
    [property: Required] long SecondsToNotifyBeforeTimeslotStart
);
