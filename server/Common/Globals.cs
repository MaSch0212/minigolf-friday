namespace MinigolfFriday;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Player = "Player";
}

public static class CustomClaimNames
{
    public const string FacebookId = "FacebookId";
    public const string LoginType = "LoginType";
}

public static class Globals
{
    public static readonly User AdminUser =
        new()
        {
            Id = "admin",
            Name = "Admin",
            IsAdmin = true,
            LoginType = UserLoginType.Admin
        };
}
