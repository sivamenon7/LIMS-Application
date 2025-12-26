-- =============================================
-- Master Data Module - Database Schema
-- =============================================

USE LIMSEnterprise;
GO

-- =============================================
-- CUSTOMERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
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
        -- Audit fields
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        -- Temporal table columns
        ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
        ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
    )
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.CustomersHistory));

    CREATE INDEX IX_Customers_CustomerCode ON Customers(CustomerCode);
    CREATE INDEX IX_Customers_Email ON Customers(Email);
END
GO

-- =============================================
-- USERS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
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
        -- Audit fields
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        -- Temporal table columns
        ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
        ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
    )
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.UsersHistory));

    CREATE INDEX IX_Users_Username ON Users(Username);
    CREATE INDEX IX_Users_Email ON Users(Email);
END
GO

-- =============================================
-- ROLES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        RoleName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        IsSystemRole BIT NOT NULL DEFAULT 0,
        -- Audit fields
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        -- Temporal table columns
        ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
        ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
    )
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RolesHistory));
END
GO

-- =============================================
-- PERMISSIONS TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE Permissions (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        PermissionName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(500),
        Module NVARCHAR(100) NOT NULL,
        -- Audit fields
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedBy NVARCHAR(100) NOT NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,
        -- Temporal table columns
        ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
        ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
    )
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.PermissionsHistory));
END
GO

-- =============================================
-- USER ROLES TABLE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE UserRoles (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        -- Audit fields
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
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        FOREIGN KEY (RoleId) REFERENCES Roles(Id),
        UNIQUE (UserId, RoleId)
    )
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.UserRolesHistory));
END
GO

PRINT 'Master Data schema created successfully';
GO
