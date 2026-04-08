-- ============================================
-- Property Leasing System - Sample/Seed Data
-- Database: PropertyLeasingDB (Azure SQL)
-- Project: IT8118 Advanced Programming - Brief B
-- ============================================

-- Note: Identity roles and users are seeded by EF Core in AppDbContext.OnModelCreating
-- and Program.cs respectively. This script provides additional sample data.

-- ============================================
-- Insert Sample Properties (if not already seeded)
-- ============================================
SET IDENTITY_INSERT [Properties] ON;

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 1)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (1, '123 Pearl Boulevard', 'Manama', 0, 2, 350.00, 0, 'Modern apartment near the city center');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 2)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (2, '45 Seef District', 'Manama', 1, 4, 800.00, 0, 'Spacious villa with private garden');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 3)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (3, '78 Riffa Valley Road', 'Riffa', 2, 0, 500.00, 0, 'Commercial shop in busy area');

-- Additional sample properties
IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 4)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (4, '15 Juffair Heights', 'Juffair', 0, 3, 450.00, 0, 'Luxury apartment with sea view');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 5)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (5, '92 Budaiya Highway', 'Budaiya', 1, 5, 1200.00, 0, 'Large villa with pool and parking');

IF NOT EXISTS (SELECT 1 FROM [Properties] WHERE [PropertyId] = 6)
    INSERT INTO [Properties] ([PropertyId], [Address], [City], [PropertyType], [Bedrooms], [MonthlyRent], [Status], [Description])
    VALUES (6, '33 Exhibition Road', 'Manama', 3, 0, 600.00, 0, 'Office space in commercial tower');

SET IDENTITY_INSERT [Properties] OFF;
GO

-- ============================================
-- Insert Sample Tenants (if not already seeded)
-- ============================================
SET IDENTITY_INSERT [Tenants] ON;

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 1)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (1, 'Ahmed Al Mansoori', 'ahmed@email.com', '+97333112233', '880112345');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 2)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (2, 'Sara Al Khalifa', 'sara@email.com', '+97333445566', '920567890');

-- Additional sample tenants
IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 3)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (3, 'Mohammed Hasan', 'mohammed@email.com', '+97366778899', '951234567');

IF NOT EXISTS (SELECT 1 FROM [Tenants] WHERE [TenantId] = 4)
    INSERT INTO [Tenants] ([TenantId], [FullName], [Email], [Phone], [NationalId])
    VALUES (4, 'Fatima Al Dosari', 'fatima@email.com', '+97339988776', '990876543');

SET IDENTITY_INSERT [Tenants] OFF;
GO

-- ============================================
-- Insert Sample Leases
-- ============================================
SET IDENTITY_INSERT [Leases] ON;

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 1)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (1, 1, 1, '2026-01-01', '2026-12-31', 350.00, 4, '2025-12-15', 'Tenant has good credit history');

IF NOT EXISTS (SELECT 1 FROM [Leases] WHERE [LeaseId] = 2)
    INSERT INTO [Leases] ([LeaseId], [PropertyId], [TenantId], [StartDate], [EndDate], [MonthlyRent], [Status], [ApplicationDate], [ApplicationNotes])
    VALUES (2, 2, 2, '2026-03-01', '2027-02-28', 800.00, 0, '2026-02-20', 'Family looking for long-term lease');

SET IDENTITY_INSERT [Leases] OFF;
GO

-- ============================================
-- Insert Sample Maintenance Requests
-- ============================================
SET IDENTITY_INSERT [MaintenanceRequests] ON;

IF NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] WHERE [RequestId] = 1)
    INSERT INTO [MaintenanceRequests] ([RequestId], [PropertyId], [TenantId], [Title], [Description], [Category], [Priority], [Status], [DateSubmitted])
    VALUES (1, 1, 1, 'Leaking kitchen faucet', 'The kitchen faucet has been dripping constantly for 2 days.', 0, 2, 0, '2026-04-01');

IF NOT EXISTS (SELECT 1 FROM [MaintenanceRequests] WHERE [RequestId] = 2)
    INSERT INTO [MaintenanceRequests] ([RequestId], [PropertyId], [TenantId], [Title], [Description], [Category], [Priority], [Status], [DateSubmitted])
    VALUES (2, 1, 1, 'AC not cooling', 'The air conditioning unit in the bedroom is running but not cooling.', 2, 3, 0, '2026-04-05');

SET IDENTITY_INSERT [MaintenanceRequests] OFF;
GO

-- ============================================
-- Insert Sample Payments
-- ============================================
SET IDENTITY_INSERT [Payments] ON;

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 1)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (1, 1, 350.00, '2026-01-01', '2026-01-01', 0, 1);  -- January rent - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 2)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (2, 1, 350.00, '2026-02-01', '2026-02-03', 0, 1);  -- February rent - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 3)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (3, 1, 350.00, '2026-03-01', '2026-03-01', 0, 1);  -- March rent - Paid

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 4)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (4, 1, 350.00, '2026-04-01', NULL, 0, 0);          -- April rent - Pending

IF NOT EXISTS (SELECT 1 FROM [Payments] WHERE [PaymentId] = 5)
    INSERT INTO [Payments] ([PaymentId], [LeaseId], [Amount], [DueDate], [PaymentDate], [PaymentType], [Status])
    VALUES (5, 1, 700.00, '2026-01-01', '2025-12-28', 1, 1);  -- Security deposit - Paid

SET IDENTITY_INSERT [Payments] OFF;
GO

PRINT 'Sample data inserted successfully.';
GO
