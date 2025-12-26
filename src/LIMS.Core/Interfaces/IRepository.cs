using LIMS.Core.Entities;

namespace LIMS.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(string whereClause, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? whereClause = null, object? parameters = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<Sample> Samples { get; }
    IRepository<Test> Tests { get; }
    IRepository<SampleTest> SampleTests { get; }
    IRepository<TestResultData> TestResultData { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Project> Projects { get; }
    IRepository<User> Users { get; }
    IRepository<Instrument> Instruments { get; }
    IRepository<InventoryItem> InventoryItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
