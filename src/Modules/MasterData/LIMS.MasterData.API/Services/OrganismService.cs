using FluentResults;
using FluentValidation;
using LIMS.MasterData.API.Cache;
using LIMS.MasterData.API.DAL.Repositories;
using LIMS.MasterData.API.Models.Mappings;
using LIMS.MasterData.API.Models.Organism;
using LIMS.Shared.Core.Interfaces;

namespace LIMS.MasterData.API.Services;

public class OrganismService : IOrganismService
{
    private readonly IOrganismRepository _repository;
    private readonly IValidator<CreateOrganismPayload> _createValidator;
    private readonly IValidator<UpdateOrganismPayload> _updateValidator;
    private readonly IOrganismCacheService _cache;
    private readonly IDbContext _dbContext;

    public OrganismService(
        IOrganismRepository repository,
        IValidator<CreateOrganismPayload> createValidator,
        IValidator<UpdateOrganismPayload> updateValidator,
        IOrganismCacheService cache,
        IDbContext dbContext)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _cache = cache;
        _dbContext = dbContext;
    }

    public async Task<Result<OrganismModel>> Create(CreateOrganismPayload payload)
    {
        // Validate
        var validationResult = await _createValidator.ValidateAsync(payload);
        if (!validationResult.IsValid)
        {
            return Result.Fail(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var entity = payload.ToEntity();
        var tx = await _dbContext.BeginTransactionAsync();

        try
        {
            await _repository.Add(entity, "Created organism", transaction: tx);
            await _dbContext.CommitTransactionAsync(tx);

            // Cache the new entity
            await _cache.SetAsync(entity.Id, entity);

            return Result.Ok(entity.ToModel());
        }
        catch (Exception ex)
        {
            _dbContext.RollbackTransaction();
            return Result.Fail($"Failed to create organism: {ex.Message}");
        }
    }

    public async Task<Result<OrganismModel>> Update(UpdateOrganismPayload payload)
    {
        // Validate
        var validationResult = await _updateValidator.ValidateAsync(payload);
        if (!validationResult.IsValid)
        {
            return Result.Fail(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        var entity = payload.ToEntity();
        var tx = await _dbContext.BeginTransactionAsync();

        try
        {
            await _repository.Update(entity, "Updated organism", transaction: tx);
            await _dbContext.CommitTransactionAsync(tx);

            // Invalidate cache
            await _cache.RemoveAsync(entity.Id);

            // Get updated entity
            var updated = await _repository.Get(entity.Id);
            if (updated != null)
            {
                await _cache.SetAsync(updated.Id, updated);
                return Result.Ok(updated.ToModel());
            }

            return Result.Fail("Failed to retrieve updated organism");
        }
        catch (InvalidOperationException ex)
        {
            _dbContext.RollbackTransaction();
            return Result.Fail(ex.Message); // Concurrency conflict
        }
        catch (Exception ex)
        {
            _dbContext.RollbackTransaction();
            return Result.Fail($"Failed to update organism: {ex.Message}");
        }
    }

    public async Task<Result<OrganismModel>> Get(Guid id)
    {
        // Try cache first
        var cached = await _cache.GetAsync(id);
        if (cached != null)
        {
            return Result.Ok(cached.ToModel());
        }

        // Get from database
        var entity = await _repository.Get(id);
        if (entity == null)
        {
            return Result.Fail($"Organism with id {id} not found");
        }

        // Cache it
        await _cache.SetAsync(entity.Id, entity);

        return Result.Ok(entity.ToModel());
    }

    public async Task<Result<IEnumerable<OrganismModel>>> GetAll()
    {
        var entities = await _repository.GetAll();
        var models = entities.Select(e => e.ToModel());
        return Result.Ok(models);
    }

    public async Task<Result<IEnumerable<OrganismModel>>> GetFiltered(
        Guid? typeId = null,
        Guid? characterizationId = null,
        bool? active = null)
    {
        var entities = await _repository.GetFiltered(typeId, characterizationId, active);
        var models = entities.Select(e => e.ToModel());
        return Result.Ok(models);
    }

    public async Task<Result> Activate(Guid id, string notes)
    {
        var tx = await _dbContext.BeginTransactionAsync();

        try
        {
            await _repository.Activate(id, notes, transaction: tx);
            await _dbContext.CommitTransactionAsync(tx);

            // Invalidate cache
            await _cache.RemoveAsync(id);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _dbContext.RollbackTransaction();
            return Result.Fail($"Failed to activate organism: {ex.Message}");
        }
    }

    public async Task<Result> Deactivate(Guid id, string notes)
    {
        var tx = await _dbContext.BeginTransactionAsync();

        try
        {
            await _repository.Deactivate(id, notes, transaction: tx);
            await _dbContext.CommitTransactionAsync(tx);

            // Invalidate cache
            await _cache.RemoveAsync(id);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _dbContext.RollbackTransaction();
            return Result.Fail($"Failed to deactivate organism: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<OrganismModel>>> GetHistory(Guid id)
    {
        var entities = await _repository.GetHistory(id);
        var models = entities.Select(e => e.ToModel());
        return Result.Ok(models);
    }
}
