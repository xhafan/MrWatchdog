using System.Text.RegularExpressions;
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
            Console.WriteLine(
                "1 Generate database schema sql file. Direct usage: MrWatchdog.ToolsApp generateDatabaseSchema"
            );
            Console.WriteLine(
                "2 Convert private key to one line for JSON. Direct usage: MrWatchdog.ToolsApp convertPrivateKeyToOneLineForJson <input file> <output file>"
            );

            selectedOption = Console.ReadLine();
        }
        else
        {
            selectedOption = args[0];
        }

        if (selectedOption == "1" || selectedOption == "generateDatabaseSchema") _generateDatabaseSchemaSqlFile();
        if (selectedOption == "2" || selectedOption == "convertPrivateKeyToOneLineForJson") _convertPrivateKeyToOneLine();

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

        void _convertPrivateKeyToOneLine()
        {
            string? inputFileName;
            string? outputFileName;

            if (args.Length != 3)
            {
                Console.Write("Enter input file name: ");
                inputFileName = Console.ReadLine();
                Console.Write("Enter ouput file name: ");
                outputFileName = Console.ReadLine();
            }
            else
            {
                inputFileName = args[1];
                outputFileName = args[2];
            }

            Guard.Hope(!string.IsNullOrWhiteSpace(inputFileName), $"Invalid input file name: {inputFileName}");
            Guard.Hope(!string.IsNullOrWhiteSpace(outputFileName), $"Invalid input file name: {outputFileName}");

            var privateKeyContent = File.ReadAllText(inputFileName);
            var oneLinePrivateKey = Regex.Replace(privateKeyContent, @"\r\n|\n|\r", "\\n");
            File.WriteAllText(outputFileName, oneLinePrivateKey);
            Console.WriteLine($"Private key has been converted to one line and saved to {outputFileName}");
        }
    }
}