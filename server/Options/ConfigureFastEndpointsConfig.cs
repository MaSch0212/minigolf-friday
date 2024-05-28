using System.Reflection;
using System.Text.Json;
using FastEndpoints;
using Microsoft.Extensions.Options;
using MinigolfFriday.Common;

namespace MinigolfFriday.Options;

public class ConfigureFastEndpointsConfig(
    IConfigureOptions<JsonSerializerOptions> configureJsonSerializerOptions
) : IConfigureOptions<Config>
{
    public void Configure(Config c)
    {
        configureJsonSerializerOptions.Configure(c.Serializer.Options);

        c.Errors.UseProblemDetails();
        c.Endpoints.ShortNames = true;
        c.Endpoints.RoutePrefix = "api";
        c.Endpoints.Configurator = c =>
        {
            if (c.Routes != null)
            {
                for (int i = 0; i < c.Routes.Length; i++)
                {
                    c.Routes[i] = c.Routes[i].Replace("/:", ":").TrimEnd('/');
                }
            }
            c.Description(x =>
            {
                if (c.Routes?.Length == 1)
                {
                    var routeParams = GetRouteParams(c.Routes[0]);
                    var props = c.ReqDtoType.GetProperties(
                        BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance
                    );
                    if (
                        !props
                            .Select(x => x.Name)
                            .Except(routeParams, StringComparer.OrdinalIgnoreCase)
                            .Any()
                    )
                    {
                        x.Accepts<EmptyRequest>();
                    }
                }

                x.ProducesProblemDetails()
                    .Produces(200, c.ResDtoType == typeof(object) ? null : c.ResDtoType);
            });
            c.Summary(x =>
            {
                x.AppendResponseDescription(401, "Unauthorized", insertBeforeExisting: true);
                x.AppendResponseDescription(403, "Forbidden", insertBeforeExisting: true);
            });
        };
    }

    private static string[] GetRouteParams(string route)
    {
        return RegularExpressions.RouteParams().Matches(route).Select(x => x.Value).ToArray();
    }
}
