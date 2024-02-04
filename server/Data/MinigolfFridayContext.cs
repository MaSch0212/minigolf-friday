using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Models;

namespace MinigolfFriday.Data;

public class MinigolfFridayContext : DbContext
{
    private readonly ILoggerFactory? _loggerFactory;

    public DbSet<MinigolfMapEntity> Maps { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<EventTimeslotEntity> EventTimeslots { get; set; }
    public DbSet<EventInstanceEntity> EventInstances { get; set; }
    public DbSet<EventPlayerRegistrationEntity> EventPlayerRegistrations { get; set; }
    public DbSet<EventInstancePreconfigurationEntity> EventInstancePreconfigurations { get; set; }

    public string DbPath { get; }

    public MinigolfFridayContext(ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory;
        var folder = AppDomain.CurrentDomain.BaseDirectory;
        DbPath = Path.Combine(folder, "MinigolfFriday.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging()
            .UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<EventTimeslotEntity>()
            .HasMany(x => x.Registrations)
            .WithOne(x => x.EventTimeslot);
    }
}
