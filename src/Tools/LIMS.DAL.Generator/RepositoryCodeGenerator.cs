using System.Text;

namespace LIMS.DAL.Generator;

public class RepositoryCodeGenerator
{
    private readonly string _namespace;

    public RepositoryCodeGenerator(string namespaceName)
    {
        _namespace = namespaceName;
    }

    public string GenerateEntity(TableInfo table)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using LIMS.Shared.Core.Entities;");
        sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
        sb.AppendLine();
        sb.AppendLine($"namespace {_namespace}.Entities;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Entity for {table.TableName} table.");
        sb.AppendLine($"/// Generated from database schema: {table.SchemaName}.{table.TableName}");
        sb.AppendLine($"/// Temporal table: {table.IsTemporalTable}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"[Table(\"{table.TableName}\", Schema = \"{table.SchemaName}\")]");
        sb.AppendLine($"public class {table.TableName} : BaseEntity");
        sb.AppendLine("{");

        foreach (var column in table.Columns)
        {
            // Skip base entity columns and temporal columns
            if (IsBaseEntityColumn(column.ColumnName) || column.IsTemporalColumn)
                continue;

            var csharpType = MapSqlTypeToCSharp(column.DataType, column.IsNullable);
            var propertyName = ToPascalCase(column.ColumnName);

            sb.AppendLine($"    /// <summary>{column.DataType}({GetTypeDetails(column)}){(column.IsNullable ? " NULL" : " NOT NULL")}</summary>");
            sb.AppendLine($"    public {csharpType} {propertyName} {{ get; set; }}{GetDefaultValue(csharpType)}");
        }

        // Add navigation properties for foreign keys
        if (table.ForeignKeys.Any())
        {
            sb.AppendLine();
            sb.AppendLine("    // Navigation Properties");
            foreach (var fk in table.ForeignKeys)
            {
                var propertyName = ToPascalCase(fk.ColumnName.Replace("Id", ""));
                sb.AppendLine($"    public {fk.ReferencedTable}? {propertyName} {{ get; set; }}");
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    public string GenerateRepository(TableInfo table)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using Dapper;");
        sb.AppendLine("using LIMS.Shared.Core.Interfaces;");
        sb.AppendLine("using LIMS.Shared.Infrastructure.Data;");
        sb.AppendLine($"using {_namespace}.Entities;");
        sb.AppendLine();
        sb.AppendLine($"namespace {_namespace}.Repositories;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Repository for {table.TableName}.");
        sb.AppendLine($"/// Generated from database schema: {table.SchemaName}.{table.TableName}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {table.TableName}Repository : IRepository<{table.TableName}>");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly IDbConnectionFactory _connectionFactory;");
        sb.AppendLine($"    private const string TableName = \"{table.SchemaName}.{table.TableName}\";");
        sb.AppendLine();
        sb.AppendLine($"    public {table.TableName}Repository(IDbConnectionFactory connectionFactory)");
        sb.AppendLine("    {");
        sb.AppendLine("        _connectionFactory = connectionFactory;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Generate CRUD methods
        sb.AppendLine(GenerateGetByIdMethod(table));
        sb.AppendLine(GenerateGetAllMethod(table));
        sb.AppendLine(GenerateFindMethod(table));
        sb.AppendLine(GenerateAddMethod(table));
        sb.AppendLine(GenerateUpdateMethod(table));
        sb.AppendLine(GenerateDeleteMethod(table));
        sb.AppendLine(GenerateCountMethod(table));
        sb.AppendLine(GenerateExistsMethod(table));

        // Generate temporal table methods if applicable
        if (table.IsTemporalTable)
        {
            sb.AppendLine(GenerateGetByIdAsOfMethod(table));
            sb.AppendLine(GenerateGetHistoryMethod(table));
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateGetByIdMethod(TableInfo table)
    {
        return $@"
    public async Task<{table.TableName}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT * FROM {{TableName}} WHERE Id = @Id AND IsDeleted = 0"";
        return await connection.QueryFirstOrDefaultAsync<{table.TableName}>(sql, new {{ Id = id }});
    }}";
    }

    private string GenerateGetAllMethod(TableInfo table)
    {
        return $@"
    public async Task<IEnumerable<{table.TableName}>> GetAllAsync(CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT * FROM {{TableName}} WHERE IsDeleted = 0"";
        return await connection.QueryAsync<{table.TableName}>(sql);
    }}";
    }

    private string GenerateFindMethod(TableInfo table)
    {
        return $@"
    public async Task<IEnumerable<{table.TableName}>> FindAsync(string whereClause, object? parameters = null, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT * FROM {{TableName}} WHERE {{whereClause}} AND IsDeleted = 0"";
        return await connection.QueryAsync<{table.TableName}>(sql, parameters);
    }}";
    }

    private string GenerateAddMethod(TableInfo table)
    {
        var columns = table.Columns
            .Where(c => !IsBaseEntityColumn(c.ColumnName) && !c.IsIdentity && !c.IsComputed && !c.IsTemporalColumn)
            .Select(c => c.ColumnName);

        var columnList = string.Join(", ", columns);
        var valueList = string.Join(", ", columns.Select(c => $"@{c}"));

        return $@"
    public async Task<{table.TableName}> AddAsync({table.TableName} entity, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @""
            INSERT INTO {{TableName}} ({columnList}, CreatedAt, CreatedBy, IsDeleted)
            VALUES ({valueList}, @CreatedAt, @CreatedBy, 0);
            SELECT * FROM {{TableName}} WHERE Id = @Id"";
        return await connection.QuerySingleAsync<{table.TableName}>(sql, entity);
    }}";
    }

    private string GenerateUpdateMethod(TableInfo table)
    {
        var columns = table.Columns
            .Where(c => !IsBaseEntityColumn(c.ColumnName) && !c.IsIdentity && !c.IsComputed && !c.IsTemporalColumn)
            .Select(c => $"{c.ColumnName} = @{c.ColumnName}");

        var setClause = string.Join(", ", columns);

        return $@"
    public async Task<bool> UpdateAsync({table.TableName} entity, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        entity.ModifiedAt = DateTime.UtcNow;
        var sql = $""UPDATE {{TableName}} SET {setClause}, ModifiedAt = @ModifiedAt, ModifiedBy = @ModifiedBy WHERE Id = @Id AND IsDeleted = 0"";
        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }}";
    }

    private string GenerateDeleteMethod(TableInfo table)
    {
        return $@"
    public async Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""UPDATE {{TableName}} SET IsDeleted = 1, DeletedAt = @DeletedAt, DeletedBy = @DeletedBy WHERE Id = @Id"";
        var result = await connection.ExecuteAsync(sql, new {{ Id = id, DeletedAt = DateTime.UtcNow, DeletedBy = deletedBy }});
        return result > 0;
    }}";
    }

    private string GenerateCountMethod(TableInfo table)
    {
        return $@"
    public async Task<int> CountAsync(string? whereClause = null, object? parameters = null, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT COUNT(*) FROM {{TableName}} WHERE IsDeleted = 0"";
        if (!string.IsNullOrWhiteSpace(whereClause))
            sql += $"" AND {{whereClause}}"";
        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }}";
    }

    private string GenerateExistsMethod(TableInfo table)
    {
        return $@"
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT COUNT(1) FROM {{TableName}} WHERE Id = @Id AND IsDeleted = 0"";
        var count = await connection.ExecuteScalarAsync<int>(sql, new {{ Id = id }});
        return count > 0;
    }}";
    }

    private string GenerateGetByIdAsOfMethod(TableInfo table)
    {
        return $@"
    public async Task<{table.TableName}?> GetByIdAsOfAsync(Guid id, DateTime asOf, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT * FROM {{TableName}} FOR SYSTEM_TIME AS OF @AsOf WHERE Id = @Id"";
        return await connection.QueryFirstOrDefaultAsync<{table.TableName}>(sql, new {{ Id = id, AsOf = asOf }});
    }}";
    }

    private string GenerateGetHistoryMethod(TableInfo table)
    {
        return $@"
    public async Task<IEnumerable<{table.TableName}>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $""SELECT * FROM {{TableName}} FOR SYSTEM_TIME ALL WHERE Id = @Id ORDER BY ValidFrom DESC"";
        return await connection.QueryAsync<{table.TableName}>(sql, new {{ Id = id }});
    }}";
    }

    private bool IsBaseEntityColumn(string columnName)
    {
        var baseColumns = new[] { "Id", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy",
            "IsDeleted", "DeletedAt", "DeletedBy", "ValidFrom", "ValidTo" };
        return baseColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase);
    }

    private string MapSqlTypeToCSharp(string sqlType, bool isNullable)
    {
        var csharpType = sqlType.ToLower() switch
        {
            "uniqueidentifier" => "Guid",
            "int" => "int",
            "bigint" => "long",
            "smallint" => "short",
            "tinyint" => "byte",
            "bit" => "bool",
            "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
            "float" => "double",
            "real" => "float",
            "datetime" or "datetime2" or "smalldatetime" => "DateTime",
            "date" => "DateTime",
            "time" => "TimeSpan",
            "datetimeoffset" => "DateTimeOffset",
            "nvarchar" or "varchar" or "nchar" or "char" or "text" or "ntext" => "string",
            "binary" or "varbinary" or "image" => "byte[]",
            _ => "object"
        };

        if (isNullable && csharpType != "string" && csharpType != "byte[]")
            return $"{csharpType}?";

        return csharpType;
    }

    private string GetDefaultValue(string csharpType)
    {
        if (csharpType == "string")
            return " = string.Empty;";
        if (csharpType == "byte[]")
            return " = Array.Empty<byte>();";
        return ";";
    }

    private string GetTypeDetails(ColumnInfo column)
    {
        return column.DataType.ToLower() switch
        {
            "decimal" or "numeric" => $"{column.Precision},{column.Scale}",
            "nvarchar" or "varchar" => column.MaxLength == -1 ? "MAX" : (column.MaxLength / 2).ToString(),
            _ => ""
        };
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
    }
}
