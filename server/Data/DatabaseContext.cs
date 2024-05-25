using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Models;

namespace MinigolfFriday.Data;

public class DatabaseContext : DbContext
{
    private readonly ILoggerFactory? _loggerFactory;

    public DbSet<MinigolfMapEntity> Maps { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<EventTimeslotEntity> EventTimeslots { get; set; }
    public DbSet<EventInstanceEntity> EventInstances { get; set; }
    public DbSet<EventTimeslotRegistrationEntity> EventTimeslotRegistrations { get; set; }
    public DbSet<EventInstancePreconfigurationEntity> EventInstancePreconfigurations { get; set; }

    public string DbPath { get; }

    public DatabaseContext(ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory;
        var folder = AppDomain.CurrentDomain.BaseDirectory;
        DbPath = Path.Combine(folder, "data/MinigolfFriday.db");
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath) ?? string.Empty);
    }

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
        options.UseSqlite(o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        options.UseLoggerFactory(_loggerFactory).UseSqlite($"Data Source={DbPath}");

#if DEBUG
        options.EnableSensitiveDataLogging();
#endif
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
}
