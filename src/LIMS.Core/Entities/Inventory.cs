namespace LIMS.Core.Entities;

public class InventoryItem : BaseEntity
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InventoryCategory Category { get; set; }
    public string? Manufacturer { get; set; }
    public string? CatalogNumber { get; set; }
    public string? LotNumber { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal ReorderLevel { get; set; }
    public decimal? MinimumStock { get; set; }
    public decimal? MaximumStock { get; set; }
    public string StorageLocation { get; set; } = string.Empty;
    public string? StorageConditions { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal? UnitCost { get; set; }

    // Navigation properties
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}

public class InventoryTransaction : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public Guid? UserId { get; set; }
    public string? Reference { get; set; }
    public string? Comments { get; set; }

    // Navigation properties
    public InventoryItem InventoryItem { get; set; } = null!;
    public User? User { get; set; }
}

public enum InventoryCategory
{
    Reagent = 1,
    Standard = 2,
    Consumable = 3,
    Glassware = 4,
    Equipment = 5,
    Chemical = 6,
    Media = 7,
    Other = 99
}

public enum TransactionType
{
    Receipt = 1,
    Issue = 2,
    Return = 3,
    Adjustment = 4,
    Transfer = 5,
    Disposal = 6
}
