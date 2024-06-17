using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Mappers;

[GenerateAutoInterface]
public class UserSettingsMapper(IIdService idService) : IUserSettingsMapper
{
    public Expression<Func<UserSettingsEntity, UserSettings>> MapUserSettingsExpression { get; } =
        (UserSettingsEntity entity) =>
            new UserSettings(
                // idService.User.Encode(entity.Id),
                false,
                false,
                false,
                false,
                3600
            // entity.Alias ?? "",
            // entity.Roles.Select(x => x.Id).ToArray(),
            // new(
            //     entity.Avoid.Select(x => idService.User.Encode(x.Id)).ToArray(),
            //     entity.Prefer.Select(x => idService.User.Encode(x.Id)).ToArray()
            // ),
            // entity.UserSettings.Select(x => x.Id)
            );

    public UserSettings Map(UserSettingsEntity entity) =>
        MapUserSettingsExpression.Compile()(entity);

    // public UserSettings Map(UserSettingsEntity entity)
    // {
    //     return new UserSettings(false, false, false, false, 3600);
    // }

    public IQueryable<UserSettingsEntity> AddIncludes(IQueryable<UserSettingsEntity> users)
    {
        return users.Include(x => x.EnableNotifications);
    }
}
