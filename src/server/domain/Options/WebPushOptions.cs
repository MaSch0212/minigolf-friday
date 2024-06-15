using Microsoft.Extensions.Options;
using WebPush;

namespace MinigolfFriday.Domain.Options;

public class WebPushOptions : VapidDetails, IOptionsWithSection
{
    public static string SectionPath => "WebPush";
    public static Type? ValidatorType => typeof(WebPushOptionsValidator);
}

public class WebPushOptionsValidator : IValidateOptions<WebPushOptions>
{
    public ValidateOptionsResult Validate(string? name, WebPushOptions options)
    {
        if (string.IsNullOrEmpty(options.Subject))
            return ValidateOptionsResult.Fail("WebPush:Subject must be set");
        if (string.IsNullOrEmpty(options.PublicKey))
            return ValidateOptionsResult.Fail("WebPush:PublicKey must be set");
        if (string.IsNullOrEmpty(options.PrivateKey))
            return ValidateOptionsResult.Fail("WebPush:PrivateKey must be set");
        return ValidateOptionsResult.Success;
    }
}
