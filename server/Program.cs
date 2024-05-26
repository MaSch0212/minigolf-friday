using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Middlewares;
using MinigolfFriday.Options;
using MinigolfFriday.Services;
using NSwag;
using NSwag.Generation.AspNetCore;

ValidatorOptions.Global.LanguageManager = new ValidationLanguageManager();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAndBindOptions<JwtOptions>();
builder.Services.AddAndBindOptions<AdminOptions>();
builder.Services.AddAndBindOptions<IdOptions>();
builder.Services.AddAndBindOptions<LoggingOptions>();

var configureJsonSerializerOptions = new ConfigureJsonSerializerOptions();
builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
builder.Services.ConfigureOptions(configureJsonSerializerOptions);

builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddScoped<IEventMapper, EventMapper>();
builder.Services.AddScoped<IMinigolfMapMapper, MinigolfMapMapper>();
builder.Services.AddScoped<IPlayerEventMapper, PlayerEventMapper>();
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.Services.AddSingleton<IIdService, IdService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEventInstanceService, EventInstanceService>();

builder.Services.AddHealthChecks().AddDbContextCheck<DatabaseContext>();
builder
    .Services
    .AddFastEndpoints(o =>
    {
        IEnumerable<Type> endpointTypes = MinigolfFriday.DiscoveredTypes.All;
        if (Environment.GetEnvironmentVariable("ENABLE_DEV_ENDPOINTS") != "true")
            endpointTypes = endpointTypes.Where(x => !x.FullName!.Contains(".Dev."));
        o.SourceGeneratorDiscoveredTypes.AddRange(endpointTypes);
    });
builder.Services.AddEndpointsApiExplorer();
builder
    .Services
    .SwaggerDocument(c =>
    {
        c.DocumentSettings = d =>
        {
            d.OperationProcessors.Add(new OperationIdProcessor());
        };
        c.ShortSchemaNames = true;
        c.AutoTagPathSegmentIndex = 0;
        c.SerializerSettings = configureJsonSerializerOptions.Configure;
        c.RemoveEmptyRequestSchema = true;
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder
    .Services
    .AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });
builder
    .Services
    .AddSpaStaticFiles(options =>
    {
        options.RootPath = "wwwroot/browser";
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UsePathBaseResolver();
app.UseForwardedHeaders();

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHtmlBaseTagInjector();
    app.UseSpaStaticFiles();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.UseFastEndpoints(new ConfigureFastEndpointsConfig(configureJsonSerializerOptions).Configure)
    .UseSwaggerGen();

app.UseEndpoints(x => { });
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../";
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("https://localhost:4200");
    }
});

var openApiOutput = app.Configuration.GetValue<string>("OpenApiOutput");
if (openApiOutput != null)
{
    using var scope = app.Services.CreateScope();
    Console.WriteLine("Generating OpenAPI...");
    var options = scope
        .ServiceProvider
        .GetRequiredService<IOptions<AspNetCoreOpenApiDocumentGeneratorSettings>>();
    var documentProvider = scope
        .ServiceProvider
        .GetRequiredService<NSwag.Generation.IOpenApiDocumentGenerator>();
    var apidoc = await documentProvider.GenerateAsync(options.Value.DocumentName);
    var targetPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), openApiOutput));
    Console.WriteLine($"Writing OpenAPI to \"{targetPath}\"...");
    File.WriteAllText(openApiOutput, apidoc.ToYaml());
    Console.WriteLine("Done");
    return;
}

using (var context = new DatabaseContext(null))
{
    context.Database.Migrate();
}

app.Run();
