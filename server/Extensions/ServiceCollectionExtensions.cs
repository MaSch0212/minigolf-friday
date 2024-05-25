using Microsoft.Extensions.Options;
using MinigolfFriday.Options;

namespace MinigolfFriday.Extensions;

public static class ServiceCollectionExtensions
{
    public static OptionsBuilder<TOptions> AddAndBindOptions<TOptions>(
        this IServiceCollection services
    )
        where TOptions : class, IOptionsWithSection
    {
        return services.AddOptions<TOptions>().BindConfiguration(TOptions.SectionPath);
    }
}
