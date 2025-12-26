using MasterDataModule.Entities;
using System.Data;

namespace MasterDataModule.DAL.Repositories;

public interface IOrganismRepository
{
    // Create
    Task Add(Organism entity, string notes, Guid? correlationId = null, IDbTransaction? tx = null);

    // Read
    Task<Organism?> Get(Guid id, IDbTransaction? tx = null);
    Task<IEnumerable<Organism>> GetAll(IDbTransaction? tx = null);
    Task<IEnumerable<Organism>> GetFiltered(Guid? typeId = null, Guid? characterizationId = null, bool? active = null, IDbTransaction? tx = null);

    // Update
    Task Update(Organism entity, string notes, Guid? correlationId = null, IDbTransaction? tx = null);

    // Activate/Deactivate
    Task Activate(Guid id, string notes, Guid? correlationId = null, IDbTransaction? tx = null);
    Task Deactivate(Guid id, string notes, Guid? correlationId = null, IDbTransaction? tx = null);

    // Existence checks
    Task<bool> Exists(Guid id, IDbTransaction? tx = null);
    Task<bool> ExistsByCombinationAsync(Guid typeId, string genus, Guid characterizationId, CancellationToken cancellationToken = default);

    // Count
    Task<int> Count(IDbTransaction? tx = null);

    // Temporal queries
    Task<IEnumerable<Organism>> GetHistory(Guid id, IDbTransaction? tx = null);
    Task<Organism?> GetAsOf(Guid id, DateTime asOf, IDbTransaction? tx = null);
}
