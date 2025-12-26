using LIMS.MasterData.API.Entities;
using LIMS.MasterData.API.Models.Organism;

namespace LIMS.MasterData.API.Models.Mappings;

public static partial class OrganismMappingExtensions
{
    public static Entities.Organism ToEntity(this CreateOrganismPayload payload) => new()
    {
        Id = payload.Id,
        TypeId = payload.TypeId,
        Genus = payload.Genus,
        CharacterizationId = payload.CharacterizationId,
        Species = payload.Species,
        Description = payload.Description,
        SeverityType = payload.SeverityType,
        PictureId = payload.PictureId,
        SporeForming = payload.SporeForming,
        Active = payload.Active
    };

    public static Entities.Organism ToEntity(this UpdateOrganismPayload payload) => new()
    {
        Id = payload.Id,
        TypeId = payload.TypeId,
        Genus = payload.Genus,
        CharacterizationId = payload.CharacterizationId,
        Species = payload.Species,
        Description = payload.Description,
        SeverityType = payload.SeverityType,
        PictureId = payload.PictureId,
        SporeForming = payload.SporeForming,
        Active = payload.Active,
        RowVersion = payload.RowVersion
    };

    public static OrganismModel ToModel(this Entities.Organism entity) => new()
    {
        Id = entity.Id,
        TypeId = entity.TypeId,
        Genus = entity.Genus,
        Species = entity.Species,
        Description = entity.Description,
        CharacterizationId = entity.CharacterizationId,
        SeverityType = entity.SeverityType,
        PictureId = entity.PictureId,
        Active = entity.Active,
        RowVersion = entity.RowVersion,
        SporeForming = entity.SporeForming
    };
}
