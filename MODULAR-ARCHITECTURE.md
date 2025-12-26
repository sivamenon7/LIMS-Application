# LIMS Modular Architecture

## Overview

This LIMS system is built using a **modular, domain-driven architecture** where each business domain is completely isolated with its own API, database schema, and UI components. Data access is handled through a **schema-driven DAL generator** that reads SQL Server metadata and generates type-safe Dapper repositories.

## Key Architectural Principles

### 1. **Domain-Driven Modularity**

Each domain module is self-contained:

```
src/Modules/
├── MasterData/
│   ├── Database/                 # SQL schema scripts
│   ├── LIMS.MasterData.API/      # Domain API
│   └── LIMS.MasterData.UI/       # UI components
├── Equipment/
│   ├── Database/
│   ├── LIMS.Equipment.API/
│   └── LIMS.Equipment.UI/
├── SampleManagement/
│   ├── Database/
│   ├── LIMS.SampleManagement.API/
│   └── LIMS.SampleManagement.UI/
└── ...
```

### 2. **Schema-Driven DAL Generation**

The DAL is **generated directly from the database schema**, not hand-written:

**Benefits:**
- ✅ Single source of truth (database schema)
- ✅ Type-safe code generation
- ✅ Automatic support for temporal tables
- ✅ Consistent patterns across all modules
- ✅ No manual repository coding
- ✅ Easy to regenerate when schema changes

### 3. **Temporal Tables for Audit Trail**

All tables use SQL Server temporal tables for:
- Complete change history
- Point-in-time queries
- Regulatory compliance (21 CFR Part 11)
- No application-level audit code needed

### 4. **Separation of Concerns**

```
┌─────────────────┐
│   UI Module     │ ← Blazor components, state management
└────────┬────────┘
         │
┌────────▼────────┐
│   API Module    │ ← Controllers, services, validation
└────────┬────────┘
         │
┌────────▼────────┐
│  Generated DAL  │ ← Auto-generated from schema
└────────┬────────┘
         │
┌────────▼────────┐
│   SQL Schema    │ ← Single source of truth
└─────────────────┘
```

## Domain Modules

### Master Data Module
**Responsibilities:**
- Customer management
- User management
- Role and permission management
- Organization hierarchy

**Tables:**
- Customers
- Users
- Roles
- Permissions
- UserRoles
- RolePermissions

### Equipment Module
**Responsibilities:**
- Instrument tracking
- Calibration management
- Maintenance scheduling
- Equipment status monitoring

**Tables:**
- Instruments
- InstrumentCalibrations
- InstrumentMaintenance
- EquipmentTypes

### Sample Management Module
**Responsibilities:**
- Sample registration
- Sample tracking
- Chain of custody
- Sample storage

**Tables:**
- Samples
- Projects
- SampleAttachments
- SampleChainOfCustody

### Testing & QC Module
**Responsibilities:**
- Test definitions
- Test execution
- Result entry and validation
- Multi-level approval workflow

**Tables:**
- Tests
- TestParameters
- SampleTests
- TestResultData
- TestApprovals

### Inventory Module
**Responsibilities:**
- Reagent tracking
- Consumable management
- Reorder management
- Lot tracking

**Tables:**
- InventoryItems
- InventoryTransactions
- InventoryLots
- ReorderAlerts

## Using the DAL Generator

### Step 1: Create Your Database Schema

Create temporal tables in SQL Server:

```sql
CREATE TABLE Customers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CustomerCode NVARCHAR(50) NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    -- ... other columns ...

    -- Audit columns
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,

    -- Temporal columns (auto-managed by SQL Server)
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.CustomersHistory));
```

### Step 2: Configure the DAL Generator

Edit `src/Tools/LIMS.DAL.Generator/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "LIMSDatabase": "Server=localhost;Database=LIMSEnterprise;..."
  },
  "OutputPath": "../../Modules/MasterData/LIMS.MasterData.API/Generated",
  "Namespace": "LIMS.MasterData.Generated",
  "Schema": "dbo"
}
```

### Step 3: Run the Generator

```bash
cd src/Tools/LIMS.DAL.Generator
dotnet run
```

**Output:**
```
LIMS DAL Generator - Schema-Driven Repository Generator
=======================================================

Connection: Server=localhost
Schema: dbo
Output: ../../Modules/MasterData/LIMS.MasterData.API/Generated
Namespace: LIMS.MasterData.Generated

Found 5 tables:
  - Customers (15 columns)
  - Users (13 columns)
  - Roles (5 columns)
  - Permissions (6 columns)
  - UserRoles (4 columns)

Generating code...
  Generated entity: Customers.cs
  Generated repository: CustomersRepository.cs
  Generated entity: Users.cs
  Generated repository: UsersRepository.cs
  ...

✓ Code generation complete!
```

### Step 4: Generated Code

**Entity (Customers.cs):**
```csharp
using LIMS.Shared.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace LIMS.MasterData.Generated.Entities;

[Table("Customers", Schema = "dbo")]
public class Customers : BaseEntity
{
    /// <summary>nvarchar(50) NOT NULL</summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>nvarchar(200) NOT NULL</summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>nvarchar(100) NOT NULL</summary>
    public string ContactName { get; set; } = string.Empty;

    // ... more properties ...
}
```

**Repository (CustomersRepository.cs):**
```csharp
using Dapper;
using LIMS.Shared.Core.Interfaces;
using LIMS.MasterData.Generated.Entities;

namespace LIMS.MasterData.Generated.Repositories;

public class CustomersRepository : IRepository<Customers>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private const string TableName = "dbo.Customers";

    public async Task<Customers?> GetByIdAsync(Guid id, ...)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Customers>(sql, new { Id = id });
    }

    // Temporal table support
    public async Task<Customers?> GetByIdAsOfAsync(Guid id, DateTime asOf, ...)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName} FOR SYSTEM_TIME AS OF @AsOf WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Customers>(sql, new { Id = id, AsOf = asOf });
    }

    public async Task<IEnumerable<Customers>> GetHistoryAsync(Guid id, ...)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName} FOR SYSTEM_TIME ALL WHERE Id = @Id ORDER BY ValidFrom DESC";
        return await connection.QueryAsync<Customers>(sql, new { Id = id });
    }

    // ... CRUD operations ...
}
```

### Step 5: Use in Services

Create domain services with business logic:

```csharp
using FluentValidation;
using LIMS.MasterData.Generated.Entities;
using LIMS.MasterData.Generated.Repositories;

namespace LIMS.MasterData.API.Services;

public class CustomerValidator : AbstractValidator<Customers>
{
    public CustomerValidator()
    {
        RuleFor(x => x.CustomerCode)
            .NotEmpty()
            .MaximLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

public class CustomerService : IService<Customers>
{
    private readonly CustomersRepository _repository;
    private readonly CustomerValidator _validator;
    private readonly ICacheService _cache;
    private readonly IEventPublisher _events;

    public async Task<Result<Customers>> CreateAsync(Customers entity)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(entity);
        if (!validationResult.IsValid)
            return Result<Customers>.ValidationFailure(validationResult.ToDictionary());

        // Business logic
        entity.CreatedBy = _currentUser.Username;
        entity.CreatedAt = DateTime.UtcNow;

        // Save
        var customer = await _repository.AddAsync(entity);

        // Publish event
        await _events.PublishAsync(new CustomerCreatedEvent { CustomerId = customer.Id });

        // Cache
        await _cache.SetAsync($"customer:{customer.Id}", customer);

        return Result<Customers>.Success(customer);
    }

    // ... more methods ...
}
```

## Service Layer Pattern

Each module follows this pattern:

```
API Module
├── Generated/                    ← Auto-generated by DAL generator
│   ├── Entities/
│   │   ├── Customers.cs
│   │   └── Users.cs
│   └── Repositories/
│       ├── CustomersRepository.cs
│       └── UsersRepository.cs
├── Services/                     ← Hand-written business logic
│   ├── CustomerService.cs
│   ├── UserService.cs
│   └── Validators/
│       ├── CustomerValidator.cs
│       └── UserValidator.cs
└── Controllers/                  ← API endpoints
    ├── CustomersController.cs
    └── UsersController.cs
```

## API Module Structure

Each domain API is a standalone ASP.NET Core Web API:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add shared services
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add domain-specific services
builder.Services.AddScoped<CustomersRepository>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<CustomerValidator>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

var app = builder.Build();
app.Run();
```

## UI Module Structure

Each domain has its own Blazor Razor Class Library:

```
LIMS.MasterData.UI/
├── Components/
│   ├── CustomerList.razor
│   ├── CustomerForm.razor
│   └── CustomerDetails.razor
├── Services/
│   └── CustomerApiClient.cs
└── wwwroot/
    └── css/
        └── masterdata.css
```

**Usage in Shell:**
```razor
@page "/customers"
@using LIMS.MasterData.UI.Components

<CustomerList />
```

## Benefits of This Architecture

### 1. **Schema is Single Source of Truth**
- Database schema drives everything
- No manual entity/repository coding
- Regenerate DAL when schema changes

### 2. **Modularity**
- Each domain can be developed independently
- Clear boundaries between modules
- Easy to add new domains

### 3. **Consistency**
- All repositories have the same structure
- Temporal table support everywhere
- Standard audit fields

### 4. **Performance**
- Dapper for high-performance data access
- Direct SQL queries (no ORM overhead)
- Efficient temporal queries

### 5. **Maintainability**
- Business logic separated from data access
- Validation centralized with FluentValidation
- Easy to understand and modify

### 6. **Compliance**
- Complete audit trail via temporal tables
- Change history without application code
- Point-in-time queries for regulatory needs

## Workflow Example

### Adding a New Feature

1. **Modify Database Schema:**
```sql
ALTER TABLE Customers
ADD CreditLimit DECIMAL(18,2) NULL;
```

2. **Regenerate DAL:**
```bash
cd src/Tools/LIMS.DAL.Generator
dotnet run
```

3. **Update Service:**
```csharp
public async Task<Result<Customers>> SetCreditLimit(Guid customerId, decimal limit)
{
    var customer = await _repository.GetByIdAsync(customerId);
    customer.CreditLimit = limit;
    customer.ModifiedBy = _currentUser.Username;

    await _repository.UpdateAsync(customer);
    return Result<Customers>.Success(customer);
}
```

4. **Add Validation:**
```csharp
RuleFor(x => x.CreditLimit)
    .GreaterThanOrEqualTo(0)
    .When(x => x.CreditLimit.HasValue);
```

5. **Done!** - Entity and repository automatically updated

## Temporal Table Queries

### Point-in-Time Query
```csharp
// Get customer as it existed on Jan 1, 2024
var customer = await _repository.GetByIdAsOfAsync(customerId, new DateTime(2024, 1, 1));
```

### Full History
```csharp
// Get all versions of a customer
var history = await _repository.GetHistoryAsync(customerId);

foreach (var version in history)
{
    Console.WriteLine($"Valid from {version.ValidFrom} to {version.ValidTo}");
    Console.WriteLine($"Company Name: {version.CompanyName}");
}
```

### Custom Temporal Queries
```csharp
// Find all customers who were active during a specific period
var sql = @"
    SELECT * FROM Customers
    FOR SYSTEM_TIME BETWEEN @StartDate AND @EndDate
    WHERE IsActive = 1";

var customers = await connection.QueryAsync<Customers>(sql, new {
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 12, 31)
});
```

## Best Practices

### 1. Database Schema
- Always use temporal tables
- Include all audit fields (BaseEntity)
- Use meaningful column names
- Add proper indexes

### 2. DAL Generation
- Regenerate after every schema change
- Never manually edit generated files
- Keep generated code in separate folder

### 3. Business Logic
- All logic in services, not repositories
- Use FluentValidation for all validation
- Publish events for important actions
- Cache frequently accessed data

### 4. API Design
- One controller per entity
- RESTful endpoints
- Consistent error handling
- OpenAPI/Swagger documentation

### 5. UI Components
- Reusable components per domain
- State management within components
- API clients for backend communication
- Proper error handling and loading states

## Running the System

### 1. Start Infrastructure
```bash
docker-compose up sqlserver redis rabbitmq -d
```

### 2. Create Database Schemas
```bash
sqlcmd -S localhost -U sa -P password -i database/create-all-schemas.sql
```

### 3. Generate DAL for Each Module
```bash
# Master Data
cd src/Tools/LIMS.DAL.Generator
dotnet run -- --module MasterData

# Equipment
dotnet run -- --module Equipment

# Repeat for all modules...
```

### 4. Run APIs (with Aspire)
```bash
cd infrastructure/LIMS.AppHost
dotnet run
```

### 5. Run UI Shell
```bash
cd src/LIMS.UI.Shell
dotnet run
```

## Conclusion

This modular, schema-driven architecture provides:

- ✅ **Consistency** - Generated code follows same patterns
- ✅ **Maintainability** - Schema is single source of truth
- ✅ **Performance** - Dapper for high-speed data access
- ✅ **Auditability** - Complete history via temporal tables
- ✅ **Modularity** - Independent domain modules
- ✅ **Scalability** - Each module can scale independently
- ✅ **Compliance** - Built-in audit trail and history

The database schema drives everything, ensuring consistency and eliminating manual repository coding while maintaining full control over SQL queries and performance.
