namespace LIMS.Core.Entities;

public class Test : BaseEntity
{
    public string TestCode { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TestCategory Category { get; set; }
    public string MethodReference { get; set; } = string.Empty;
    public decimal? StandardCost { get; set; }
    public int EstimatedDuration { get; set; } // in minutes
    public bool RequiresCalibration { get; set; }
    public bool IsActive { get; set; } = true;
    public string? InstrumentType { get; set; }
    public string? SamplePreparationInstructions { get; set; }

    // Navigation properties
    public ICollection<TestParameter> Parameters { get; set; } = new List<TestParameter>();
    public ICollection<SampleTest> SampleTests { get; set; } = new List<SampleTest>();
}

public class TestParameter : BaseEntity
{
    public Guid TestId { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public ParameterDataType DataType { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }

    // Navigation properties
    public Test Test { get; set; } = null!;
}

public enum TestCategory
{
    Chemical = 1,
    Physical = 2,
    Microbiological = 3,
    Sensory = 4,
    Molecular = 5,
    Chromatography = 6,
    Spectroscopy = 7,
    Other = 99
}

public enum ParameterDataType
{
    Numeric = 1,
    Text = 2,
    Boolean = 3,
    Date = 4,
    List = 5
}
