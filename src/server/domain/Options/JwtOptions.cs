using Microsoft.Extensions.Options;

namespace MinigolfFriday.Domain.Options;

public class JwtOptions : IOptionsWithSection
{
    public static string SectionPath => "Authentication:Jwt";
    public static Type? ValidatorType => typeof(JwtOptionsValidator);

    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(15);
}

public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrEmpty(options.Secret))
            return ValidateOptionsResult.Fail("Authentication:Jwt:Secret must be set");
        if (string.IsNullOrEmpty(options.Issuer))
            return ValidateOptionsResult.Fail("Authentication:Jwt:Issuer must be set");
        if (string.IsNullOrEmpty(options.Audience))
            return ValidateOptionsResult.Fail("Authentication:Jwt:Audience must be set");
        if (options.Expiration <= TimeSpan.FromMinutes(1))
            return ValidateOptionsResult.Fail(
                "Authentication:Jwt:Expiration must be greater than or equal to 1 minute"
            );
        return ValidateOptionsResult.Success;
    }
}
