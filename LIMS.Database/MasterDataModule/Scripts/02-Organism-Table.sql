-- =============================================
-- Organism Table with Temporal Support
-- =============================================

USE LIMSEnterprise;
GO

-- Create Organism table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Organism')
BEGIN
    CREATE TABLE [dbo].[Organism] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT (newsequentialid()),
        [TypeId] UNIQUEIDENTIFIER NULL,
        [Genus] NVARCHAR(255) NULL,
        [Species] NVARCHAR(255) NULL,
        [Description] NVARCHAR(255) NULL,
        [CharacterizationId] UNIQUEIDENTIFIER NULL,
        [SeverityType] NVARCHAR(50) NULL,
        [PictureId] UNIQUEIDENTIFIER NULL,
        [SporeForming] BIT NOT NULL DEFAULT (0),
        [Initial] BIT NOT NULL DEFAULT (1),
        [Active] BIT NOT NULL DEFAULT (1),
        [RowVersion] ROWVERSION NOT NULL,
        [ChangedData] NVARCHAR(MAX) NOT NULL,
        [FromDate] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
        [ToDate] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
        PERIOD FOR SYSTEM_TIME ([FromDate], [ToDate]),
        CONSTRAINT [PK_Organism] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Organism] UNIQUE ([TypeId], [Genus], [CharacterizationId], [Species])
    )
    WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [audit].[OrganismHistory]));

    PRINT 'Organism table created successfully';
END
ELSE
BEGIN
    PRINT 'Organism table already exists';
END
GO

-- Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Organism_TypeId' AND object_id = OBJECT_ID('dbo.Organism'))
BEGIN
    CREATE INDEX IX_Organism_TypeId ON dbo.Organism(TypeId) WHERE TypeId IS NOT NULL;
    PRINT 'Created index IX_Organism_TypeId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Organism_CharacterizationId' AND object_id = OBJECT_ID('dbo.Organism'))
BEGIN
    CREATE INDEX IX_Organism_CharacterizationId ON dbo.Organism(CharacterizationId) WHERE CharacterizationId IS NOT NULL;
    PRINT 'Created index IX_Organism_CharacterizationId';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Organism_Active' AND object_id = OBJECT_ID('dbo.Organism'))
BEGIN
    CREATE INDEX IX_Organism_Active ON dbo.Organism(Active);
    PRINT 'Created index IX_Organism_Active';
END
GO

PRINT 'Organism schema complete';
GO
