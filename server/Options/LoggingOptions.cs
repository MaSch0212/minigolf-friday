namespace MinigolfFriday.Options;

public class LoggingOptions : IOptionsWithSection
{
    public static string SectionPath => "Logging";

    public bool EnableDbLogging { get; set; } = false;
}
