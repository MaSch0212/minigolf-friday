namespace MinigolfFriday;

public class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public const string FacebookIdClaim = "FacebookId";

    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(15);
}
