namespace MinigolfFriday;

public class FacebookOptions
{
    public const string SectionName = "Authentication:Facebook";

    public required string AppId { get; set; }
    public required string AppSecret { get; set; }
}
