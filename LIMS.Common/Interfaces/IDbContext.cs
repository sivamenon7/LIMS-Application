using System.Data;

namespace LIMS.Common.Interfaces;

/// <summary>
/// Database context for transaction management
/// </summary>
public interface IDbContext
{
    Task<IDbConnection> GetConnectionAsync();
    Task<IDbTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync(IDbTransaction transaction);
    void RollbackTransaction();
}

/// <summary>
/// User context for tracking who made changes
/// </summary>
public interface IUserContext
{
    string Username { get; }
    Guid UserId { get; }
    string ChangedData(string notes, Guid? correlationId = null);
}
