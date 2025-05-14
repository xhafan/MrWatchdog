using CoreDdd.Nhibernate.DatabaseSchemaGenerators;
using MrWatchdog.Core.Configurations;
using MrWatchdog.Core.Infrastructure;

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

    using var nhibernateConfigurator = new NhibernateConfigurator(ConsoleAppSettings.Configuration);
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