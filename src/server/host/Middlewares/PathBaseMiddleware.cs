using Microsoft.Extensions.Primitives;

namespace MinigolfFriday.Middlewares;

public static class PathBaseMiddleware
{
    public static IApplicationBuilder UsePathBaseResolver(this IApplicationBuilder app)
    {
        if (app is WebApplication webApp)
        {
            if (
                (webApp.Configuration["path-base"] ?? webApp.Configuration["PathBase"])
                is string pathBase
            )
            {
                app.UsePathBase(pathBase);
            }
        }

        return app.Use(
            async (context, next) =>
            {
                bool isMatch = false;
                var originalPath = context.Request.Path;
                var originalPathBase = context.Request.PathBase;

                if (
                    context.Request.Headers.TryGetValue("X-Forwarded-Path", out StringValues values)
                    && values.Count > 0
                )
                {
                    foreach (var path in values)
                    {
                        if (
                            context.Request.Path.StartsWithSegments(path, out var remaining)
                            && remaining.HasValue
                        )
                        {
                            context.Request.Path = remaining;
                            context.Request.PathBase = path;
                            isMatch = true;
                            break;
                        }
                    }
                }

                if (!isMatch)
                {
                    context.Request.Path = originalPath;
                    context.Request.PathBase = originalPathBase;
                }

                await next();
            }
        );
    }
}
