using System.Data;
using CoreUtils;
using DatabaseBuilder;
using Microsoft.Extensions.Logging;

namespace CoreBackend.Infrastructure;

public static class DatabaseBuilderHelper
{
    public static void BuildDatabase(
        Func<IDbConnection> createConnectionFunc,
        string databaseScriptsDirectoryPath, 
        ILogger logger,
        string environmentName
    )
    {
        logger.LogInformation("DatabaseScripts directory path: {dbScriptsDirectoryPath}", databaseScriptsDirectoryPath);

        var builderOfDatabase = new BuilderOfDatabase(createConnectionFunc, logAction: msg => logger.LogInformation(msg));
        builderOfDatabase.BuildDatabase(databaseScriptsDirectoryPath);

        _CheckDatabaseEnvironmentMatchesAppEnvironment(createConnectionFunc, environmentName);
    }

    private static void _CheckDatabaseEnvironmentMatchesAppEnvironment(
        Func<IDbConnection> createConnectionFunc, 
        string environmentName
    )
    {
        using var connection = createConnectionFunc();
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
