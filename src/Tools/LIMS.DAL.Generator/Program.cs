using LIMS.DAL.Generator;
using Microsoft.Extensions.Configuration;

Console.WriteLine("LIMS DAL Generator - Schema-Driven Repository Generator");
Console.WriteLine("=======================================================\n");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("LIMSDatabase")
    ?? throw new InvalidOperationException("Connection string 'LIMSDatabase' not found");

var outputPath = configuration["OutputPath"] ?? "../../../Generated";
var namespaceName = configuration["Namespace"] ?? "LIMS.Generated";
var schemaName = configuration["Schema"] ?? "dbo";

Console.WriteLine($"Connection: {connectionString.Split(';')[0]}");
Console.WriteLine($"Schema: {schemaName}");
Console.WriteLine($"Output: {outputPath}");
Console.WriteLine($"Namespace: {namespaceName}\n");

var generator = new SchemaReader(connectionString);
var tables = await generator.ReadSchemaAsync(schemaName);

Console.WriteLine($"Found {tables.Count} tables:");
foreach (var table in tables)
{
    Console.WriteLine($"  - {table.TableName} ({table.Columns.Count} columns)");
}

var codeGenerator = new RepositoryCodeGenerator(namespaceName);

Directory.CreateDirectory(Path.Combine(outputPath, "Entities"));
Directory.CreateDirectory(Path.Combine(outputPath, "Repositories"));

Console.WriteLine("\nGenerating code...");

foreach (var table in tables)
{
    // Generate entity class
    var entityCode = codeGenerator.GenerateEntity(table);
    var entityPath = Path.Combine(outputPath, "Entities", $"{table.TableName}.cs");
    await File.WriteAllTextAsync(entityPath, entityCode);
    Console.WriteLine($"  Generated entity: {table.TableName}.cs");

    // Generate repository class
    var repositoryCode = codeGenerator.GenerateRepository(table);
    var repositoryPath = Path.Combine(outputPath, "Repositories", $"{table.TableName}Repository.cs");
    await File.WriteAllTextAsync(repositoryPath, repositoryCode);
    Console.WriteLine($"  Generated repository: {table.TableName}Repository.cs");
}

Console.WriteLine("\n✓ Code generation complete!");
