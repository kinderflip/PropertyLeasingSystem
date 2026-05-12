-- Sample data for PropertyLeasingDB.
-- Identity roles and the manager/staff accounts are created by the application
-- at startup, so this file only seeds the business tables.

-- Properties
SET IDENTITY_INSERT [Properties] ON;

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 1)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (1, 'Building 2455, Road 2832, Block 428, Seef District', 'Manama', 0, NULL, NULL, NULL,
            'Pearl Boulevard Residences');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 2)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (2, 'House 108, Road 3803, Block 338, Juffair', 'Manama', 1, 4, 900.000, 0,
            'Al Fateh Villa - 4 bedroom standalone with garden');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 3)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (3, 'Building 217, Road 2409, Block 924, East Riffa', 'Riffa', 2, NULL, NULL, NULL,
            'Riffa Commercial Centre');

SET IDENTITY_INSERT [Properties] OFF;
GO

-- Units
SET IDENTITY_INSERT [Units] ON;

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 1)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (1, 1, '101', 1, 'Furnished, AC, WiFi, covered parking', 60.00, 450.000, 0, 'First floor 1BR');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 2)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (2, 1, '102', 1, 'Furnished, AC, WiFi', 62.00, 470.000, 0, 'First floor 1BR');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 3)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (3, 1, '201', 2, 'Furnished, AC, WiFi, balcony, 2 parking', 95.00, 650.000, 0, 'Second floor 2BR');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 4)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (4, 3, 'G1', 5, 'Street frontage, shutter', 45.00, 380.000, 0, 'Ground floor shop, corner unit');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 5)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (5, 3, 'G2', 5, 'Street frontage, shutter', 50.00, 420.000, 0, 'Ground floor shop');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 6)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (6, 3, 'F1-Suite-7', 4, 'Partitioned, AC, reception, 5 parking spaces', 70.00, 550.000, 0, 'First floor office suite');

SET IDENTITY_INSERT [Units] OFF;
GO

-- Tenants
SET IDENTITY_INSERT [Tenants] ON;

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 1)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (1, 'Ahmed Hassan', 'ahmed.hassan@example.bh', '+97333112233', '870412345');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 2)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (2, 'Sara Faisal', 'sara.faisal@example.bh', '+97333445566', '900823456');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 3)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (3, 'Fatima Yousef', 'fatima.yousef@example.bh', '+97333778899', '920345678');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 4)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (4, 'Sami Mohamed', 'sami.mohamed@example.bh', '+97333224455', '850612345');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 5)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (5, 'Nada Tariq', 'nada.tariq@example.bh', '+97333557788', '940109876');

SET IDENTITY_INSERT [Tenants] OFF;
GO

-- Leases
SET IDENTITY_INSERT [Leases] ON;

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 1)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [UnitId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (1, 1, 1, 1, '2026-01-01', '2026-12-31', 450.000, 4, '2025-12-15', 'Tenant has good credit history.');

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 2)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [UnitId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (2, 2, NULL, 2, '2026-03-01', '2027-02-28', 900.000, 4, '2026-02-10', 'Family looking for long-term lease.');

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 3)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [UnitId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (3, 3, 4, 4, '2026-05-01', '2027-04-30', 380.000, 0, '2026-04-15', 'New application for corner shop.');

SET IDENTITY_INSERT [Leases] OFF;
GO

UPDATE [Units]      SET [Status] = 1 WHERE [UnitId] = 1;
UPDATE [Properties] SET [Status] = 1 WHERE [PropertyId] = 2;
GO

-- Maintenance Requests
SET IDENTITY_INSERT [MaintenanceRequests] ON;

IF NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] WHERE [RequestId] = 1)
    INSERT INTO [MaintenanceRequests] ([RequestId], [PropertyId], [UnitId], [TenantId], [Title], [Description], [Category], [Priority], [Status], [DateSubmitted])
    VALUES (1, 1, 1, 1, 'Leaking kitchen faucet', 'Kitchen faucet in Unit 101 has been dripping for two days.', 0, 2, 0, '2026-04-01');

IF NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] WHERE [RequestId] = 2)
    INSERT INTO [MaintenanceRequests] ([RequestId], [PropertyId], [UnitId], [TenantId], [Title], [Description], [Category], [Priority], [Status], [DateSubmitted])
    VALUES (2, 2, NULL, 2, 'AC not cooling - master bedroom', 'Master bedroom AC runs but does not cool.', 2, 3, 0, '2026-04-05');

SET IDENTITY_INSERT [MaintenanceRequests] OFF;
GO

-- Payments
SET IDENTITY_INSERT [Payments] ON;

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 1)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (1, 1, 450.000, '2026-01-01', '2026-01-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 2)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (2, 1, 450.000, '2026-02-01', '2026-02-03', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 3)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (3, 1, 450.000, '2026-03-01', '2026-03-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 4)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (4, 1, 450.000, '2026-04-01', NULL, 0, 0);

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 5)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (5, 1, 900.000, '2026-01-01', '2025-12-28', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 6)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (6, 2, 900.000, '2026-03-01', '2026-03-01', 0, 1);

SET IDENTITY_INSERT [Payments] OFF;
GO
