namespace MinigolfFriday;

public enum UserLoginType
{
    Facebook,
    Email
}

public class User
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsAdmin { get; set; }
    public required UserLoginType LoginType { get; set; }
}
