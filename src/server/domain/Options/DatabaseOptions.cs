using Microsoft.Extensions.Options;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Domain.Options;

public class DatabaseOptions : IOptionsWithSection
{
    public static string SectionPath => "Database";
    public static Type? ValidatorType => typeof(DatabaseOptionsValidator);

    public bool SkipMigration { get; set; } = false;
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;
    public string MsSqlConnectionString { get; set; } = null!;
    public string PostgreSqlConnectionString { get; set; } = null!;
    public string SqliteConnectionString { get; set; } = null!;
}

public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (
            options.Provider == DatabaseProvider.MsSql
            && string.IsNullOrEmpty(options.MsSqlConnectionString)
        )
        {
            return ValidateOptionsResult.Fail("Database:MsSqlConnectionString must be set");
        }

        if (
            options.Provider == DatabaseProvider.PostgreSql
            && string.IsNullOrEmpty(options.PostgreSqlConnectionString)
        )
        {
            return ValidateOptionsResult.Fail("Database:PostgreSqlConnectionString must be set");
        }

        if (
            options.Provider == DatabaseProvider.Sqlite
            && string.IsNullOrEmpty(options.SqliteConnectionString)
        )
        {
            return ValidateOptionsResult.Fail("Database:SqliteConnectionString must be set");
        }

        return ValidateOptionsResult.Success;
    }
}
