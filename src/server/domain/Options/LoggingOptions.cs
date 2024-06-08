namespace MinigolfFriday.Domain.Options;

public class LoggingOptions : IOptionsWithSection
{
    public static string SectionPath => "Logging";
    public static Type? ValidatorType => null;

    public bool EnableDbLogging { get; set; } = false;
}
