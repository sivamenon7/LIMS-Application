using Dapper;
using LIMS.Core.Entities;
using LIMS.Core.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text;

namespace LIMS.Infrastructure.Data;

public class DapperRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbConnectionFactory _connectionFactory;
    protected readonly string _tableName;

    public DapperRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _tableName = GetTableName();
    }

    private static string GetTableName()
    {
        var type = typeof(T);
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Name ?? $"{type.Name}s";
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id AND IsDeleted = 0";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {_tableName} WHERE IsDeleted = 0";
        return await connection.QueryAsync<T>(sql);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(string whereClause, object? parameters = null, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {_tableName} WHERE {whereClause} AND IsDeleted = 0";
        return await connection.QueryAsync<T>(sql, parameters);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var properties = GetProperties(excludeKey: true);
        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var sql = $@"
            INSERT INTO {_tableName} ({columns})
            VALUES ({values});
            SELECT * FROM {_tableName} WHERE Id = @Id";

        return await connection.QuerySingleAsync<T>(sql, entity);
    }

    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        entity.ModifiedAt = DateTime.UtcNow;

        var properties = GetProperties(excludeKey: true);
        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

        var sql = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id AND IsDeleted = 0";

        var result = await connection.ExecuteAsync(sql, entity);
        return result > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = $@"
            UPDATE {_tableName}
            SET IsDeleted = 1, DeletedAt = @DeletedAt
            WHERE Id = @Id";

        var result = await connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        return result > 0;
    }

    public virtual async Task<int> CountAsync(string? whereClause = null, object? parameters = null, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = $"SELECT COUNT(*) FROM {_tableName} WHERE IsDeleted = 0";
        if (!string.IsNullOrWhiteSpace(whereClause))
        {
            sql += $" AND {whereClause}";
        }

        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE Id = @Id AND IsDeleted = 0";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    private static IEnumerable<PropertyInfo> GetProperties(bool excludeKey = false)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanWrite && !p.GetCustomAttributes<NotMappedAttribute>().Any());

        if (excludeKey)
        {
            properties = properties.Where(p => p.Name != "Id");
        }

        return properties;
    }
}
