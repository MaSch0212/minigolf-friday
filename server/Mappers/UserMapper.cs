namespace MinigolfFriday;

public static class UserMapper
{
    public static User ToModel(this UserEntity entity)
    {
        var result = new User
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            IsAdmin = entity.IsAdmin,
            LoginType = entity.GetLoginType()
        };
        foreach (var avoid in entity.Avoid ?? Enumerable.Empty<UserEntity>())
        {
            result.PlayerPreferences.Avoid.Add(avoid.Id.ToString());
        }
        foreach (var prefer in entity.Prefer ?? Enumerable.Empty<UserEntity>())
        {
            result.PlayerPreferences.Prefer.Add(prefer.Id.ToString());
        }
        return result;
    }

    public static UserLoginType GetLoginType(this UserEntity entity)
    {
        return entity.FacebookId is not null ? UserLoginType.Facebook : UserLoginType.Email;
    }
}
