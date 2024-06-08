using System.Reflection;

namespace MinigolfFriday.IntegrationTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DatabaseProviderDataSourceAttribute : Attribute, ITestDataSource
{
    public IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        return [
            [DatabaseProvider.Sqlite],
            [DatabaseProvider.PostgreSql],

            // Currently excluded as resetting the DB in MSSql takes 10 seconds per test for some reason
            //[DatabaseProvider.MsSql]
        ];
    }

    public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        if (data == null)
        {
            return null;
        }

        return $"{methodInfo.Name} ({data[0]})";
    }
}
