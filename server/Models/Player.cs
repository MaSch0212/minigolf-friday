namespace MinigolfFriday.Models;

public class Player
{
    public string? Id { get; set; }
    public string? Alias { get; set; }
    public required string Name { get; set; }
    public string? FacebookId { get; set; }
    public string? WhatsAppNumber { get; set; }
    public PlayerPreferences PlayerPreferences { get; } = new();
}

public record PlayerPreferences
{
    public List<string> Avoid { get; } = [];
    public List<string> Prefer { get; } = [];
}
