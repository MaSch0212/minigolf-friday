using System.Linq.Expressions;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Host.Mappers;

[GenerateAutoInterface]
public class UserPushSubscriptionMapper() : IUserPushSubscriptionMapper
{
    public Expression<
        Func<UserPushSubscriptionEntity, UserPushSubscription>
    > MapUserPushSubscriptionExpression { get; } =
        (UserPushSubscriptionEntity entity) =>
            new UserPushSubscription(
                entity.Id,
                entity.UserId,
                entity.Lang,
                entity.Endpoint,
                entity.P256DH,
                entity.Auth
            );

    public UserPushSubscription Map(UserPushSubscriptionEntity entity) =>
        MapUserPushSubscriptionExpression.Compile()(entity);
}
