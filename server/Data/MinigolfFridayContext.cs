using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MinigolfFriday.Data;

public class MinigolfFridayContext : DbContext
{
    private readonly ILoggerFactory? _loggerFactory;

    public DbSet<PlayerEntity> Players { get; set; }

    public string DbPath { get; }

    public MinigolfFridayContext(ILoggerFactory? loggerFactory)
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
}
