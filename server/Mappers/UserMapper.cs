namespace MinigolfFriday;

public static class UserMapper
{
    public static User ToModel(this UserEntity entity)
    {
        return new User
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            IsAdmin = entity.IsAdmin
        };
    }
}
