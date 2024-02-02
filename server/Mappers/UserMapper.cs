namespace MinigolfFriday;

public static class UserMapper
{
    public static User ToModel(this UserEntity entity)
    {
        return new User
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            IsAdmin = entity.IsAdmin,
            LoginType = entity.GetLoginType()
        };
    }

    public static UserLoginType GetLoginType(this UserEntity entity)
    {
        return entity.FacebookId is not null ? UserLoginType.Facebook : UserLoginType.Email;
    }
}
