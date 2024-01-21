using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday;
using MinigolfFriday.Data;
using MinigolfFriday.Middlewares;
using MinigolfFriday.Models;
using MinigolfFriday.Services;
using MinigolfFriday.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<FacebookOptions>().BindConfiguration(FacebookOptions.SectionName);
builder.Services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName);

builder.Services.AddControllers();
builder.Services.AddDbContext<MinigolfFridayContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder
    .Services
    .AddAuthorization(options =>
    {
        options.AddPolicy(Policies.Admin, policy => policy.RequireRole(Roles.Admin));
        options.AddPolicy(Policies.Player, policy => policy.RequireRole(Roles.Player));
    });
builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IFacebookAccessTokenProvider, FacebookAccessTokenProvider>();

builder.Services.AddScoped<IValidator<Player>, PlayerValidator>();
builder.Services.AddScoped<IValidator<PlayerPreferences>, PlayerPreferencesValidator>();
builder.Services.AddScoped<IUSerService, UserService>();
builder.Services.AddScoped<IFacebookService, FacebookService>();
builder.Services.AddScoped<IJwtService, JwtService>();

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
