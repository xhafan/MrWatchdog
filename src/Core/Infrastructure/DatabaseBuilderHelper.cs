using DatabaseBuilder;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MrWatchdog.Core.Infrastructure;

public static class DatabaseBuilderHelper
{
    public static void BuildDatabase(
        string connectionString, 
        string databaseScriptsDirectoryPath, 
        ILogger logger
    )
    {
        logger.LogInformation("DatabaseScripts directory path: {dbScriptsDirectoryPath}", databaseScriptsDirectoryPath);

        var builderOfDatabase = new BuilderOfDatabase(() => new NpgsqlConnection(connectionString), logAction: msg => logger.LogInformation(msg));
        builderOfDatabase.BuildDatabase(databaseScriptsDirectoryPath);
    }
}
