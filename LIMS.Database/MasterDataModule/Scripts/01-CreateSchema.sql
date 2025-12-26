-- =============================================
-- Master Data Module - Database Schema
-- =============================================

USE LIMSEnterprise;
GO

-- Create audit schema if not exists
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'audit')
BEGIN
    EXEC('CREATE SCHEMA [audit]');
    PRINT 'Created audit schema';
END
GO

PRINT 'Master Data schema setup complete';
GO
