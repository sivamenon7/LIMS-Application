using LIMS.Common.Enums;

namespace MasterDataModule.Models.Organism;

public class OrganismModel
{
    public Guid Id { get; set; }
    public Guid? TypeId { get; set; }
    public string? Genus { get; set; }
    public string? Species { get; set; }
    public string? Description { get; set; }
    public Guid? CharacterizationId { get; set; }
    public SeverityType? SeverityType { get; set; }
    public Guid? PictureId { get; set; }
    public bool SporeForming { get; set; }
    public bool Active { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
