using LIMS.Common.Interfaces;
using LIMS.Common.Enums;
using UUIDNext;

namespace MasterDataModule.Entities;

public partial class Organism : IIdEntity, IIdAndRowVersionEntity, IAuditDataEntity, IEquatable<Organism>
{
    public Organism()
    {
        Id = Uuid.NewDatabaseFriendly(Database.SqlServer); // Version 7 UUID
        SporeForming = false;
        Initial = true;
        Active = true;
        ChangedData = string.Empty;
    }

    public Guid Id { get; set; }
    public Guid? TypeId { get; set; }
    public string? Genus { get; set; }
    public string? Species { get; set; }
    public string? Description { get; set; }
    public Guid? CharacterizationId { get; set; }
    public SeverityType? SeverityType { get; set; }
    public Guid? PictureId { get; set; }
    public bool SporeForming { get; set; }
    public bool Initial { get; set; }
    public bool Active { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public string ChangedData { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public bool Equals(Organism? other, bool includeAuditData = false)
        => other != null &&
           Id == other.Id &&
           TypeId == other.TypeId &&
           Genus == other.Genus &&
           Species == other.Species &&
           Description == other.Description &&
           CharacterizationId == other.CharacterizationId &&
           SeverityType == other.SeverityType &&
           PictureId == other.PictureId &&
           SporeForming == other.SporeForming &&
           Active == other.Active;
}
