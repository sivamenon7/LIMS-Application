using LIMS.Core.Entities;
using LIMS.Core.Interfaces;
using System.Data;

namespace LIMS.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    private IRepository<Sample>? _samples;
    private IRepository<Test>? _tests;
    private IRepository<SampleTest>? _sampleTests;
    private IRepository<TestResultData>? _testResultData;
    private IRepository<Customer>? _customers;
    private IRepository<Project>? _projects;
    private IRepository<User>? _users;
    private IRepository<Instrument>? _instruments;
    private IRepository<InventoryItem>? _inventoryItems;

    public UnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IRepository<Sample> Samples =>
        _samples ??= new DapperRepository<Sample>(_connectionFactory);

    public IRepository<Test> Tests =>
        _tests ??= new DapperRepository<Test>(_connectionFactory);

    public IRepository<SampleTest> SampleTests =>
        _sampleTests ??= new DapperRepository<SampleTest>(_connectionFactory);

    public IRepository<TestResultData> TestResultData =>
        _testResultData ??= new DapperRepository<TestResultData>(_connectionFactory);

    public IRepository<Customer> Customers =>
        _customers ??= new DapperRepository<Customer>(_connectionFactory);

    public IRepository<Project> Projects =>
        _projects ??= new DapperRepository<Project>(_connectionFactory);

    public IRepository<User> Users =>
        _users ??= new DapperRepository<User>(_connectionFactory);

    public IRepository<Instrument> Instruments =>
        _instruments ??= new DapperRepository<Instrument>(_connectionFactory);

    public IRepository<InventoryItem> InventoryItems =>
        _inventoryItems ??= new DapperRepository<InventoryItem>(_connectionFactory);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dapper doesn't track changes like EF Core, so this is a no-op
        // Changes are persisted immediately in each repository method
        return await Task.FromResult(0);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection == null)
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
        }
        _transaction = _connection.BeginTransaction();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
            await Task.CompletedTask;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}
