namespace LIMS.Core.Entities;

public class Sample : BaseEntity
{
    public string SampleNumber { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SampleType Type { get; set; }
    public SampleStatus Status { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? ProjectId { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string? SpecificationReference { get; set; }
    public int Priority { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Project? Project { get; set; }
    public ICollection<SampleTest> Tests { get; set; } = new List<SampleTest>();
    public ICollection<SampleAttachment> Attachments { get; set; } = new List<SampleAttachment>();
}

public enum SampleType
{
    Unknown = 0,
    RawMaterial = 1,
    InProcess = 2,
    FinishedProduct = 3,
    Reference = 4,
    Environmental = 5,
    Calibration = 6,
    QualityControl = 7
}

public enum SampleStatus
{
    Registered = 0,
    Received = 1,
    InProgress = 2,
    Testing = 3,
    UnderReview = 4,
    Approved = 5,
    Rejected = 6,
    Completed = 7,
    Archived = 8
}
