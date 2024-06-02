namespace MinigolfFriday.Options;

public class JwtOptions : IOptionsWithSection
{
    public static string SectionPath => "Authentication:Jwt";

    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(15);
}
