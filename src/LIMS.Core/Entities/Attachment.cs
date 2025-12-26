namespace LIMS.Core.Entities;

public class SampleAttachment : BaseEntity
{
    public Guid SampleId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AttachmentCategory Category { get; set; }

    // Navigation properties
    public Sample Sample { get; set; } = null!;
}

public enum AttachmentCategory
{
    ChainOfCustody = 1,
    Certificate = 2,
    Report = 3,
    Specification = 4,
    Image = 5,
    RawData = 6,
    Other = 99
}
