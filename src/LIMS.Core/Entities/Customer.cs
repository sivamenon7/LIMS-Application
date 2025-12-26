namespace LIMS.Core.Entities;

public class Customer : BaseEntity
{
    public string CustomerCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? TaxId { get; set; }
    public string? BillingEmail { get; set; }

    // Navigation properties
    public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class Project : BaseEntity
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public Guid? ProjectManagerId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public User? ProjectManager { get; set; }
    public ICollection<Sample> Samples { get; set; } = new List<Sample>();
}

public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}
