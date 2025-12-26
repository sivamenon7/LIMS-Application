using FluentResults;
using LIMS.MasterData.API.Models.Organism;

namespace LIMS.MasterData.API.Services;

public interface IOrganismService
{
    Task<Result<OrganismModel>> Create(CreateOrganismPayload payload);
    Task<Result<OrganismModel>> Update(UpdateOrganismPayload payload);
    Task<Result<OrganismModel>> Get(Guid id);
    Task<Result<IEnumerable<OrganismModel>>> GetAll();
    Task<Result<IEnumerable<OrganismModel>>> GetFiltered(Guid? typeId = null, Guid? characterizationId = null, bool? active = null);
    Task<Result> Activate(Guid id, string notes);
    Task<Result> Deactivate(Guid id, string notes);
    Task<Result<IEnumerable<OrganismModel>>> GetHistory(Guid id);
}
