using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday;
using MinigolfFriday.Data;
using MinigolfFriday.Middlewares;
using MinigolfFriday.Models;
using MinigolfFriday.Services;
using MinigolfFriday.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddDbContext<MinigolfFridayContext>();
builder.Services.AddOptions<FacebookOptions>().BindConfiguration(FacebookOptions.SectionName);
builder
    .Services
    .AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, FacebookSignedRequestAuthHandler>("facebook", _ => { });
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IFacebookAccessTokenProvider, FacebookAccessTokenProvider>();

builder.Services.AddScoped<IValidator<Player>, PlayerValidator>();
builder.Services.AddScoped<IValidator<PlayerPreferences>, PlayerPreferencesValidator>();
builder.Services.AddScoped<IFacebookService, FacebookService>();

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

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
#pragma warning restore ASP0014 // Suggest using top level route registrations

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "../";
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("https://localhost:4200");
    }
});

using (var context = new MinigolfFridayContext(null))
{
    context.Database.Migrate();
}

app.Run();
