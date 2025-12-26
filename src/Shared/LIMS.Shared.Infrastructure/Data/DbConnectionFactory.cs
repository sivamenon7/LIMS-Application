using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace LIMS.Shared.Infrastructure.Data;

/// <summary>
/// Factory for creating database connections.
/// Each domain module can have its own database or share the same one.
/// </summary>
public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(string? connectionStringName = null);
    IDbConnection CreateConnection(string? connectionStringName = null);
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IDbConnection> CreateConnectionAsync(string? connectionStringName = null)
    {
        var connectionString = GetConnectionString(connectionStringName);
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public IDbConnection CreateConnection(string? connectionStringName = null)
    {
        var connectionString = GetConnectionString(connectionStringName);
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    private string GetConnectionString(string? name)
    {
        name ??= "LIMSDatabase";
        return _configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' not found.");
    }
}
