using Dapper;
using LIMS.MasterData.API.DAL.Statements;
using LIMS.MasterData.API.Entities;
using LIMS.Shared.Core.Interfaces;
using System.Data;

namespace LIMS.MasterData.API.DAL.Repositories;

/// <summary>
/// Generated repository for Organism entity
/// DO NOT MANUALLY EDIT THIS FILE - it is auto-generated
/// </summary>
public partial class OrganismRepository : IOrganismRepository
{
    private readonly IDbContext _dbContext;
    private readonly IUserContext _userContext;
    private readonly OrganismStatements _statements;

    public OrganismRepository(IDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
        _statements = new OrganismStatements();
    }

    #region Create

    public async Task Add(Organism entity, string notes, Guid? correlationId = null, IDbTransaction? tx = null)
    {
        entity.ChangedData = _userContext.ChangedData(notes, correlationId);

        var connection = await _dbContext.GetConnectionAsync();
        await connection.ExecuteAsync(_statements.Insert, entity, tx);
    }

    #endregion

    #region Read

    public async Task<Organism?> Get(Guid id, IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<Organism>(
            _statements.GetById,
            new { Id = id },
            tx);
    }

    public async Task<IEnumerable<Organism>> GetAll(IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();
        return await connection.QueryAsync<Organism>(_statements.GetAll, transaction: tx);
    }

    public async Task<IEnumerable<Organism>> GetFiltered(
        Guid? typeId = null,
        Guid? characterizationId = null,
        bool? active = null,
        IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();

        var sql = "SELECT * FROM dbo.Organism WHERE 1=1";
        var parameters = new DynamicParameters();

        if (typeId.HasValue)
        {
            sql += " AND TypeId = @TypeId";
            parameters.Add("TypeId", typeId.Value);
        }

        if (characterizationId.HasValue)
        {
            sql += " AND CharacterizationId = @CharacterizationId";
            parameters.Add("CharacterizationId", characterizationId.Value);
        }

        if (active.HasValue)
        {
            sql += " AND Active = @Active";
            parameters.Add("Active", active.Value);
        }

        sql += " ORDER BY Genus, Species";

        return await connection.QueryAsync<Organism>(sql, parameters, tx);
    }

    #endregion

    #region Update

    public async Task Update(Organism entity, string notes, Guid? correlationId = null, IDbTransaction? tx = null)
    {
        entity.ChangedData = _userContext.ChangedData(notes, correlationId);

        var connection = await _dbContext.GetConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(_statements.Update, entity, tx);

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException($"Organism with Id {entity.Id} not found or has been modified by another user.");
        }
    }

    #endregion

    #region Activate/Deactivate

    public async Task Activate(Guid id, string notes, Guid? correlationId = null, IDbTransaction? tx = null)
    {
        var changedData = _userContext.ChangedData(notes, correlationId);

        var connection = await _dbContext.GetConnectionAsync();
        await connection.ExecuteAsync(
            _statements.Activate,
            new { Id = id, ChangedData = changedData },
            tx);
    }

    public async Task Deactivate(Guid id, string notes, Guid? correlationId = null, IDbTransaction? tx = null)
    {
        var changedData = _userContext.ChangedData(notes, correlationId);

        var connection = await _dbContext.GetConnectionAsync();
        await connection.ExecuteAsync(
            _statements.Deactivate,
            new { Id = id, ChangedData = changedData },
            tx);
    }

    #endregion

    #region Existence & Count

    public async Task<bool> Exists(Guid id, IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(
            _statements.Exists,
            new { Id = id },
            tx);
        return count > 0;
    }

    public async Task<bool> ExistsByCombinationAsync(
        Guid typeId,
        string genus,
        Guid characterizationId,
        CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetConnectionAsync();
        var count = await connection.ExecuteScalarAsync<int>(
            _statements.ExistsByCombination,
            new { TypeId = typeId, Genus = genus, CharacterizationId = characterizationId });
        return count > 0;
    }

    public async Task<int> Count(IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(_statements.Count, transaction: tx);
    }

    #endregion

    #region Temporal Queries

    public async Task<IEnumerable<Organism>> GetHistory(Guid id, IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();
        return await connection.QueryAsync<Organism>(
            _statements.GetHistory,
            new { Id = id },
            tx);
    }

    public async Task<Organism?> GetAsOf(Guid id, DateTime asOf, IDbTransaction? tx = null)
    {
        var connection = await _dbContext.GetConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<Organism>(
            _statements.GetAsOf,
            new { Id = id, AsOf = asOf },
            tx);
    }

    #endregion
}
