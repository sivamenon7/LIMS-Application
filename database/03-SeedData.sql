-- =============================================
-- LIMS Enterprise Seed Data Script
-- =============================================

USE LIMSEnterprise;
GO

-- =============================================
-- SEED PERMISSIONS
-- =============================================
INSERT INTO Permissions (Id, PermissionName, Description, Module, CreatedBy)
VALUES
    (NEWID(), 'Sample.Create', 'Create new samples', 'Samples', 'SYSTEM'),
    (NEWID(), 'Sample.Read', 'View samples', 'Samples', 'SYSTEM'),
    (NEWID(), 'Sample.Update', 'Update samples', 'Samples', 'SYSTEM'),
    (NEWID(), 'Sample.Delete', 'Delete samples', 'Samples', 'SYSTEM'),
    (NEWID(), 'Test.Create', 'Create new tests', 'Tests', 'SYSTEM'),
    (NEWID(), 'Test.Read', 'View tests', 'Tests', 'SYSTEM'),
    (NEWID(), 'Test.Update', 'Update tests', 'Tests', 'SYSTEM'),
    (NEWID(), 'Test.Delete', 'Delete tests', 'Tests', 'SYSTEM'),
    (NEWID(), 'Result.Enter', 'Enter test results', 'Results', 'SYSTEM'),
    (NEWID(), 'Result.Review', 'Review test results', 'Results', 'SYSTEM'),
    (NEWID(), 'Result.Approve', 'Approve test results', 'Results', 'SYSTEM'),
    (NEWID(), 'Customer.Manage', 'Manage customers', 'Customers', 'SYSTEM'),
    (NEWID(), 'User.Manage', 'Manage users', 'Administration', 'SYSTEM'),
    (NEWID(), 'Inventory.Manage', 'Manage inventory', 'Inventory', 'SYSTEM'),
    (NEWID(), 'Instrument.Manage', 'Manage instruments', 'Instruments', 'SYSTEM');
GO

-- =============================================
-- SEED ROLES
-- =============================================
DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @LabTechRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ReviewerRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @CustomerServiceRoleId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Roles (Id, RoleName, Description, IsSystemRole, CreatedBy)
VALUES
    (@AdminRoleId, 'Administrator', 'Full system access', 1, 'SYSTEM'),
    (@LabTechRoleId, 'Lab Technician', 'Perform tests and enter results', 1, 'SYSTEM'),
    (@ReviewerRoleId, 'Reviewer', 'Review and approve test results', 1, 'SYSTEM'),
    (@CustomerServiceRoleId, 'Customer Service', 'Manage samples and customers', 1, 'SYSTEM');
GO

-- =============================================
-- ASSIGN PERMISSIONS TO ROLES
-- =============================================
-- Admin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT
    (SELECT Id FROM Roles WHERE RoleName = 'Administrator'),
    Id,
    'SYSTEM'
FROM Permissions;

-- Lab Technician permissions
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT
    (SELECT Id FROM Roles WHERE RoleName = 'Lab Technician'),
    Id,
    'SYSTEM'
FROM Permissions
WHERE PermissionName IN ('Sample.Read', 'Test.Read', 'Result.Enter');

-- Reviewer permissions
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT
    (SELECT Id FROM Roles WHERE RoleName = 'Reviewer'),
    Id,
    'SYSTEM'
FROM Permissions
WHERE PermissionName IN ('Sample.Read', 'Test.Read', 'Result.Read', 'Result.Review', 'Result.Approve');

-- Customer Service permissions
INSERT INTO RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT
    (SELECT Id FROM Roles WHERE RoleName = 'Customer Service'),
    Id,
    'SYSTEM'
FROM Permissions
WHERE PermissionName IN ('Sample.Create', 'Sample.Read', 'Sample.Update', 'Customer.Manage');
GO

-- =============================================
-- SEED ADMIN USER (Password: Admin@123)
-- =============================================
INSERT INTO Users (Username, Email, FirstName, LastName, PasswordHash, IsActive, Department, JobTitle, CreatedBy)
VALUES
    ('admin', 'admin@lims.local', 'System', 'Administrator',
     'AQAAAAEAACcQAAAAEJ4Q8J3nZ4N5xJ+vKxJ4J3Z4J4J5J6J7J8J9J0J1J2J3J4J5J6J7J8J9J0',
     1, 'IT', 'System Administrator', 'SYSTEM');

-- Assign Admin role to admin user
INSERT INTO UserRoles (UserId, RoleId, CreatedBy)
SELECT
    (SELECT Id FROM Users WHERE Username = 'admin'),
    (SELECT Id FROM Roles WHERE RoleName = 'Administrator'),
    'SYSTEM';
GO

-- =============================================
-- SEED SAMPLE CUSTOMERS
-- =============================================
INSERT INTO Customers (CustomerCode, CompanyName, ContactName, Email, Phone, Address, City, State, PostalCode, Country, CreatedBy)
VALUES
    ('CUST001', 'Acme Pharmaceuticals Inc.', 'John Smith', 'john.smith@acmepharma.com', '+1-555-0101', '123 Pharma Street', 'Boston', 'MA', '02101', 'USA', 'SYSTEM'),
    ('CUST002', 'BioTech Solutions Ltd.', 'Sarah Johnson', 'sarah@biotechsol.com', '+1-555-0102', '456 Research Ave', 'San Francisco', 'CA', '94102', 'USA', 'SYSTEM'),
    ('CUST003', 'Global Chemical Corp.', 'Michael Chen', 'mchen@globalchem.com', '+1-555-0103', '789 Industrial Blvd', 'Houston', 'TX', '77001', 'USA', 'SYSTEM');
GO

-- =============================================
-- SEED TEST DEFINITIONS
-- =============================================
DECLARE @pHTestId UNIQUEIDENTIFIER = NEWID();
DECLARE @moistureTestId UNIQUEIDENTIFIER = NEWID();
DECLARE @microTestId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Tests (Id, TestCode, TestName, Description, Category, MethodReference, StandardCost, EstimatedDuration, RequiresCalibration, CreatedBy)
VALUES
    (@pHTestId, 'PH-001', 'pH Measurement', 'Measurement of pH value', 2, 'USP <791>', 25.00, 30, 1, 'SYSTEM'),
    (@moistureTestId, 'MOIST-001', 'Moisture Content', 'Karl Fischer Moisture Analysis', 2, 'USP <921>', 50.00, 60, 1, 'SYSTEM'),
    (@microTestId, 'MICRO-001', 'Microbial Limit Test', 'Total Aerobic Microbial Count', 3, 'USP <61>', 150.00, 72, 0, 'SYSTEM');

-- pH Test Parameters
INSERT INTO TestParameters (TestId, ParameterName, DisplayName, DataType, UnitOfMeasure, MinValue, MaxValue, TargetValue, DisplayOrder, CreatedBy)
VALUES
    (@pHTestId, 'pH_Value', 'pH Value', 1, 'pH', 0, 14, 7.0, 1, 'SYSTEM'),
    (@pHTestId, 'Temperature', 'Temperature', 1, '°C', 15, 30, 25, 2, 'SYSTEM');

-- Moisture Test Parameters
INSERT INTO TestParameters (TestId, ParameterName, DisplayName, DataType, UnitOfMeasure, MinValue, MaxValue, DisplayOrder, CreatedBy)
VALUES
    (@moistureTestId, 'Moisture_Percent', 'Moisture Content', 1, '%', 0, 100, 1, 'SYSTEM');

-- Microbial Test Parameters
INSERT INTO TestParameters (TestId, ParameterName, DisplayName, DataType, UnitOfMeasure, MaxValue, DisplayOrder, CreatedBy)
VALUES
    (@microTestId, 'Total_Count', 'Total Aerobic Count', 1, 'CFU/g', 1000, 1, 'SYSTEM');
GO

-- =============================================
-- SEED INSTRUMENTS
-- =============================================
INSERT INTO Instruments (InstrumentId, InstrumentName, Manufacturer, Model, SerialNumber, Status, Location, CreatedBy)
VALUES
    ('INST-001', 'pH Meter 1', 'Mettler Toledo', 'SevenCompact S220', 'SN12345678', 1, 'Lab Room A', 'SYSTEM'),
    ('INST-002', 'Karl Fischer Titrator', 'Metrohm', '899 Coulometer', 'SN87654321', 1, 'Lab Room B', 'SYSTEM'),
    ('INST-003', 'HPLC System', 'Agilent', '1260 Infinity II', 'SN11223344', 1, 'Lab Room C', 'SYSTEM');
GO

-- =============================================
-- SEED INVENTORY ITEMS
-- =============================================
INSERT INTO InventoryItems (ItemCode, ItemName, Description, Category, Manufacturer, Quantity, UnitOfMeasure, ReorderLevel, StorageLocation, CreatedBy)
VALUES
    ('REA-001', 'pH Buffer 4.0', 'pH Buffer Solution 4.0', 2, 'Fisher Scientific', 10, 'L', 2, 'Shelf A1', 'SYSTEM'),
    ('REA-002', 'pH Buffer 7.0', 'pH Buffer Solution 7.0', 2, 'Fisher Scientific', 10, 'L', 2, 'Shelf A1', 'SYSTEM'),
    ('REA-003', 'pH Buffer 10.0', 'pH Buffer Solution 10.0', 2, 'Fisher Scientific', 8, 'L', 2, 'Shelf A1', 'SYSTEM'),
    ('CON-001', 'Pipette Tips 1000µL', 'Disposable pipette tips', 3, 'Eppendorf', 5000, 'pcs', 1000, 'Shelf B2', 'SYSTEM'),
    ('CON-002', 'Sample Vials', 'Glass sample vials 20mL', 4, 'Sigma-Aldrich', 500, 'pcs', 100, 'Shelf C3', 'SYSTEM');
GO

PRINT 'Seed data inserted successfully';
GO
