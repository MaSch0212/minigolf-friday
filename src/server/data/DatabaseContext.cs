using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Options;

namespace MinigolfFriday.Data;

public class DatabaseContext(
    IOptions<DatabaseOptions> databaseOptions,
    ILoggerFactory loggerFactory,
    IOptionsMonitor<LoggingOptions> loggingOptions
) : DbContext
{
    public DbSet<MinigolfMapEntity> Maps { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<EventTimeslotEntity> EventTimeslots { get; set; }
    public DbSet<EventInstanceEntity> EventInstances { get; set; }
    public DbSet<EventTimeslotRegistrationEntity> EventTimeslotRegistrations { get; set; }
    public DbSet<EventInstancePreconfigurationEntity> EventInstancePreconfigurations { get; set; }

    public RoleEntity RoleById(Role role)
    {
        var existing = ChangeTracker.Entries<RoleEntity>().FirstOrDefault(x => x.Entity.Id == role);
        if (existing != null)
            return existing.Entity;
        var entity = new RoleEntity { Id = role, Name = null! };
        Entry(entity).State = EntityState.Unchanged;
        return entity;
    }

    public UserEntity UserById(long id)
    {
        var existing = ChangeTracker.Entries<UserEntity>().FirstOrDefault(x => x.Entity.Id == id);
        if (existing != null)
            return existing.Entity;
        var entity = new UserEntity
        {
            Id = id,
            Alias = null!,
            LoginToken = null!
        };
        Entry(entity).State = EntityState.Unchanged;
        return entity;
    }

    public MinigolfMapEntity MapById(long id)
    {
        var existing = ChangeTracker
            .Entries<MinigolfMapEntity>()
            .FirstOrDefault(x => x.Entity.Id == id);
        if (existing != null)
            return existing.Entity;
        var entity = new MinigolfMapEntity { Id = id, Name = default! };
        Entry(entity).State = EntityState.Unchanged;
        return entity;
    }

    public EventEntity EventById(long id)
    {
        var existing = ChangeTracker.Entries<EventEntity>().FirstOrDefault(x => x.Entity.Id == id);
        if (existing != null)
            return existing.Entity;
        var entity = new EventEntity
        {
            Id = id,
            Date = default,
            RegistrationDeadline = default
        };
        Entry(entity).State = EntityState.Unchanged;
        return entity;
    }

    public EventTimeslotEntity EventTimeslotById(long id)
    {
        var existing = ChangeTracker
            .Entries<EventTimeslotEntity>()
            .FirstOrDefault(x => x.Entity.Id == id);
        if (existing != null)
            return existing.Entity;
        var entity = new EventTimeslotEntity { Id = id, Time = default, };
        Entry(entity).State = EntityState.Unchanged;
        return entity;
    }

    public EventInstancePreconfigurationEntity PreconfigurationById(long id)
    {
        var existing = ChangeTracker
            .Entries<EventInstancePreconfigurationEntity>()
            .FirstOrDefault(x => x.Entity.Id == id);
        if (existing != null)
            return existing.Entity;
        var entity = new EventInstancePreconfigurationEntity { Id = id };
        Entry(entity).State = EntityState.Unchanged;
        return entity;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        _ = databaseOptions.Value.Provider switch
        {
            DatabaseProvider.Sqlite
                => ConfigureSqlite(options, databaseOptions.Value.SqliteConnectionString),
            DatabaseProvider.MsSql
                => ConfigureMsSql(options, databaseOptions.Value.MsSqlConnectionString),
            DatabaseProvider.PostgreSql
                => ConfigurePostgreSql(options, databaseOptions.Value.PostgreSqlConnectionString),
            _
                => throw new ArgumentOutOfRangeException(
                    $"{DatabaseOptions.SectionPath}:Provider",
                    $"Invalid provider {databaseOptions.Value.Provider}"
                )
        };

        if (loggingOptions.CurrentValue.EnableDbLogging == true)
        {
            options.UseLoggerFactory(loggerFactory);
#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<EventEntity>(EventEntity.Configure);
        builder.Entity<EventInstanceEntity>(EventInstanceEntity.Configure);
        builder.Entity<EventInstancePreconfigurationEntity>(
            EventInstancePreconfigurationEntity.Configure
        );
        builder.Entity<EventTimeslotEntity>(EventTimeslotEntity.Configure);
        builder.Entity<EventTimeslotRegistrationEntity>(EventTimeslotRegistrationEntity.Configure);
        builder.Entity<MinigolfMapEntity>(MinigolfMapEntity.Configure);
        builder.Entity<RoleEntity>(RoleEntity.Configure);
        builder.Entity<UserEntity>(UserEntity.Configure);
    }

    private static DbContextOptionsBuilder ConfigureSqlite(
        DbContextOptionsBuilder options,
        string connectionString
    )
    {
        var parsedConStr = new SqliteConnectionStringBuilder(connectionString);

        Directory.CreateDirectory(
            Path.GetDirectoryName(parsedConStr.DataSource)
                ?? throw new ArgumentException(
                    "Invalid connection string",
                    nameof(connectionString)
                )
        );

        parsedConStr.DataSource = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, parsedConStr.DataSource)
        );

        return options.UseSqlite(
            parsedConStr.ConnectionString,
            o =>
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("MinigolfFriday.Migrations.Sqlite")
        );
    }

    private static DbContextOptionsBuilder ConfigureMsSql(
        DbContextOptionsBuilder options,
        string connectionString
    )
    {
        return options.UseSqlServer(
            connectionString,
            o =>
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("MinigolfFriday.Migrations.MsSql")
        );
    }

    private static DbContextOptionsBuilder ConfigurePostgreSql(
        DbContextOptionsBuilder options,
        string connectionString
    )
    {
        return options.UseNpgsql(
            connectionString,
            o =>
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                    .MigrationsAssembly("MinigolfFriday.Migrations.PostgreSql")
        );
    }
}
