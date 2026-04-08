-- ============================================
-- Property Leasing System - Table Creation Script
-- Database: PropertyLeasingDB (Azure SQL)
-- Project: IT8118 Advanced Programming - Brief B
-- ============================================

-- Note: ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.)
-- are created automatically by EF Core migrations.
-- This script documents the application-specific tables.

-- ============================================
-- Properties Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Properties')
BEGIN
    CREATE TABLE [Properties] (
        [PropertyId]    INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Address]       NVARCHAR(200)   NOT NULL,
        [City]          NVARCHAR(100)   NOT NULL,
        [PropertyType]  INT             NOT NULL DEFAULT 0,   -- 0=Apartment, 1=Villa, 2=Shop, 3=Office
        [Bedrooms]      INT             NOT NULL DEFAULT 0,
        [MonthlyRent]   DECIMAL(10,2)   NOT NULL,
        [Status]        INT             NOT NULL DEFAULT 0,   -- 0=Available, 1=Leased, 2=UnderMaintenance
        [Description]   NVARCHAR(500)   NULL
    );
END
GO

-- ============================================
-- Tenants Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE [Tenants] (
        [TenantId]      INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FullName]      NVARCHAR(100)   NOT NULL,
        [Email]         NVARCHAR(450)   NOT NULL,
        [Phone]         NVARCHAR(450)   NOT NULL,
        [NationalId]    NVARCHAR(20)    NOT NULL,
        [UserId]        NVARCHAR(450)   NULL   -- FK to AspNetUsers.Id (optional)
    );
END
GO

-- ============================================
-- Leases Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Leases')
BEGIN
    CREATE TABLE [Leases] (
        [LeaseId]           INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]        INT             NOT NULL,
        [TenantId]          INT             NOT NULL,
        [StartDate]         DATETIME2       NOT NULL,
        [EndDate]           DATETIME2       NOT NULL,
        [MonthlyRent]       DECIMAL(10,2)   NOT NULL,
        [Status]            INT             NOT NULL DEFAULT 0,  -- 0=Application, 1=Screening, 2=Approved, 3=Rejected, 4=Active, 5=Renewal, 6=Expired, 7=Terminated
        [ApplicationDate]   DATETIME2       NOT NULL DEFAULT GETDATE(),
        [ApplicationNotes]  NVARCHAR(500)   NULL,
        [ScreeningNotes]    NVARCHAR(500)   NULL,
        [ApprovalDate]      DATETIME2       NULL,
        CONSTRAINT [FK_Leases_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([PropertyId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Leases_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [Tenants]([TenantId]) ON DELETE CASCADE
    );
END
GO

-- ============================================
-- MaintenanceRequests Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRequests')
BEGIN
    CREATE TABLE [MaintenanceRequests] (
        [RequestId]         INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]        INT             NOT NULL,
        [TenantId]          INT             NOT NULL,
        [Title]             NVARCHAR(100)   NOT NULL,
        [Description]       NVARCHAR(500)   NOT NULL,
        [Category]          INT             NOT NULL DEFAULT 0,  -- 0=Plumbing, 1=Electrical, 2=HVAC, 3=General
        [Priority]          INT             NOT NULL DEFAULT 1,  -- 0=Low, 1=Medium, 2=High, 3=Urgent
        [Status]            INT             NOT NULL DEFAULT 0,  -- 0=Submitted, 1=Assigned, 2=InProgress, 3=Resolved, 4=Closed
        [AssignedStaffId]   NVARCHAR(450)   NULL,                -- FK to AspNetUsers.Id
        [StaffNotes]        NVARCHAR(500)   NULL,
        [DateSubmitted]     DATETIME2       NOT NULL DEFAULT GETDATE(),
        [DateAssigned]      DATETIME2       NULL,
        [DateResolved]      DATETIME2       NULL,
        CONSTRAINT [FK_Maintenance_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([PropertyId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Maintenance_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [Tenants]([TenantId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Maintenance_Staff] FOREIGN KEY ([AssignedStaffId]) REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL
    );
END
GO

-- ============================================
-- Payments Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE [Payments] (
        [PaymentId]     INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [LeaseId]       INT             NOT NULL,
        [Amount]        DECIMAL(10,2)   NOT NULL,
        [DueDate]       DATETIME2       NOT NULL,
        [PaymentDate]   DATETIME2       NULL,
        [PaymentType]   INT             NOT NULL DEFAULT 0,  -- 0=Rent, 1=Deposit, 2=Fine
        [Status]        INT             NOT NULL DEFAULT 0,  -- 0=Pending, 1=Completed, 2=Overdue
        CONSTRAINT [FK_Payments_Leases] FOREIGN KEY ([LeaseId]) REFERENCES [Leases]([LeaseId]) ON DELETE CASCADE
    );
END
GO

-- ============================================
-- Notifications Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationId]    INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]            NVARCHAR(450)   NOT NULL,
        [Title]             NVARCHAR(200)   NOT NULL,
        [Message]           NVARCHAR(500)   NOT NULL,
        [Type]              INT             NOT NULL DEFAULT 0,  -- 0=LeaseUpdate, 1=MaintenanceUpdate, 2=PaymentReminder, 3=General
        [IsRead]            BIT             NOT NULL DEFAULT 0,
        [CreatedAt]         DATETIME2       NOT NULL DEFAULT GETDATE(),
        [LinkUrl]           NVARCHAR(200)   NULL,
        CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END
GO
