namespace MinigolfFriday.Options;

public class AdminOptions : IOptionsWithSection
{
    public static string SectionPath => "Admin";

    public string? LoginToken { get; set; }
}
