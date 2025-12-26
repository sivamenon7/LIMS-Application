using LIMS.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace LIMS.Database.Common.Context;

public class DbContext : IDbContext
{
    private readonly IConfiguration _configuration;
    private IDbConnection? _connection;
    private IDbTransaction? _currentTransaction;

    public DbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IDbConnection> GetConnectionAsync()
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            var connectionString = _configuration.GetConnectionString("LIMSDatabase")
                ?? throw new InvalidOperationException("Connection string 'LIMSDatabase' not found");

            _connection = new SqlConnection(connectionString);
            await ((SqlConnection)_connection).OpenAsync();
        }

        return _connection;
    }

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        var connection = await GetConnectionAsync();
        _currentTransaction = connection.BeginTransaction();
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbTransaction transaction)
    {
        try
        {
            transaction.Commit();
        }
        finally
        {
            transaction.Dispose();
            _currentTransaction = null;
        }
        await Task.CompletedTask;
    }

    public void RollbackTransaction()
    {
        _currentTransaction?.Rollback();
        _currentTransaction?.Dispose();
        _currentTransaction = null;
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _connection?.Dispose();
    }
}
