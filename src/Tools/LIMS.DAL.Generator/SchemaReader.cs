using Dapper;
using Microsoft.Data.SqlClient;

namespace LIMS.DAL.Generator;

public class SchemaReader
{
    private readonly string _connectionString;

    public SchemaReader(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<TableInfo>> ReadSchemaAsync(string schemaName = "dbo")
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var tables = new List<TableInfo>();

        // Get all tables with temporal table information
        var tableQuery = @"
            SELECT
                t.name AS TableName,
                t.temporal_type_desc AS TemporalType,
                SCHEMA_NAME(t.schema_id) AS SchemaName
            FROM sys.tables t
            WHERE SCHEMA_NAME(t.schema_id) = @SchemaName
            AND t.is_ms_shipped = 0
            AND t.temporal_type IN (0, 2) -- Regular or System-versioned temporal tables
            ORDER BY t.name";

        var tableNames = await connection.QueryAsync<(string TableName, string TemporalType, string SchemaName)>(
            tableQuery, new { SchemaName = schemaName });

        foreach (var (tableName, temporalType, schema) in tableNames)
        {
            var columns = await GetColumnsAsync(connection, schema, tableName);
            var primaryKey = await GetPrimaryKeyAsync(connection, schema, tableName);
            var foreignKeys = await GetForeignKeysAsync(connection, schema, tableName);

            tables.Add(new TableInfo
            {
                TableName = tableName,
                SchemaName = schema,
                IsTemporalTable = temporalType == "SYSTEM_VERSIONED_TEMPORAL_TABLE",
                Columns = columns,
                PrimaryKey = primaryKey,
                ForeignKeys = foreignKeys
            });
        }

        return tables;
    }

    private async Task<List<ColumnInfo>> GetColumnsAsync(SqlConnection connection, string schemaName, string tableName)
    {
        var query = @"
            SELECT
                c.name AS ColumnName,
                TYPE_NAME(c.user_type_id) AS DataType,
                c.max_length AS MaxLength,
                c.precision AS Precision,
                c.scale AS Scale,
                c.is_nullable AS IsNullable,
                c.is_identity AS IsIdentity,
                c.is_computed AS IsComputed,
                CAST(CASE WHEN c.generated_always_type IN (1, 2) THEN 1 ELSE 0 END AS BIT) AS IsTemporalColumn
            FROM sys.columns c
            INNER JOIN sys.tables t ON c.object_id = t.object_id
            WHERE t.name = @TableName
            AND SCHEMA_NAME(t.schema_id) = @SchemaName
            ORDER BY c.column_id";

        var columns = await connection.QueryAsync<ColumnInfo>(query, new { SchemaName = schemaName, TableName = tableName });
        return columns.ToList();
    }

    private async Task<string?> GetPrimaryKeyAsync(SqlConnection connection, string schemaName, string tableName)
    {
        var query = @"
            SELECT c.name
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            WHERE t.name = @TableName
            AND SCHEMA_NAME(t.schema_id) = @SchemaName
            AND i.is_primary_key = 1";

        var pkColumn = await connection.QueryFirstOrDefaultAsync<string>(query, new { SchemaName = schemaName, TableName = tableName });
        return pkColumn;
    }

    private async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(SqlConnection connection, string schemaName, string tableName)
    {
        var query = @"
            SELECT
                fk.name AS ForeignKeyName,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
                OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
            WHERE t.name = @TableName
            AND SCHEMA_NAME(t.schema_id) = @SchemaName";

        var fks = await connection.QueryAsync<ForeignKeyInfo>(query, new { SchemaName = schemaName, TableName = tableName });
        return fks.ToList();
    }
}

public class TableInfo
{
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public bool IsTemporalTable { get; set; }
    public List<ColumnInfo> Columns { get; set; } = new();
    public string? PrimaryKey { get; set; }
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
}

public class ColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int MaxLength { get; set; }
    public byte Precision { get; set; }
    public byte Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsComputed { get; set; }
    public bool IsTemporalColumn { get; set; }
}

public class ForeignKeyInfo
{
    public string ForeignKeyName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
}
