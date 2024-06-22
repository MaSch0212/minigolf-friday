using FastEndpoints;
using FastEndpoints.Swagger;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Extensions;
using MinigolfFriday.Domain.Options;
using MinigolfFriday.Host.Hubs;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Middlewares;
using MinigolfFriday.Host.Options;
using MinigolfFriday.Host.Services;
using NSwag;
using NSwag.Generation.AspNetCore;

ValidatorOptions.Global.LanguageManager = new ValidationLanguageManager();

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddAndBindOptions<JwtOptions>();
builder.Services.AddAndBindOptions<AdminOptions>();
builder.Services.AddAndBindOptions<IdOptions>();
builder.Services.AddAndBindOptions<LoggingOptions>();
builder.Services.AddAndBindOptions<DatabaseOptions>();
builder.Services.AddAndBindOptions<WebPushOptions>();

var configureJsonSerializerOptions = new ConfigureJsonSerializerOptions();
builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
builder.Services.ConfigureOptions(configureJsonSerializerOptions);

builder.Services.AddSingleton<IIdService, IdService>();

builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IEventInstanceService, EventInstanceService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRealtimeEventsService, RealtimeEventsService>();
builder.Services.AddScoped<IWebPushService, WebPushService>();

builder.Services.AddScoped<IEventMapper, EventMapper>();
builder.Services.AddScoped<IMinigolfMapMapper, MinigolfMapMapper>();
builder.Services.AddScoped<IPlayerEventMapper, PlayerEventMapper>();
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.Services.AddScoped<IUserPushSubscriptionMapper, UserPushSubscriptionMapper>();
builder.Services.AddScoped<IUserSettingsMapper, UserSettingsMapper>();

builder.Services.AddHealthChecks().AddDbContextCheck<DatabaseContext>();
builder.Services.AddFastEndpoints(o =>
{
    IEnumerable<Type> endpointTypes = MinigolfFriday.Host.DiscoveredTypes.All;
    if (Environment.GetEnvironmentVariable("ENABLE_DEV_ENDPOINTS") != "true")
        endpointTypes = endpointTypes.Where(x => !x.FullName!.Contains(".Dev."));
    o.SourceGeneratorDiscoveredTypes.AddRange(endpointTypes);
});
builder
    .Services.AddSignalR()
    .AddJsonProtocol(options =>
        configureJsonSerializerOptions.Configure(options.PayloadSerializerOptions)
    );
builder.Services.AddEndpointsApiExplorer();
builder.Services.SwaggerDocument(c =>
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
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});
builder.Services.AddSpaStaticFiles(options =>
{
    options.RootPath = "wwwroot";
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
app.MapHub<RealtimeEventsHub>("/hubs/realtime-events");
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
    var options = scope.ServiceProvider.GetRequiredService<
        IOptions<AspNetCoreOpenApiDocumentGeneratorSettings>
    >();
    var documentProvider =
        scope.ServiceProvider.GetRequiredService<NSwag.Generation.IOpenApiDocumentGenerator>();
    var apidoc = await documentProvider.GenerateAsync(options.Value.DocumentName);
    var targetPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), openApiOutput));
    Console.WriteLine($"Writing OpenAPI to \"{targetPath}\"...");
    File.WriteAllText(openApiOutput, apidoc.ToYaml());
    Console.WriteLine("Done");
    return;
}

using (var scope = app.Services.CreateScope())
{
    var databaseOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
    if (!databaseOptions.Value.SkipMigration)
    {
        using var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        dbContext.Database.Migrate();
    }
}

app.Run();
