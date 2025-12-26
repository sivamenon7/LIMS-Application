-- =============================================
-- LIMS Enterprise Database Creation Script
-- SQL Server 2025 with Temporal Tables
-- =============================================

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'LIMSEnterprise')
BEGIN
    CREATE DATABASE LIMSEnterprise;
END
GO

USE LIMSEnterprise;
GO

-- Enable temporal table support
ALTER DATABASE LIMSEnterprise SET TEMPORAL_HISTORY_RETENTION ON;
GO

PRINT 'Database LIMSEnterprise created successfully';
GO
