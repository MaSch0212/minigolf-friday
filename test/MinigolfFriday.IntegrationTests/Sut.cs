using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Data.SqlClient;
using MinigolfFriday.IntegrationTests.Builders;
using Npgsql;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace MinigolfFriday.IntegrationTests;

internal interface IHttpClientAccessor
{
    HttpClient HttpClient { get; }
}

[TestClass]
internal sealed class Sut : IAsyncDisposable, IHttpClientAccessor
{
    private static readonly SemaphoreSlim _networkLock = new(1);
    private static readonly SemaphoreSlim _mssqlLock = new(1);
    private static readonly SemaphoreSlim _postgresLock = new(1);

    private static readonly ConcurrentQueue<AppContainer> _freeSqliteAppContainers = new();
    private static readonly ConcurrentQueue<AppContainer> _freeMssqlAppContainers = new();
    private static readonly ConcurrentQueue<AppContainer> _freePostgresContainers = new();
    private static INetwork? _network;
    private static IContainer? _mssqlContainer;
    private static IContainer? _postgresContainer;

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
            await TraceContainerLog(App, _start);
            await AppClient.ResetDatabase();
            GetAppContainerQueue(_app.DatabaseProvider).Enqueue(_app);
        }
        catch
        {
            await App.DisposeAsync();
            throw;
        }
        finally
        {
            _httpClient.Dispose();
        }
    }

    private static async Task TraceContainerLog(
        IContainer container,
        DateTime since = default,
        [CallerArgumentExpression(nameof(container))] string containerName = ""
    )
    {
        var (stdout, stderr) = await container.GetLogsAsync(since);
        Trace.WriteLine($"{containerName} (stdout):\n{stdout}");
        Trace.WriteLine($"{containerName} (stderr):\n{stderr}");
    }

    public static async Task<Sut> CreateAsync(
        DatabaseProvider databaseProvider,
        bool getAdminToken = true
    )
    {
        var start = DateTime.Now;
        var queue = GetAppContainerQueue(databaseProvider);
        if (!queue.TryDequeue(out var container))
            container = await CreateContainerAsync(databaseProvider);
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
            queue.Enqueue(container);
            throw;
        }
    }

    [AssemblyInitialize]
    public static void InitializeAssembly(TestContext _)
    {
        AssertionOptions.AssertEquivalencyUsing(options =>
            options
                .Using<DateTimeOffset>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))
                )
                .WhenTypeIs<DateTimeOffset>()
        );
    }

    [AssemblyCleanup]
    public static async Task CleanupAssembly()
    {
        await Task.WhenAll(
            new[] { _freeSqliteAppContainers, _freeMssqlAppContainers, _freePostgresContainers }
                .SelectMany(x => x)
                .Select(x => x.Container.DisposeAsync())
                .Select(x => x.AsTask())
        );
        if (_mssqlContainer != null)
            await _mssqlContainer.DisposeAsync();
        if (_postgresContainer != null)
            await _postgresContainer.DisposeAsync();
        if (_network != null)
            await _network.DisposeAsync();
    }

    private static ConcurrentQueue<AppContainer> GetAppContainerQueue(
        DatabaseProvider databaseProvider
    ) =>
        databaseProvider switch
        {
            DatabaseProvider.Sqlite => _freeSqliteAppContainers,
            DatabaseProvider.MsSql => _freeMssqlAppContainers,
            DatabaseProvider.PostgreSql => _freePostgresContainers,
            _ => throw new ArgumentOutOfRangeException()
        };

    private static async Task<INetwork> GetNetworkAsync()
    {
        await _networkLock.WaitAsync();
        try
        {
            if (_network != null)
                return _network;

            var network = new NetworkBuilder().Build();
            await network.CreateAsync();
            _network = network;
            return network;
        }
        finally
        {
            _networkLock.Release();
        }
    }

    private static async Task<AppContainer> CreateContainerAsync(DatabaseProvider databaseProvider)
    {
        var tokenExpiration = TimeSpan.FromSeconds(Random.Shared.Next(300, 180000));
        var builder = new ContainerBuilder()
            .WithImage("masch0212/minigolf-friday:intttest")
            .WithPortBinding(80, true)
            .WithEnvironment("LOGGING__LOGLEVEL__MICROSOFT.ASPNETCORE", "Information")
            .WithEnvironment("LOGGING__ENABLEDBLOGGING", "true")
            .WithEnvironment("AUTHENTICATION__JWT__EXPIRATION", tokenExpiration.ToString())
            .WithEnvironment("DATABASE__PROVIDER", databaseProvider.ToString())
            .WithNetwork(await GetNetworkAsync())
            .WithWaitStrategy(
                Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(x => x.ForPath("healthz"))
            );

        string? databaseName = null;
        if (databaseProvider == DatabaseProvider.Sqlite)
        {
            builder = builder.WithEnvironment(
                "DATABASE__SQLITECONNECTIONSTRING",
                "Data Source=data/MinigolfFriday.db"
            );
        }
        else if (databaseProvider == DatabaseProvider.MsSql)
        {
            var dbContainer = await GetMsSqlContainerAsync();
            databaseName = $"MF-{Guid.NewGuid().ToString("N")[..8]}";

            using var db = new SqlConnection(
                $"Server=localhost,{dbContainer.GetMappedPublicPort(1433)};User=sa;Password=p@ssw0rd;TrustServerCertificate=True;"
            );
            await db.OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE [{databaseName}]";
            await cmd.ExecuteNonQueryAsync();

            builder = builder
                .WithEnvironment(
                    "DATABASE__MSSQLCONNECTIONSTRING",
                    $"Server=mssql,1433;User=sa;Password=p@ssw0rd;Database={databaseName};TrustServerCertificate=True;"
                )
                .DependsOn(dbContainer);
        }
        else if (databaseProvider == DatabaseProvider.PostgreSql)
        {
            var dbContainer = await GetPostgresContainerAsync();
            databaseName = $"mf_{Guid.NewGuid().ToString("N")[..8].ToLowerInvariant()}";

            using var db = new NpgsqlConnection(
                $"Server=localhost;Port={dbContainer.GetMappedPublicPort(5432)};User Id=postgres;Password=p@ssw0rd;"
            );
            await db.OpenAsync();
            using var cmd = db.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE {databaseName}";
            await cmd.ExecuteNonQueryAsync();

            builder = builder
                .WithEnvironment(
                    "DATABASE__POSTGRESQLCONNECTIONSTRING",
                    $"Server=postgres;Port=5432;User Id=postgres;Password=p@ssw0rd;Database={databaseName};"
                )
                .DependsOn(dbContainer);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(databaseProvider));
        }

        var container = builder.Build();
        try
        {
            await container.StartAsync();
        }
        catch
        {
            await TraceContainerLog(container);
            throw;
        }

        return new(container, tokenExpiration, databaseProvider, databaseName);
    }

    private static async Task<IContainer> GetMsSqlContainerAsync()
    {
        await _mssqlLock.WaitAsync();
        try
        {
            if (_mssqlContainer != null)
                return _mssqlContainer;

            var container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPortBinding(1433, true)
                .WithPassword("p@ssw0rd")
                .WithNetwork(await GetNetworkAsync())
                .WithNetworkAliases("mssql")
                .Build();
            await container.StartAsync();
            _mssqlContainer = container;
            return container;
        }
        finally
        {
            _mssqlLock.Release();
        }
    }

    private static async Task<IContainer> GetPostgresContainerAsync()
    {
        await _postgresLock.WaitAsync();
        try
        {
            if (_postgresContainer != null)
                return _postgresContainer;

            var container = new PostgreSqlBuilder()
                .WithImage("postgres:14")
                .WithPortBinding(5432, true)
                .WithUsername("postgres")
                .WithPassword("p@ssw0rd")
                .WithDatabase("postgres")
                .WithNetwork(await GetNetworkAsync())
                .WithNetworkAliases("postgres")
                .Build();
            await container.StartAsync();
            _postgresContainer = container;
            return container;
        }
        finally
        {
            _postgresLock.Release();
        }
    }

    private record AppContainer(
        IContainer Container,
        TimeSpan TokenExpiration,
        DatabaseProvider DatabaseProvider,
        string? DatabaseName
    );
}
