namespace MinigolfFriday;

public enum UserLoginType
{
    Facebook,
    Email,
    Admin
}

public class User
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsAdmin { get; set; }
    public required UserLoginType LoginType { get; set; }
    public PlayerPreferences PlayerPreferences { get; } = new();
}

public record PlayerPreferences
{
    public List<string> Avoid { get; } = [];
    public List<string> Prefer { get; } = [];
}
