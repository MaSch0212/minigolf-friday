using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MinigolfFriday.IntegrationTests.Builders;

namespace MinigolfFriday.IntegrationTests;

internal interface IHttpClientAccessor
{
    HttpClient HttpClient { get; }
}

[TestClass]
internal sealed class Sut : IAsyncDisposable, IHttpClientAccessor
{
    private static readonly ConcurrentQueue<AppContainer> _freeAppContainers = new();

    private readonly AppContainer _app;
    private readonly HttpClient _httpClient;
    private readonly DateTime _start;

    public IContainer App => _app.Container;
    public MinigolfFridayClient AppClient { get; }
    public TimeSpan TokenExpiration => _app.TokenExpiration;
    public string AdminToken { get; private set; } = null!;
    public string AppBaseUrl => $"http://{App.Hostname}:{App.GetMappedPublicPort(80)}";

    HttpClient IHttpClientAccessor.HttpClient => _httpClient;

    private Sut(DateTime start, AppContainer app)
    {
        _start = start;

        _httpClient = new HttpClient();
        _app = app;
        AppClient = new MinigolfFridayClient(AppBaseUrl, _httpClient);
    }

    public async Task<string> Token(string loginToken) =>
        (await AppClient.GetTokenAsync(new() { LoginToken = loginToken })).Token;

    public UserBuilder User(string? alias = null) => new(this, alias);

    public MinigolfMapBuilder MinigolfMap(string? name = null) => new(this, name);

    public EventBuilder Event() => new(this);

    public EventTimeslotBuilder EventTimeslot(TimeSpan time, string mapId) =>
        new(this, time, mapId);

    public EventInstancePreconfigurationBuilder EventInstancePreconfiguration() => new(this);

    public async ValueTask DisposeAsync()
    {
        try
        {
            await AppClient.ResetDatabase();
            await TraceContainerLog(App);
            _freeAppContainers.Enqueue(_app);
        }
        catch
        {
            await TraceContainerLog(App);
            await App.DisposeAsync();
            throw;
        }
        finally
        {
            _httpClient.Dispose();
        }
    }

    private async Task TraceContainerLog(
        IContainer container,
        [CallerArgumentExpression(nameof(container))] string containerName = ""
    )
    {
        var (stdout, stderr) = await container.GetLogsAsync(_start);
        Trace.WriteLine($"{containerName} (stdout):\n{stdout}");
        Trace.WriteLine($"{containerName} (stderr):\n{stderr}");
    }

    public static async Task<Sut> CreateAsync(bool getAdminToken = true)
    {
        var start = DateTime.Now;
        if (!_freeAppContainers.TryDequeue(out var container))
            container = await CreateContainerAsync();
        try
        {
            var scope = new Sut(start, container);
            if (getAdminToken)
            {
                scope.AdminToken = scope.AppClient.Token = (
                    await scope.AppClient.GetTokenAsync(new() { LoginToken = "admin" })
                ).Token;
            }
            return scope;
        }
        catch
        {
            _freeAppContainers.Enqueue(container);
            throw;
        }
    }

    [AssemblyCleanup]
    public static async Task CleanupAssembly()
    {
        await Task.WhenAll(
            _freeAppContainers.Select(x => x.Container.DisposeAsync()).Select(x => x.AsTask())
        );
    }

    private static async Task<AppContainer> CreateContainerAsync()
    {
        var tokenExpiration = TimeSpan.FromSeconds(Random.Shared.Next(300, 180000));
        var container = new ContainerBuilder()
            .WithImage("masch0212/minigolf-friday:intttest")
            .WithPortBinding(80)
            .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT.ASPNETCORE", "Information")
            .WithEnvironment("LOGGING__ENABLEDBLOGGING", "true")
            .WithEnvironment("AUTHENTICATION__JWT__EXPIRATION", tokenExpiration.ToString())
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPath("healthz"))
            )
            .Build();

        await container.StartAsync();

        return new(container, tokenExpiration);
    }

    private record AppContainer(IContainer Container, TimeSpan TokenExpiration);
}
