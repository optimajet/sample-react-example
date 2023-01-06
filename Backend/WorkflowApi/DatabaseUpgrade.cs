using Microsoft.Data.SqlClient;

namespace WorkflowApi;

public class DatabaseUpgrade
{
    private const int AttemptCount = 100;
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(5);

    private readonly string _connectionString;
    private readonly string _sqlScript;

    private DatabaseUpgrade(string connectionString, string sqlScript)
    {
        _connectionString = connectionString;
        _sqlScript = sqlScript;
    }

    public static async Task WaitForUpgrade(string connectionString)
    {
        var sql = await File.ReadAllTextAsync("./Sql/CreatePersistenceObjects.sql");
        var instance = new DatabaseUpgrade(connectionString, sql);
        await instance.Upgrade();
        await Console.Out.WriteLineAsync("The database has been upgraded");
    }

    private async Task Upgrade(int attemptNumber = 0)
    {
        try
        {
            await Console.Out.WriteLineAsync($"Upgrading database, attempt number: {attemptNumber}");
            await UpgradeDatabase();
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            if (attemptNumber >= AttemptCount) throw;

            await Task.Delay(Delay);
            await Upgrade(attemptNumber + 1);
        }
    }

    private async Task UpgradeDatabase()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = _sqlScript;
        await command.ExecuteNonQueryAsync();
    }
}