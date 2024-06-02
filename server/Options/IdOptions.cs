namespace MinigolfFriday.Options;

public class IdOptions : IOptionsWithSection
{
    public static string SectionPath => "Ids";

    public string? Seed { get; set; }
}
