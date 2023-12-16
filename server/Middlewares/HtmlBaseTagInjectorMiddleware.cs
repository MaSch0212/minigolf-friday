using System.Text.RegularExpressions;

namespace MinigolfFriday.Middlewares;

public static partial class HtmlBaseTagInjectorMiddleware
{
    [GeneratedRegex(@"<base href=""(?<PathBase>[^""]+)""\s*\/?>")]
    public static partial Regex BaseTagRegex();

    public static IApplicationBuilder UseHtmlBaseTagInjector(this IApplicationBuilder app)
    {
        return app.Use(
            async (context, next) =>
            {
                var body = context.Response.Body;
                using var newBody = new MemoryStream();
                context.Response.Body = newBody;

                await next();

                context.Response.Body = body;
                newBody.Seek(0, SeekOrigin.Begin);
                if (context.Response.ContentType == "text/html")
                {
                    using var streamReader = new StreamReader(newBody);
                    var html = await streamReader.ReadToEndAsync();
                    var baseTagMatch = BaseTagRegex().Match(html);
                    if (baseTagMatch.Success)
                    {
                        var pathBaseGroup = baseTagMatch.Groups["PathBase"];
                        html = string.Concat(
                            html[..pathBaseGroup.Index],
                            context.Request.PathBase.Value?.TrimEnd('/') + "/",
                            html[(pathBaseGroup.Index + pathBaseGroup.Value.Length)..]
                        );
                    }

                    context.Response.ContentLength = null;
                    await using var sw = new StreamWriter(context.Response.Body);
                    await sw.WriteAsync(html);
                }
                else
                {
                    await newBody.CopyToAsync(context.Response.Body);
                }
            }
        );
    }
}
