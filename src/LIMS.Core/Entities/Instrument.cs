namespace LIMS.Core.Entities;

public class Instrument : BaseEntity
{
    public string InstrumentId { get; set; } = string.Empty;
    public string InstrumentName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public InstrumentStatus Status { get; set; }
    public DateTime? LastCalibrationDate { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Comments { get; set; }

    // Navigation properties
    public ICollection<InstrumentCalibration> Calibrations { get; set; } = new List<InstrumentCalibration>();
    public ICollection<InstrumentMaintenance> MaintenanceRecords { get; set; } = new List<InstrumentMaintenance>();
}

public class InstrumentCalibration : BaseEntity
{
    public Guid InstrumentId { get; set; }
    public DateTime CalibrationDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid PerformedByUserId { get; set; }
    public CalibrationResult Result { get; set; }
    public string? StandardReference { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public Instrument Instrument { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}

public class InstrumentMaintenance : BaseEntity
{
    public Guid InstrumentId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public MaintenanceType Type { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PartsReplaced { get; set; }
    public decimal? Cost { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public Instrument Instrument { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}

public enum InstrumentStatus
{
    Available = 1,
    InUse = 2,
    UnderMaintenance = 3,
    CalibrationDue = 4,
    OutOfService = 5
}

public enum CalibrationResult
{
    Pass = 1,
    Fail = 2,
    Conditional = 3
}

public enum MaintenanceType
{
    Preventive = 1,
    Corrective = 2,
    Emergency = 3,
    Upgrade = 4
}
