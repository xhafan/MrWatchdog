using CoreDdd.Nhibernate.DatabaseSchemaGenerators;
using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using System.Text.RegularExpressions;

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
            
            // git status --find-renames=10% | findstr /R /C:"renamed:" > renamed.txt
            // git status --find-renames=10% | findstr /R /C:"new file:" /C:"deleted:" > added_deleted.txt
            Console.WriteLine(
                "3 Generate \"git mv\" script for renamed files from \"git status --find-renames=10%\" output. Direct usage: MrWatchdog.ToolsApp generateGitMoveScript <input file> <output file>"
            );

            selectedOption = Console.ReadLine();
        }
        else
        {
            selectedOption = args[0];
        }

        if (selectedOption == "1" || selectedOption == "generateDatabaseSchema") _generateDatabaseSchemaSqlFile();
        if (selectedOption == "2" || selectedOption == "convertPrivateKeyToOneLineForJson") _convertPrivateKeyToOneLine();
        if (selectedOption == "3" || selectedOption == "generateGitMoveScript") _generateGitMoveScript();

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
            var oneLinePrivateKey = Regex.Replace(privateKeyContent, @"\r\n|\n|\r", "");
            File.WriteAllText(outputFileName, oneLinePrivateKey);
            Console.WriteLine($"Private key has been converted to one line and saved to {outputFileName}");
        }

        void _generateGitMoveScript()
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

            var outputLines = new List<string>();
            var renameLines = File.ReadAllLines(inputFileName);
            foreach (var renameLine in renameLines)
            {
                var match = Regex.Match(renameLine, @"renamed:\s+(.*?)\s+->\s+(.*)");

                if (match.Success)
                {
                    var oldPath = match.Groups[1].Value;
                    var newPath = match.Groups[2].Value;

                    var targetDir = Path.GetDirectoryName(newPath);
                    if (targetDir == null) throw new Exception($"Error getting directory from {newPath}");
                    var windowsDir = targetDir.Replace('/', '\\');
                    var mkdirCommand = $"if not exist \"{windowsDir}\" mkdir \"{windowsDir}\"";
                    outputLines.Add(mkdirCommand);

                    var gitCommand = $"git mv \"{oldPath}\" \"{newPath}\"";
                    outputLines.Add(gitCommand);

                    // Replace "watchdog" with "scraper" in old path (case-insensitive)
                    var transformedOldPath = Regex.Replace(oldPath, "watchdog_searches", "watchdogs", RegexOptions.IgnoreCase);
                    transformedOldPath = Regex.Replace(transformedOldPath, "watchdog_search", "watchdog", RegexOptions.IgnoreCase);
                    transformedOldPath = Regex.Replace(transformedOldPath, "watchdogs_searches", "watchdogs", RegexOptions.IgnoreCase);
                    transformedOldPath = Regex.Replace(transformedOldPath, "WatchdogSearches", "Watchdogs", RegexOptions.IgnoreCase);
                    transformedOldPath = Regex.Replace(transformedOldPath, "WatchdogSearch", "Watchdog", RegexOptions.IgnoreCase);
                    transformedOldPath = Regex.Replace(transformedOldPath, "/Search/", "/Detail/", RegexOptions.IgnoreCase);
                    transformedOldPath = Regex.Replace(transformedOldPath, "scrapers", "watchdogs", RegexOptions.IgnoreCase);

                    var transformedNewPath = Regex.Replace(newPath, "scrapers", "watchdogs", RegexOptions.IgnoreCase);

                    if (!string.Equals(transformedOldPath, transformedNewPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine(gitCommand);
                    }
                }
                else
                {
                    Console.WriteLine("Line format not recognized.");
                }
            }

            File.WriteAllLines(outputFileName, outputLines);
        }
    }
}