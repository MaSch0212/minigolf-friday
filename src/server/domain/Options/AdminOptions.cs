namespace MinigolfFriday.Domain.Options;

public class AdminOptions : IOptionsWithSection
{
    public static string SectionPath => "Admin";
    public static Type? ValidatorType => null;

    public string? LoginToken { get; set; }
}
