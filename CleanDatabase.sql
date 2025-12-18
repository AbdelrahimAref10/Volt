-- =============================================
-- Script to clean all tables and migration history from database
-- Database: EcommerceAdmin
-- Run this script in SQL Server Management Studio (SSMS)
-- =============================================

USE [EcommerceAdmin]
GO

-- Disable foreign key constraints
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) 
    + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) 
    + ' NOCHECK CONSTRAINT ' + QUOTENAME(name) + ';'
FROM sys.foreign_keys;

EXEC sp_executesql @sql;
PRINT 'Foreign key constraints disabled'
GO

-- Drop all user tables
DECLARE @dropTables NVARCHAR(MAX) = N'';
SELECT @dropTables += N'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) 
    + '.' + QUOTENAME(name) + ';'
FROM sys.tables
WHERE name NOT LIKE 'sys%' 
  AND name NOT LIKE 'MS_%'
  AND name != '__EFMigrationsHistory';

EXEC sp_executesql @dropTables;
PRINT 'All user tables dropped'
GO

-- Drop the migration history table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[__EFMigrationsHistory];
    PRINT 'Migration history table dropped'
END
ELSE
BEGIN
    PRINT 'Migration history table does not exist'
END
GO

PRINT '========================================'
PRINT 'Database cleaned successfully!'
PRINT 'All tables and migration history removed.'
PRINT 'You can now create a fresh migration.'
PRINT '========================================'
GO

