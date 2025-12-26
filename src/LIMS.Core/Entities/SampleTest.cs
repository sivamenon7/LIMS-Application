namespace LIMS.Core.Entities;

public class SampleTest : BaseEntity
{
    public Guid SampleId { get; set; }
    public Guid TestId { get; set; }
    public SampleTestStatus Status { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? InstrumentId { get; set; }
    public TestResult? Result { get; set; }
    public string? Comments { get; set; }
    public string? FailureReason { get; set; }

    // Navigation properties
    public Sample Sample { get; set; } = null!;
    public Test Test { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public User? ReviewedByUser { get; set; }
    public ICollection<TestResultData> ResultData { get; set; } = new List<TestResultData>();
}

public class TestResultData : BaseEntity
{
    public Guid SampleTestId { get; set; }
    public Guid TestParameterId { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string? StringValue { get; set; }
    public decimal? NumericValue { get; set; }
    public bool? BooleanValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool IsOutOfSpecification { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public SampleTest SampleTest { get; set; } = null!;
    public TestParameter TestParameter { get; set; } = null!;
}

public enum SampleTestStatus
{
    Pending = 0,
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    UnderReview = 4,
    Approved = 5,
    Rejected = 6,
    OnHold = 7,
    Cancelled = 8
}

public enum TestResult
{
    NotTested = 0,
    Pass = 1,
    Fail = 2,
    Inconclusive = 3,
    OutOfSpecification = 4
}
