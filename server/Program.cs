using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder
    .Services
    .AddSpaStaticFiles(options =>
    {
        options.RootPath = "wwwroot";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

if ((app.Configuration["path-base"] ?? app.Configuration["PathBase"]) is string pathBase)
    app.UsePathBase(pathBase);

app.Use(
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
                    context
                        .Request
                        .Path
                        .StartsWithSegments(
                            "/" + path?.Trim('/'),
                            out var matched,
                            out var remaining
                        )
                )
                {
                    isMatch = true;
                    context.Request.Path = remaining;
                    context.Request.PathBase = context.Request.PathBase.Add(matched);
                    break;
                }
            }
        }

        try
        {
            await next();
        }
        finally
        {
            if (isMatch)
            {
                context.Request.Path = originalPath;
                context.Request.PathBase = originalPathBase;
            }
        }
    }
);
app.UseForwardedHeaders();

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.Use(
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
                var baseTagMatch = Regex.Match(html, @"<base href=""(?<PathBase>[^""]+)""\s*\/?>");
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
                await using (var sw = new StreamWriter(context.Response.Body))
                {
                    await sw.WriteAsync(html);
                }
            }
            else
            {
                await newBody.CopyToAsync(context.Response.Body);
            }
        }
    );
    app.UseSpaStaticFiles();
}

app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../";
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
    }
});

app.Run();
