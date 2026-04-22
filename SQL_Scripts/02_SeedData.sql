-- ============================================
-- Property Leasing System - Sample/Seed Data
-- Database: PropertyLeasingDB (Azure SQL / LocalDB)
-- Project: IT8118 Advanced Programming - Brief B
-- Updated 2026-04-21: Bahrain-localised data + multi-unit & standalone mix
-- ============================================

-- Note: Identity roles and users are seeded by EF Core in AppDbContext.OnModelCreating
-- and Program.cs respectively. This script provides additional sample data.

-- ============================================
-- Insert Sample Properties
--   P1 = Pearl Boulevard Residences (Seef)  -- multi-unit  (Bedrooms/MonthlyRent/Status = NULL)
--   P2 = Al Fateh Villa (Juffair)            -- standalone (rent/status populated)
--   P3 = Riffa Commercial Centre (Riffa)     -- multi-unit
-- ============================================
SET IDENTITY_INSERT [Properties] ON;

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 1)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (1, 'Building 2455, Road 2832, Block 428, Seef District', 'Manama', 0, NULL, NULL, NULL,
            'Pearl Boulevard Residences — modern apartment building (multi-unit).');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 2)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (2, 'House 108, Road 3803, Block 338, Juffair', 'Manama', 1, 4, 900.000, 0,
            'Al Fateh Villa — standalone 4-bedroom villa with private garden.');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 3)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (3, 'Building 217, Road 2409, Block 924, East Riffa', 'Riffa', 2, NULL, NULL, NULL,
            'Riffa Commercial Centre — ground-floor shops and first-floor offices (multi-unit).');

SET IDENTITY_INSERT [Properties] OFF;
GO

-- ============================================
-- Insert Sample Units
-- ============================================
SET IDENTITY_INSERT [Units] ON;

-- Pearl Boulevard Residences (PropertyId = 1)
IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 1)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (1, 1, '101', 1, 'Furnished, AC, WiFi, covered parking', 60.00, 450.000, 0, 'First-floor 1-bedroom facing Seef boulevard.');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 2)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (2, 1, '102', 1, 'Furnished, AC, WiFi', 62.00, 470.000, 0, 'First-floor 1-bedroom, quieter side.');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 3)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (3, 1, '201', 2, 'Furnished, AC, WiFi, balcony, 2 parking', 95.00, 650.000, 0, 'Second-floor 2-bedroom with balcony.');

-- Riffa Commercial Centre (PropertyId = 3)
IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 4)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (4, 3, 'G1', 5, 'Street frontage, shutter, water+power metered', 45.00, 380.000, 0, 'Ground-floor shop, corner unit.');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 5)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (5, 3, 'G2', 5, 'Street frontage, shutter', 50.00, 420.000, 0, 'Ground-floor shop next to G1.');

IF NOT EXISTS (SELECT 1 FROM [Units] WHERE [UnitId] = 6)
    INSERT INTO [Units] ([UnitId], [PropertyId], [UnitNumber], [UnitType], [Amenities], [SizeSqm], [MonthlyRent], [Status], [Description])
    VALUES (6, 3, 'F1-Suite-7', 4, 'Partitioned, AC, reception, 5 parking spaces', 70.00, 550.000, 0, 'First-floor office suite.');

SET IDENTITY_INSERT [Units] OFF;
GO

-- ============================================
-- Insert Sample Tenants (Bahraini names)
-- ============================================
SET IDENTITY_INSERT [Tenants] ON;

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 1)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (1, 'Ahmed bin Mohammed Al Mansoori', 'ahmed.mansoori@example.bh', '+973 3311 2233', '870412345');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 2)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (2, 'Sara bint Khalifa Al Khalifa', 'sara.khalifa@example.bh', '+973 3344 5566', '900823456');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 3)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (3, 'Fatima bint Isa Al Dosari', 'fatima.dosari@example.bh', '+973 3377 8899', '920345678');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 4)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (4, 'Hamad bin Salman Al Mahmood', 'hamad.mahmood@example.bh', '+973 3322 4455', '850612345');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 5)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (5, 'Noor bint Ali Al Zayani', 'noor.zayani@example.bh', '+973 3355 7788', '940109876');

SET IDENTITY_INSERT [Tenants] OFF;
GO

-- ============================================
-- Insert Sample Leases
--   L1: Unit 101 (Pearl Boulevard) leased by Ahmed   (multi-unit → UnitId = 1)
--   L2: Al Fateh Villa leased by Sara                (standalone → UnitId = NULL)
--   L3: Shop G1 (Riffa) application by Hamad         (multi-unit → UnitId = 4)
-- ============================================
SET IDENTITY_INSERT [Leases] ON;

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 1)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [UnitId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (1, 1, 1, 1, '2026-01-01', '2026-12-31', 450.000, 4, '2025-12-15', 'Tenant has good credit history.');

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 2)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [UnitId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (2, 2, NULL, 2, '2026-03-01', '2027-02-28', 900.000, 4, '2026-02-10', 'Family looking for long-term lease on standalone villa.');

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 3)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [UnitId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (3, 3, 4, 4, '2026-05-01', '2027-04-30', 380.000, 0, '2026-04-15', 'New application for corner shop.');

SET IDENTITY_INSERT [Leases] OFF;
GO

-- Reflect statuses on Units/Properties to match leases above.
UPDATE [Units]      SET [Status] = 1 WHERE [UnitId] = 1;         -- Unit 101 is leased
UPDATE [Properties] SET [Status] = 1 WHERE [PropertyId] = 2;     -- Al Fateh Villa leased
GO

-- ============================================
-- Insert Sample Maintenance Requests
-- ============================================
SET IDENTITY_INSERT [MaintenanceRequests] ON;

IF NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] WHERE [RequestId] = 1)
    INSERT INTO [MaintenanceRequests] ([RequestId], [PropertyId], [UnitId], [TenantId], [Title], [Description], [Category], [Priority], [Status], [DateSubmitted])
    VALUES (1, 1, 1, 1, 'Leaking kitchen faucet', 'Kitchen faucet in Unit 101 has been dripping constantly for 2 days.', 0, 2, 0, '2026-04-01');

IF NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] WHERE [RequestId] = 2)
    INSERT INTO [MaintenanceRequests] ([RequestId], [PropertyId], [UnitId], [TenantId], [Title], [Description], [Category], [Priority], [Status], [DateSubmitted])
    VALUES (2, 2, NULL, 2, 'AC not cooling (villa master bedroom)', 'Master bedroom AC runs but does not cool.', 2, 3, 0, '2026-04-05');

SET IDENTITY_INSERT [MaintenanceRequests] OFF;
GO

-- ============================================
-- Insert Sample Payments (all amounts in BD)
-- ============================================
SET IDENTITY_INSERT [Payments] ON;

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 1)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (1, 1, 450.000, '2026-01-01', '2026-01-01', 0, 1);  -- Jan rent - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 2)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (2, 1, 450.000, '2026-02-01', '2026-02-03', 0, 1);  -- Feb rent - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 3)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (3, 1, 450.000, '2026-03-01', '2026-03-01', 0, 1);  -- Mar rent - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 4)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (4, 1, 450.000, '2026-04-01', NULL, 0, 0);          -- Apr rent - Pending

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 5)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (5, 1, 900.000, '2026-01-01', '2025-12-28', 1, 1);  -- 2-month security deposit - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 6)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (6, 2, 900.000, '2026-03-01', '2026-03-01', 0, 1);  -- Villa Mar rent - Paid

SET IDENTITY_INSERT [Payments] OFF;
GO

PRINT 'Sample data inserted successfully.';
GO
