using System.Linq.Expressions;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Host.Mappers;

[GenerateAutoInterface]
public class UserSettingsMapper : IUserSettingsMapper
{
    public Expression<Func<UserEntity, UserSettings?>> MapUserToUserSettingsExpression { get; } =
        (UserEntity entity) =>
            entity.Settings == null
                ? null
                : new()
                {
                    EnableNotifications = entity.Settings.EnableNotifications,
                    NotifyOnEventPublish = entity.Settings.NotifyOnEventPublish,
                    NotifyOnEventStart = entity.Settings.NotifyOnEventStart,
                    NotifyOnEventUpdated = entity.Settings.NotifyOnEventUpdated,
                    NotifyOnTimeslotStart = entity.Settings.NotifyOnTimeslotStart,
                    SecondsToNotifyBeforeTimeslotStart = entity
                        .Settings
                        .SecondsToNotifyBeforeTimeslotStart
                };
}
