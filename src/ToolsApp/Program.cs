using CoreDdd.Nhibernate.DatabaseSchemaGenerators;
using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;

namespace MrWatchdog.ToolsApp;

public class Program
{
    public static void Main(string[] args)
    {

        string? selectedOption;
        if (args.Length == 0)
        {
            Console.WriteLine("Choose from the following options and press enter:");
            Console.WriteLine("1 Generate database schema sql file");

            selectedOption = Console.ReadLine();
        }
        else
        {
            selectedOption = args[0];
        }

        if (selectedOption == "1") _generateDatabaseSchemaSqlFile();

        return;

        void _generateDatabaseSchemaSqlFile()
        {
            const string databaseSchemaFileName = "MrWatchdog_generated_database_schema.sql";

            var connectionStringName = ConsoleAppSettings.Configuration["DatabaseConnectionStringName"];
            Guard.Hope(connectionStringName != null, nameof(connectionStringName) + " is null");
            var connectionString = ConsoleAppSettings.Configuration.GetConnectionString(connectionStringName);
            Guard.Hope(connectionString != null, nameof(connectionString) + " is null");
            using var nhibernateConfigurator = new NhibernateConfigurator(connectionString);
            new DatabaseSchemaGenerator(databaseSchemaFileName, nhibernateConfigurator).Generate();
            Console.WriteLine($"Database schema sql file has been generated into {databaseSchemaFileName}");

            _addSemicolonsToPostgreSqlScript();
            return;

            void _addSemicolonsToPostgreSqlScript()
            {
                var sqlScript = File.ReadAllText(databaseSchemaFileName);
                sqlScript = sqlScript.Replace("\r\n", ";\r\n");
                File.WriteAllText(databaseSchemaFileName, sqlScript);
            }
        }
    }
}