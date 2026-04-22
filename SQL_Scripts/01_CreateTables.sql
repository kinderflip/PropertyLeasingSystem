-- ============================================
-- Property Leasing System - Table Creation Script
-- Database: PropertyLeasingDB (Azure SQL / LocalDB)
-- Project: IT8118 Advanced Programming - Brief B
-- Updated: 2026-04-21  (adds Units + standalone-vs-multi-unit support)
-- ============================================

-- Note: ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.)
-- are created automatically by EF Core migrations.
-- This script documents the application-specific tables.

-- ============================================
-- Properties Table
--   Brief B point 1: "The system manages buildings and their units."
--   A Property is a building OR a standalone dwelling:
--     - Standalone: Bedrooms/MonthlyRent/Status are used (and non-null).
--     - Multi-unit: Bedrooms/MonthlyRent/Status are NULL; rent/status live on Units.
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Properties')
BEGIN
    CREATE TABLE [Properties] (
        [PropertyId]    INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Address]       NVARCHAR(200)   NOT NULL,
        [City]          NVARCHAR(100)   NOT NULL,
        [PropertyType]  INT             NOT NULL DEFAULT 0,   -- 0=Apartment, 1=Villa, 2=Shop, 3=Office
        [Bedrooms]      INT             NULL,                 -- nullable: only for standalone
        [MonthlyRent]   DECIMAL(10,2)   NULL,                 -- nullable: only for standalone
        [Status]        INT             NULL,                 -- 0=Available, 1=Leased, 2=UnderMaintenance; NULL for multi-unit
        [Description]   NVARCHAR(500)   NULL
    );
END
GO

-- ============================================
-- Units Table  (NEW in 2026-04-21)
--   A Unit is a rentable sub-part of a multi-unit Property
--   (e.g. apartment 101, shop G1, office Suite-7).
--   If a Property has 0 rows here, it is treated as standalone.
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Units')
BEGIN
    CREATE TABLE [Units] (
        [UnitId]        INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]    INT             NOT NULL,
        [UnitNumber]    NVARCHAR(20)    NOT NULL,              -- "101", "A-3", "Suite-7"
        [UnitType]      INT             NOT NULL DEFAULT 0,    -- 0=Studio, 1=OneBed, 2=TwoBed, 3=ThreeBed, 4=Office, 5=Shop, 6=Other
        [Amenities]     NVARCHAR(500)   NULL,
        [SizeSqm]       DECIMAL(8,2)    NOT NULL,
        [MonthlyRent]   DECIMAL(10,2)   NOT NULL,
        [Status]        INT             NOT NULL DEFAULT 0,    -- 0=Available, 1=Leased, 2=UnderMaintenance
        [Description]   NVARCHAR(500)   NULL,
        CONSTRAINT [FK_Units_Properties] FOREIGN KEY ([PropertyId]) REFERENCES [Properties]([PropertyId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Units_PropertyId] ON [Units]([PropertyId]);
    CREATE INDEX [IX_Units_Status]     ON [Units]([Status]);
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
--   UnitId is nullable:
--     - multi-unit property → UnitId MUST be set
--     - standalone property → UnitId MUST be NULL
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Leases')
BEGIN
    CREATE TABLE [Leases] (
        [LeaseId]           INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]        INT             NOT NULL,
        [UnitId]            INT             NULL,
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
        CONSTRAINT [FK_Leases_Units]      FOREIGN KEY ([UnitId])     REFERENCES [Units]([UnitId])         ON DELETE NO ACTION,
        CONSTRAINT [FK_Leases_Tenants]    FOREIGN KEY ([TenantId])   REFERENCES [Tenants]([TenantId])     ON DELETE CASCADE
    );
    CREATE INDEX [IX_Leases_PropertyId] ON [Leases]([PropertyId]);
    CREATE INDEX [IX_Leases_UnitId]     ON [Leases]([UnitId]);
    CREATE INDEX [IX_Leases_TenantId]   ON [Leases]([TenantId]);
    CREATE INDEX [IX_Leases_Status]     ON [Leases]([Status]);
END
GO

-- ============================================
-- MaintenanceRequests Table
--   Same UnitId rule as Leases.
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRequests')
BEGIN
    CREATE TABLE [MaintenanceRequests] (
        [RequestId]         INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]        INT             NOT NULL,
        [UnitId]            INT             NULL,
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
        CONSTRAINT [FK_Maintenance_Units]      FOREIGN KEY ([UnitId])     REFERENCES [Units]([UnitId])         ON DELETE NO ACTION,
        CONSTRAINT [FK_Maintenance_Tenants]    FOREIGN KEY ([TenantId])   REFERENCES [Tenants]([TenantId])     ON DELETE CASCADE,
        CONSTRAINT [FK_Maintenance_Staff]      FOREIGN KEY ([AssignedStaffId]) REFERENCES [AspNetUsers]([Id])  ON DELETE SET NULL
    );
    CREATE INDEX [IX_Maintenance_PropertyId] ON [MaintenanceRequests]([PropertyId]);
    CREATE INDEX [IX_Maintenance_UnitId]     ON [MaintenanceRequests]([UnitId]);
    CREATE INDEX [IX_Maintenance_Status]     ON [MaintenanceRequests]([Status]);
END
GO

-- ============================================
-- Payments Table  (unaffected by multi-unit refactor — keyed by Lease)
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
    CREATE INDEX [IX_Payments_LeaseId] ON [Payments]([LeaseId]);
    CREATE INDEX [IX_Payments_Status]  ON [Payments]([Status]);
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
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications]([UserId]);
END
GO
