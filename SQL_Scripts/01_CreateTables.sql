-- Property Leasing System: schema
-- Identity tables (AspNetUsers, AspNetRoles, ...) are created by EF Core.
-- Run EF migrations first, then this script for the application tables.

-- Properties
-- A Property is either standalone (single rentable dwelling) or multi-unit (has Units).
-- Standalone: Bedrooms, MonthlyRent, Status are filled in.
-- Multi-unit:  those three columns are NULL; rent and status live on Units instead.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Properties')
BEGIN
    CREATE TABLE [Properties] (
        [PropertyId]    INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Address]       NVARCHAR(200)   NOT NULL,
        [City]          NVARCHAR(100)   NOT NULL,
        [PropertyType]  INT             NOT NULL DEFAULT 0,   -- 0=Apartment 1=Villa 2=Shop 3=Office
        [Bedrooms]      INT             NULL,
        [MonthlyRent]   DECIMAL(10,2)   NULL,
        [Status]        INT             NULL,                 -- 0=Available 1=Leased 2=UnderMaintenance
        [Description]   NVARCHAR(500)   NULL
    );
END
GO

-- Units: a rentable sub-part of a multi-unit Property.
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Units')
BEGIN
    CREATE TABLE [Units] (
        [UnitId]        INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]    INT             NOT NULL,
        [UnitNumber]    NVARCHAR(20)    NOT NULL,
        [UnitType]      INT             NOT NULL DEFAULT 0,    -- 0=Studio 1=1BR 2=2BR 3=3BR 4=Office 5=Shop 6=Other
        [Amenities]     NVARCHAR(500)   NULL,
        [SizeSqm]       DECIMAL(8,2)    NOT NULL,
        [MonthlyRent]   DECIMAL(10,2)   NOT NULL,
        [Status]        INT             NOT NULL DEFAULT 0,    -- 0=Available 1=Leased 2=UnderMaintenance
        [Description]   NVARCHAR(500)   NULL,
        CONSTRAINT [FK_Units_Properties] FOREIGN KEY ([PropertyId])
            REFERENCES [Properties]([PropertyId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Units_PropertyId] ON [Units]([PropertyId]);
    CREATE INDEX [IX_Units_Status]     ON [Units]([Status]);
END
GO

-- Tenants
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tenants')
BEGIN
    CREATE TABLE [Tenants] (
        [TenantId]      INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FullName]      NVARCHAR(100)   NOT NULL,
        [Email]         NVARCHAR(450)   NOT NULL,
        [Phone]         NVARCHAR(20)    NOT NULL,
        [NationalId]    NVARCHAR(20)    NOT NULL,
        [UserId]        NVARCHAR(450)   NULL
    );
    CREATE INDEX [IX_Tenants_Email]      ON [Tenants]([Email]);
    CREATE INDEX [IX_Tenants_NationalId] ON [Tenants]([NationalId]);
    CREATE INDEX [IX_Tenants_UserId]     ON [Tenants]([UserId]);
END
GO

-- Leases
-- UnitId is NULL for standalone leases, required for multi-unit leases.
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
        [Status]            INT             NOT NULL DEFAULT 0,
        [ApplicationDate]   DATETIME2       NOT NULL DEFAULT GETDATE(),
        [ApplicationNotes]  NVARCHAR(500)   NULL,
        [ScreeningNotes]    NVARCHAR(500)   NULL,
        [ApprovalDate]      DATETIME2       NULL,
        CONSTRAINT [FK_Leases_Properties] FOREIGN KEY ([PropertyId])
            REFERENCES [Properties]([PropertyId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Leases_Units] FOREIGN KEY ([UnitId])
            REFERENCES [Units]([UnitId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Leases_Tenants] FOREIGN KEY ([TenantId])
            REFERENCES [Tenants]([TenantId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Leases_PropertyId] ON [Leases]([PropertyId]);
    CREATE INDEX [IX_Leases_UnitId]     ON [Leases]([UnitId]);
    CREATE INDEX [IX_Leases_TenantId]   ON [Leases]([TenantId]);
    CREATE INDEX [IX_Leases_Status]     ON [Leases]([Status]);
END
GO

-- MaintenanceRequests
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRequests')
BEGIN
    CREATE TABLE [MaintenanceRequests] (
        [RequestId]         INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PropertyId]        INT             NOT NULL,
        [UnitId]            INT             NULL,
        [TenantId]          INT             NOT NULL,
        [Title]             NVARCHAR(100)   NOT NULL,
        [Description]       NVARCHAR(500)   NOT NULL,
        [Category]          INT             NOT NULL DEFAULT 0,  -- 0=Plumbing 1=Electrical 2=HVAC 3=General
        [Priority]          INT             NOT NULL DEFAULT 1,  -- 0=Low 1=Medium 2=High 3=Urgent
        [Status]            INT             NOT NULL DEFAULT 0,  -- 0=Submitted 1=Assigned 2=InProgress 3=Resolved 4=Closed
        [AssignedStaffId]   NVARCHAR(450)   NULL,
        [StaffNotes]        NVARCHAR(500)   NULL,
        [DateSubmitted]     DATETIME2       NOT NULL DEFAULT GETDATE(),
        [DateAssigned]      DATETIME2       NULL,
        [DateResolved]      DATETIME2       NULL,
        CONSTRAINT [FK_Maintenance_Properties] FOREIGN KEY ([PropertyId])
            REFERENCES [Properties]([PropertyId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Maintenance_Units] FOREIGN KEY ([UnitId])
            REFERENCES [Units]([UnitId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Maintenance_Tenants] FOREIGN KEY ([TenantId])
            REFERENCES [Tenants]([TenantId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Maintenance_Staff] FOREIGN KEY ([AssignedStaffId])
            REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_Maintenance_PropertyId] ON [MaintenanceRequests]([PropertyId]);
    CREATE INDEX [IX_Maintenance_UnitId]     ON [MaintenanceRequests]([UnitId]);
    CREATE INDEX [IX_Maintenance_Status]     ON [MaintenanceRequests]([Status]);
END
GO

-- Payments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE [Payments] (
        [PaymentId]     INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [LeaseId]       INT             NOT NULL,
        [Amount]        DECIMAL(10,2)   NOT NULL,
        [DueDate]       DATETIME2       NOT NULL,
        [PaymentDate]   DATETIME2       NULL,
        [PaymentType]   INT             NOT NULL DEFAULT 0,  -- 0=Rent 1=Deposit 2=Fine
        [Status]        INT             NOT NULL DEFAULT 0,  -- 0=Pending 1=Completed 2=Overdue
        CONSTRAINT [FK_Payments_Leases] FOREIGN KEY ([LeaseId])
            REFERENCES [Leases]([LeaseId]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Payments_LeaseId] ON [Payments]([LeaseId]);
    CREATE INDEX [IX_Payments_Status]  ON [Payments]([Status]);
END
GO

-- Notifications
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationId]    INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]            NVARCHAR(450)   NOT NULL,
        [Title]             NVARCHAR(200)   NOT NULL,
        [Message]           NVARCHAR(500)   NOT NULL,
        [Type]              INT             NOT NULL DEFAULT 0,  -- 0=LeaseUpdate 1=MaintenanceUpdate 2=PaymentReminder 3=General
        [IsRead]            BIT             NOT NULL DEFAULT 0,
        [CreatedAt]         DATETIME2       NOT NULL DEFAULT GETDATE(),
        [LinkUrl]           NVARCHAR(200)   NULL,
        CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId])
            REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications]([UserId]);
END
GO

-- Extensions to AspNetUsers for maintenance staff routing.
IF COL_LENGTH('AspNetUsers', 'Skills') IS NULL
    ALTER TABLE [AspNetUsers] ADD [Skills] NVARCHAR(200) NULL;
GO
IF COL_LENGTH('AspNetUsers', 'IsAvailable') IS NULL
    ALTER TABLE [AspNetUsers] ADD [IsAvailable] BIT NOT NULL DEFAULT 1;
GO
IF COL_LENGTH('AspNetUsers', 'FullName') IS NULL
    ALTER TABLE [AspNetUsers] ADD [FullName] NVARCHAR(MAX) NOT NULL DEFAULT '';
GO

-- Link Tenants to their Identity login.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Tenants_AspNetUsers_UserId')
BEGIN
    ALTER TABLE [Tenants]
        ADD CONSTRAINT [FK_Tenants_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL;
END
GO
