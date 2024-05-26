using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MinigolfFriday.IntegrationTests.Builders;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MinigolfFriday.IntegrationTests;

[TestClass]
internal sealed class Sut : IAsyncDisposable
{
    private static readonly ConcurrentQueue<IContainer> _freeAppContainers = new();

    private readonly HttpClient _appHttpClient;
    private readonly DateTime _start;

    public IContainer App { get; }
    public MinigolfFridayClient AppClient { get; }

    private Sut(DateTime start, IContainer app)
    {
        _start = start;

        _appHttpClient = new HttpClient();
        App = app;
        AppClient = new MinigolfFridayClient(
          $"http://{App.Hostname}:{App.GetMappedPublicPort(80)}",
          _appHttpClient
        );
    }

    public async Task<string> Token(UserBuilder.Result userResult) =>
      await Token(userResult.LoginToken);

    public async Task<string> Token(string loginToken) =>
      (await AppClient.GetTokenAsync(new() { LoginToken = loginToken })).Token;

    public UserBuilder User(string? alias = null) => new(this, alias);

    public MinigolfMapBuilder MinigolfMap(string? name = null) => new(this, name);

    public EventBuilder Event() => new(this);

    public EventTimeslotBuilder EventTimeslot(TimeSpan time, string mapId) => new(this, time, mapId);

    public EventInstancePreconfigurationBuilder EventInstancePreconfiguration() => new(this);

    public async ValueTask DisposeAsync()
    {
        try
        {
            (
              await _appHttpClient.PostAsync($"{AppClient.BaseUrl}api/dev/resetdb", null)
            ).EnsureSuccessStatusCode();

            await TraceContainerLog(App);

            _freeAppContainers.Enqueue(App);
        }
        catch
        {
            await TraceContainerLog(App);
            await App.DisposeAsync();
            throw;
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

    public static async Task<Sut> CreateAsync()
    {
        var start = DateTime.Now;
        if (!_freeAppContainers.TryDequeue(out var container))
            container = await CreateContainerAsync();
        try
        {
            var scope = new Sut(start, container);
            scope.AppClient.Token = (
              await scope.AppClient.GetTokenAsync(new() { LoginToken = "admin" })
            ).Token;
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
        await Task.WhenAll(_freeAppContainers.Select(x => x.DisposeAsync()).Select(x => x.AsTask()));
    }

    private static async Task<IContainer> CreateContainerAsync()
    {
        var container = new ContainerBuilder()
          .WithImage("masch0212/minigolf-friday:intttest")
          .WithPortBinding(80, true)
          .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT.ASPNETCORE", "Information")
          .WithWaitStrategy(
            Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPath("healthz"))
          )
          .Build();

        await container.StartAsync();

        return container;
    }
}
