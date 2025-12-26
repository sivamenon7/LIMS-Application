-- =============================================
-- LIMS Enterprise Table Creation Script
-- With Temporal Tables for Full Audit Trail
-- =============================================

USE LIMSEnterprise;
GO

-- =============================================
-- CUSTOMERS TABLE
-- =============================================
CREATE TABLE Customers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    CustomerCode NVARCHAR(50) NOT NULL UNIQUE,
    CompanyName NVARCHAR(200) NOT NULL,
    ContactName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(50) NOT NULL,
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(100),
    PostalCode NVARCHAR(20),
    Country NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    TaxId NVARCHAR(50),
    BillingEmail NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.CustomersHistory));
GO

CREATE INDEX IX_Customers_CustomerCode ON Customers(CustomerCode);
CREATE INDEX IX_Customers_Email ON Customers(Email);
GO

-- =============================================
-- USERS TABLE
-- =============================================
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginDate DATETIME2 NULL,
    Department NVARCHAR(100),
    JobTitle NVARCHAR(100),
    EmployeeId NVARCHAR(50),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.UsersHistory));
GO

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_EmployeeId ON Users(EmployeeId);
GO

-- =============================================
-- ROLES TABLE
-- =============================================
CREATE TABLE Roles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    IsSystemRole BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RolesHistory));
GO

-- =============================================
-- PERMISSIONS TABLE
-- =============================================
CREATE TABLE Permissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Module NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.PermissionsHistory));
GO

-- =============================================
-- USER ROLES TABLE
-- =============================================
CREATE TABLE UserRoles (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    UNIQUE (UserId, RoleId)
);
GO

-- =============================================
-- ROLE PERMISSIONS TABLE
-- =============================================
CREATE TABLE RolePermissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    RoleId UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(Id),
    UNIQUE (RoleId, PermissionId)
);
GO

-- =============================================
-- PROJECTS TABLE
-- =============================================
CREATE TABLE Projects (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProjectCode NVARCHAR(50) NOT NULL UNIQUE,
    ProjectName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 1,
    ProjectManagerId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (ProjectManagerId) REFERENCES Users(Id)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.ProjectsHistory));
GO

CREATE INDEX IX_Projects_CustomerId ON Projects(CustomerId);
CREATE INDEX IX_Projects_ProjectCode ON Projects(ProjectCode);
GO

-- =============================================
-- SAMPLES TABLE
-- =============================================
CREATE TABLE Samples (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SampleNumber NVARCHAR(50) NOT NULL UNIQUE,
    BatchNumber NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    Type INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0,
    ReceivedDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NULL,
    CompletedDate DATETIME2 NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    ProjectId UNIQUEIDENTIFIER NULL,
    StorageLocation NVARCHAR(200),
    Quantity DECIMAL(18,4) NULL,
    UnitOfMeasure NVARCHAR(50),
    SpecificationReference NVARCHAR(200),
    Priority INT NOT NULL DEFAULT 0,
    Comments NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.SamplesHistory));
GO

CREATE INDEX IX_Samples_SampleNumber ON Samples(SampleNumber);
CREATE INDEX IX_Samples_BatchNumber ON Samples(BatchNumber);
CREATE INDEX IX_Samples_CustomerId ON Samples(CustomerId);
CREATE INDEX IX_Samples_Status ON Samples(Status);
CREATE INDEX IX_Samples_ReceivedDate ON Samples(ReceivedDate);
GO

-- =============================================
-- TESTS TABLE
-- =============================================
CREATE TABLE Tests (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    TestCode NVARCHAR(50) NOT NULL UNIQUE,
    TestName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Category INT NOT NULL DEFAULT 1,
    MethodReference NVARCHAR(200),
    StandardCost DECIMAL(18,2) NULL,
    EstimatedDuration INT NOT NULL DEFAULT 0,
    RequiresCalibration BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    InstrumentType NVARCHAR(100),
    SamplePreparationInstructions NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.TestsHistory));
GO

CREATE INDEX IX_Tests_TestCode ON Tests(TestCode);
CREATE INDEX IX_Tests_Category ON Tests(Category);
GO

-- =============================================
-- TEST PARAMETERS TABLE
-- =============================================
CREATE TABLE TestParameters (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    TestId UNIQUEIDENTIFIER NOT NULL,
    ParameterName NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    DataType INT NOT NULL DEFAULT 1,
    UnitOfMeasure NVARCHAR(50),
    MinValue DECIMAL(18,6) NULL,
    MaxValue DECIMAL(18,6) NULL,
    TargetValue DECIMAL(18,6) NULL,
    AcceptanceCriteria NVARCHAR(500),
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsRequired BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
    FOREIGN KEY (TestId) REFERENCES Tests(Id)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.TestParametersHistory));
GO

CREATE INDEX IX_TestParameters_TestId ON TestParameters(TestId);
GO

-- =============================================
-- SAMPLE TESTS TABLE
-- =============================================
CREATE TABLE SampleTests (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SampleId UNIQUEIDENTIFIER NOT NULL,
    TestId UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    ScheduledDate DATETIME2 NULL,
    StartedDate DATETIME2 NULL,
    CompletedDate DATETIME2 NULL,
    AssignedToUserId UNIQUEIDENTIFIER NULL,
    ReviewedByUserId UNIQUEIDENTIFIER NULL,
    ReviewedDate DATETIME2 NULL,
    InstrumentId NVARCHAR(50),
    Result INT NULL,
    Comments NVARCHAR(MAX),
    FailureReason NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
    FOREIGN KEY (SampleId) REFERENCES Samples(Id),
    FOREIGN KEY (TestId) REFERENCES Tests(Id),
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.SampleTestsHistory));
GO

CREATE INDEX IX_SampleTests_SampleId ON SampleTests(SampleId);
CREATE INDEX IX_SampleTests_TestId ON SampleTests(TestId);
CREATE INDEX IX_SampleTests_Status ON SampleTests(Status);
GO

-- =============================================
-- TEST RESULT DATA TABLE
-- =============================================
CREATE TABLE TestResultData (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SampleTestId UNIQUEIDENTIFIER NOT NULL,
    TestParameterId UNIQUEIDENTIFIER NOT NULL,
    ParameterName NVARCHAR(100) NOT NULL,
    StringValue NVARCHAR(MAX) NULL,
    NumericValue DECIMAL(18,6) NULL,
    BooleanValue BIT NULL,
    DateValue DATETIME2 NULL,
    IsOutOfSpecification BIT NOT NULL DEFAULT 0,
    Remarks NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),
    FOREIGN KEY (SampleTestId) REFERENCES SampleTests(Id),
    FOREIGN KEY (TestParameterId) REFERENCES TestParameters(Id)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.TestResultDataHistory));
GO

CREATE INDEX IX_TestResultData_SampleTestId ON TestResultData(SampleTestId);
GO

-- =============================================
-- INSTRUMENTS TABLE
-- =============================================
CREATE TABLE Instruments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InstrumentId NVARCHAR(50) NOT NULL UNIQUE,
    InstrumentName NVARCHAR(200) NOT NULL,
    Manufacturer NVARCHAR(200),
    Model NVARCHAR(100),
    SerialNumber NVARCHAR(100),
    Status INT NOT NULL DEFAULT 1,
    LastCalibrationDate DATETIME2 NULL,
    NextCalibrationDate DATETIME2 NULL,
    LastMaintenanceDate DATETIME2 NULL,
    NextMaintenanceDate DATETIME2 NULL,
    Location NVARCHAR(200),
    Comments NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.InstrumentsHistory));
GO

CREATE INDEX IX_Instruments_InstrumentId ON Instruments(InstrumentId);
GO

-- =============================================
-- INSTRUMENT CALIBRATIONS TABLE
-- =============================================
CREATE TABLE InstrumentCalibrations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    CalibrationDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    PerformedByUserId UNIQUEIDENTIFIER NOT NULL,
    Result INT NOT NULL DEFAULT 1,
    StandardReference NVARCHAR(200),
    CertificateNumber NVARCHAR(100),
    Comments NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    FOREIGN KEY (InstrumentId) REFERENCES Instruments(Id),
    FOREIGN KEY (PerformedByUserId) REFERENCES Users(Id)
);
GO

-- =============================================
-- INSTRUMENT MAINTENANCE TABLE
-- =============================================
CREATE TABLE InstrumentMaintenance (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    MaintenanceDate DATETIME2 NOT NULL,
    Type INT NOT NULL DEFAULT 1,
    PerformedByUserId UNIQUEIDENTIFIER NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    PartsReplaced NVARCHAR(MAX),
    Cost DECIMAL(18,2) NULL,
    Comments NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    FOREIGN KEY (InstrumentId) REFERENCES Instruments(Id),
    FOREIGN KEY (PerformedByUserId) REFERENCES Users(Id)
);
GO

-- =============================================
-- INVENTORY ITEMS TABLE
-- =============================================
CREATE TABLE InventoryItems (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ItemCode NVARCHAR(50) NOT NULL UNIQUE,
    ItemName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Category INT NOT NULL DEFAULT 1,
    Manufacturer NVARCHAR(200),
    CatalogNumber NVARCHAR(100),
    LotNumber NVARCHAR(100),
    Quantity DECIMAL(18,4) NOT NULL DEFAULT 0,
    UnitOfMeasure NVARCHAR(50) NOT NULL,
    ReorderLevel DECIMAL(18,4) NOT NULL DEFAULT 0,
    MinimumStock DECIMAL(18,4) NULL,
    MaximumStock DECIMAL(18,4) NULL,
    StorageLocation NVARCHAR(200),
    StorageConditions NVARCHAR(500),
    ExpirationDate DATETIME2 NULL,
    UnitCost DECIMAL(18,2) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.InventoryItemsHistory));
GO

CREATE INDEX IX_InventoryItems_ItemCode ON InventoryItems(ItemCode);
GO

-- =============================================
-- INVENTORY TRANSACTIONS TABLE
-- =============================================
CREATE TABLE InventoryTransactions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    InventoryItemId UNIQUEIDENTIFIER NOT NULL,
    Type INT NOT NULL DEFAULT 1,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(50) NOT NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UserId UNIQUEIDENTIFIER NULL,
    Reference NVARCHAR(200),
    Comments NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    FOREIGN KEY (InventoryItemId) REFERENCES InventoryItems(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

CREATE INDEX IX_InventoryTransactions_InventoryItemId ON InventoryTransactions(InventoryItemId);
CREATE INDEX IX_InventoryTransactions_TransactionDate ON InventoryTransactions(TransactionDate);
GO

-- =============================================
-- SAMPLE ATTACHMENTS TABLE
-- =============================================
CREATE TABLE SampleAttachments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    SampleId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FileType NVARCHAR(50) NOT NULL,
    FileSize BIGINT NOT NULL,
    StoragePath NVARCHAR(1000) NOT NULL,
    Description NVARCHAR(MAX),
    Category INT NOT NULL DEFAULT 99,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL,
    FOREIGN KEY (SampleId) REFERENCES Samples(Id)
);
GO

CREATE INDEX IX_SampleAttachments_SampleId ON SampleAttachments(SampleId);
GO

PRINT 'All tables created successfully with temporal versioning enabled';
GO
