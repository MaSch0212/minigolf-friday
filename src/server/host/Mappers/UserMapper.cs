using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Mappers;

[GenerateAutoInterface]
public class UserMapper(IIdService idService) : IUserMapper
{
    public Expression<Func<UserEntity, User>> MapUserExpression { get; } =
        (UserEntity entity) =>
            new User(
                idService.User.Encode(entity.Id),
                entity.Alias ?? "",
                entity.Roles.Select(x => x.Id).ToArray(),
                new(
                    entity.Avoid.Select(x => idService.User.Encode(x.Id)).ToArray(),
                    entity.Prefer.Select(x => idService.User.Encode(x.Id)).ToArray()
                ),
                new(false, false, false, false, 3600) //entity.UserSettings //.Select(x => x.Id)
            );

    public User Map(UserEntity entity) => MapUserExpression.Compile()(entity);

    public IQueryable<UserEntity> AddIncludes(IQueryable<UserEntity> users)
    {
        return users.Include(x => x.Avoid).Include(x => x.Prefer).Include(x => x.Roles);
    }
}
