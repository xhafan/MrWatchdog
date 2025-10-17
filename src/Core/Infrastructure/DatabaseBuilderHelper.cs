using CoreUtils;
using DatabaseBuilder;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MrWatchdog.Core.Infrastructure;

public static class DatabaseBuilderHelper
{
    public static void BuildDatabase(
        string connectionString, 
        string databaseScriptsDirectoryPath, 
        ILogger logger,
        string environmentName
    )
    {
        logger.LogInformation("DatabaseScripts directory path: {dbScriptsDirectoryPath}", databaseScriptsDirectoryPath);

        var builderOfDatabase = new BuilderOfDatabase(() => new NpgsqlConnection(connectionString), logAction: msg => logger.LogInformation(msg));
        builderOfDatabase.BuildDatabase(databaseScriptsDirectoryPath);

        _CheckDatabaseEnvironmentMatchesAppEnvironment(connectionString, environmentName);
    }

    private static void _CheckDatabaseEnvironmentMatchesAppEnvironment(
        string connectionString, 
        string environmentName
    )
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            select "Value" from "Environment"
            """;

        var databaseEnvironment = (string?)command.ExecuteScalar();
        connection.Close();

        Guard.Hope(databaseEnvironment == environmentName,
            $"Database environment {databaseEnvironment} does not match the application environment {environmentName}.");
    }
}
