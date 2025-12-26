namespace LIMS.EventBus.Events;

public record SampleCreatedEvent
{
    public Guid SampleId { get; init; }
    public string SampleNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

public record SampleStatusChangedEvent
{
    public Guid SampleId { get; init; }
    public string SampleNumber { get; init; } = string.Empty;
    public int OldStatus { get; init; }
    public int NewStatus { get; init; }
    public DateTime ChangedAt { get; init; }
    public string ChangedBy { get; init; } = string.Empty;
}

public record TestResultEnteredEvent
{
    public Guid SampleTestId { get; init; }
    public Guid SampleId { get; init; }
    public Guid TestId { get; init; }
    public int TestResult { get; init; }
    public DateTime EnteredAt { get; init; }
    public string EnteredBy { get; init; } = string.Empty;
}

public record TestResultApprovedEvent
{
    public Guid SampleTestId { get; init; }
    public Guid SampleId { get; init; }
    public string SampleNumber { get; init; } = string.Empty;
    public Guid ReviewedBy { get; init; }
    public DateTime ApprovedAt { get; init; }
}
