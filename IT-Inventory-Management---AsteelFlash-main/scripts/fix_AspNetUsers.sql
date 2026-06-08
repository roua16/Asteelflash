/*
Safe script to inspect and add missing columns to dbo.AspNetUsers
Run this from SSMS / Azure Data Studio while connected to the `ITStockM` database.
Backup the database before running.

This script:
1) Shows current column types for dbo.AspNetUsers
2) Adds missing columns used by the AppUser model with safe defaults
3) Adds nullable EmployeeId (int) and creation/metadata columns

NOTE: This does NOT change the primary key type. If your `Id` column is not INT,
      converting it is a destructive operation and requires a migration plan.
*/

PRINT '1) Current AspNetUsers columns and types';
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;

-- Add textual profile columns if they do not exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'FullName')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD FullName nvarchar(256) NULL;
    PRINT 'Added column FullName';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Post')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Post nvarchar(256) NULL;
    PRINT 'Added column Post';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Service')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Service nvarchar(256) NULL;
    PRINT 'Added column Service';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'Role')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD Role nvarchar(256) NULL;
    PRINT 'Added column Role';
END

-- EmployeeId as nullable int
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'EmployeeId')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD EmployeeId int NULL;
    PRINT 'Added column EmployeeId';
END

-- CreatedAt, UpdatedAt, IsDeleted
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'CreatedAt')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_AspNetUsers_CreatedAt DEFAULT (SYSUTCDATETIME());
    PRINT 'Added column CreatedAt with default SYSUTCDATETIME()';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD UpdatedAt datetime2 NULL;
    PRINT 'Added column UpdatedAt';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE dbo.AspNetUsers ADD IsDeleted bit NOT NULL CONSTRAINT DF_AspNetUsers_IsDeleted DEFAULT (0);
    PRINT 'Added column IsDeleted with default 0';
END

PRINT '2) Final AspNetUsers columns and types';
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;

PRINT '3) Sample rows (top 20)';
SELECT TOP 20 Id, UserName, Email, FullName, EmployeeId, CreatedAt, UpdatedAt, IsDeleted
FROM dbo.AspNetUsers;

/* If you see an error about primary key or Id column type, do NOT proceed.
   Report the result of the first SELECT (column list) here so I can adjust.
*/
