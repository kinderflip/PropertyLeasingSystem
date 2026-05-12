-- Extended demo data for the Property Leasing System.
-- Adds extra properties, units, tenants, leases, payments,
-- maintenance requests and notifications on top of 02_SeedData.sql.
-- All inserts use IF NOT EXISTS so the script is safe to re-run.

BEGIN TRANSACTION;

-- Additional Properties
SET IDENTITY_INSERT [Properties] ON;

IF NOT EXISTS (SELECT 1 FROM Properties WHERE PropertyId = 1001)
    INSERT INTO Properties (PropertyId, Address, City, PropertyType, Bedrooms, MonthlyRent, [Status], [Description])
    VALUES (1001, 'Building 18, Road 1612, Block 216, Juffair', 'Manama', 0,
            NULL, NULL, NULL,
            'Juffair Heights apartment tower');

IF NOT EXISTS (SELECT 1 FROM Properties WHERE PropertyId = 1002)
    INSERT INTO Properties (PropertyId, Address, City, PropertyType, Bedrooms, MonthlyRent, [Status], [Description])
    VALUES (1002, 'Villa 224, Road 4514, Block 945, Hamad Town', 'Hamad Town', 1,
            3, 750.00, 0,
            'Al Zahrani Villa - 3 bedroom family villa');

SET IDENTITY_INSERT [Properties] OFF;


-- Additional Units (all for Juffair Heights)
SET IDENTITY_INSERT [Units] ON;

IF NOT EXISTS (SELECT 1 FROM Units WHERE UnitId = 1001)
    INSERT INTO Units (UnitId, PropertyId, UnitNumber, UnitType, Amenities, SizeSqm, MonthlyRent, [Status], [Description])
    VALUES (1001, 1001, 'A-01', 3,
            'AC, 3 balconies, Fully furnished kitchen, Covered parking, Sea view',
            120.00, 850.00, 1,
            'Third floor 3BR with sea view');

IF NOT EXISTS (SELECT 1 FROM Units WHERE UnitId = 1002)
    INSERT INTO Units (UnitId, PropertyId, UnitNumber, UnitType, Amenities, SizeSqm, MonthlyRent, [Status], [Description])
    VALUES (1002, 1001, 'A-02', 0,
            'AC, Compact kitchen, High-speed internet, Building gym access',
            35.00, 320.00, 0,
            'Studio unit');

IF NOT EXISTS (SELECT 1 FROM Units WHERE UnitId = 1003)
    INSERT INTO Units (UnitId, PropertyId, UnitNumber, UnitType, Amenities, SizeSqm, MonthlyRent, [Status], [Description])
    VALUES (1003, 1001, 'B-01', 2,
            'AC, Balcony, Furnished kitchen, Storage room',
            85.00, 580.00, 2,
            'Second floor 2BR, currently under maintenance');

SET IDENTITY_INSERT [Units] OFF;


-- Additional Tenants
SET IDENTITY_INSERT [Tenants] ON;

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1001)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1001, 'Khalid Ibrahim', 'khalid.ibrahim@example.bh', '+97333661122', '880312345', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1002)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1002, 'Maryam Said', 'maryam.said@example.bh', '+97333772233', '920745678', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1003)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1003, 'Ali Saleh', 'ali.saleh@example.bh', '+97333883344', '950218765', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1004)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1004, 'Layla Omar', 'layla.omar@example.bh', '+97333994455', '890567890', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1005)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1005, 'Mansour Salim', 'mansour.salim@example.bh', '+97333115566', '970134567', NULL);

SET IDENTITY_INSERT [Tenants] OFF;


-- Sync unit and property statuses with the leases added below.
UPDATE Units SET [Status] = 1 WHERE UnitId IN (1, 2, 3, 4, 6, 1001);
UPDATE Units SET [Status] = 0 WHERE UnitId = 5;
UPDATE Units SET [Status] = 2 WHERE UnitId = 1003;

UPDATE Properties SET [Status] = 1 WHERE PropertyId = 2;
UPDATE Properties SET [Status] = 0 WHERE PropertyId = 1002;


-- Leases
SET IDENTITY_INSERT [Leases] ON;

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1001)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1001, 1, 1, 1001, '2025-02-01', '2026-02-01', 450.00, 4,
            '2025-01-10', '2025-01-20',
            'Long-term residency preferred.',
            'Employment verified at Alba. Credit check passed.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1002)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1002, 1, 2, 1002, '2025-04-01', '2026-04-01', 470.00, 4,
            '2025-03-12', '2025-03-22',
            'Single occupant, professional.',
            'Government employee. References confirmed.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1003)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1003, 1, 3, 1003, '2025-07-01', '2026-07-01', 650.00, 4,
            '2025-06-15', '2025-06-25',
            'Family of 3, requires 2BR.',
            'Income statement and family ID verified.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1004)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1004, 3, 4, 1, '2025-03-01', '2026-03-01', 380.00, 4,
            '2025-02-08', '2025-02-18',
            'Retail business, mobile accessories.',
            'Commercial registration verified.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1005)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1005, 3, 6, 1004, '2025-05-01', '2026-05-01', 550.00, 4,
            '2025-04-10', '2025-04-20',
            'Law firm with 3 staff members.',
            'CR and trade licence verified.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1006)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1006, 2, NULL, 1005, '2025-01-01', '2026-01-01', 900.00, 4,
            '2024-12-10', '2024-12-20',
            'Family of 5, long-term stay preferred.',
            'Bank statement and salary slip verified.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1007)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1007, 1001, 1001, 102, '2025-06-01', '2026-06-01', 850.00, 4,
            '2025-05-15', '2025-05-25',
            'Prefers sea view unit.',
            'Employment letter from Bapco.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1008)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1008, 3, 5, 103, '2024-01-01', '2025-01-01', 420.00, 6,
            '2023-12-05', '2023-12-15',
            'Seasonal retail, gifts and home decor.',
            'Trade licence presented.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1009)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1009, 1002, NULL, 2, '2026-05-01', '2027-05-01', 750.00, 0,
            '2026-04-20', NULL,
            '1-year lease with renewal option. Viewing completed.',
            NULL);

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1010)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1010, 1001, 1002, 101, '2026-05-01', '2027-05-01', 320.00, 1,
            '2026-04-15', NULL,
            'Studio needed for 12-month work placement.',
            'Awaiting employment confirmation letter.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1011)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1011, 1001, 1002, 103, '2025-09-01', '2026-09-01', 320.00, 3,
            '2025-08-20', NULL,
            'Looking for affordable unit close to work.',
            'Income insufficient for rent-to-income ratio.');

IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1012)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1012, 1, 1, 1, '2024-01-01', '2025-01-01', 430.00, 6,
            '2023-12-01', '2023-12-10',
            'First-time lease application.',
            'ID and salary slip verified.');

SET IDENTITY_INSERT [Leases] OFF;
DBCC CHECKIDENT ('Leases', RESEED, 2000);


-- Payments
SET IDENTITY_INSERT [Payments] ON;

-- Lease 1001 (Pearl 101, Khalid)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1001)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1001, 1001, 900.00,  '2025-02-01', '2025-01-30', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1002)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1002, 1001, 450.00,  '2025-02-01', '2025-02-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1003)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1003, 1001, 450.00,  '2025-03-01', '2025-03-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1004)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1004, 1001, 450.00,  '2025-04-01', '2025-04-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1005)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1005, 1001, 450.00,  '2025-05-01', '2025-05-03', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1006)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1006, 1001, 50.00,   '2025-05-15', '2025-05-20', 2, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1007)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1007, 1001, 450.00,  '2025-06-01', '2025-06-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1008)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1008, 1001, 450.00,  '2026-03-01', NULL,         0, 2);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1009)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1009, 1001, 450.00,  '2026-04-01', NULL,         0, 0);

-- Lease 1002 (Pearl 102, Mariam)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1010)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1010, 1002, 940.00,  '2025-04-01', '2025-03-30', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1011)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1011, 1002, 470.00,  '2025-04-01', '2025-04-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1012)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1012, 1002, 470.00,  '2025-05-01', '2025-05-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1013)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1013, 1002, 470.00,  '2025-06-01', '2025-06-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1014)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1014, 1002, 470.00,  '2026-04-01', NULL,         0, 0);

-- Lease 1003 (Pearl 201, Abdullah)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1015)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1015, 1003, 1300.00, '2025-07-01', '2025-06-28', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1016)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1016, 1003, 650.00,  '2025-07-01', '2025-07-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1017)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1017, 1003, 650.00,  '2025-08-01', '2025-08-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1018)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1018, 1003, 650.00,  '2025-09-01', '2025-09-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1019)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1019, 1003, 650.00,  '2026-04-01', NULL,         0, 0);

-- Lease 1004 (Riffa G1, Ahmed)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1020)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1020, 1004, 760.00,  '2025-03-01', '2025-02-28', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1021)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1021, 1004, 380.00,  '2025-03-01', '2025-03-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1022)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1022, 1004, 380.00,  '2025-04-01', '2025-04-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1023)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1023, 1004, 380.00,  '2025-05-01', '2025-05-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1024)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1024, 1004, 380.00,  '2026-02-01', NULL,         0, 2);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1025)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1025, 1004, 380.00,  '2026-03-01', NULL,         0, 0);

-- Lease 1005 (Riffa Suite-7, Latifa)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1026)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1026, 1005, 1100.00, '2025-05-01', '2025-04-29', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1027)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1027, 1005, 550.00,  '2025-05-01', '2025-05-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1028)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1028, 1005, 550.00,  '2025-06-01', '2025-06-03', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1029)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1029, 1005, 550.00,  '2025-07-01', '2025-07-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1030)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1030, 1005, 550.00,  '2026-04-01', NULL,         0, 0);

-- Lease 1006 (Al Fateh Villa, Tariq)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1031)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1031, 1006, 1800.00, '2025-01-01', '2024-12-30', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1032)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1032, 1006, 900.00,  '2025-01-01', '2025-01-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1033)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1033, 1006, 900.00,  '2025-02-01', '2025-02-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1034)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1034, 1006, 900.00,  '2025-03-01', '2025-03-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1035)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1035, 1006, 900.00,  '2025-04-01', '2025-04-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1036)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1036, 1006, 900.00,  '2025-12-01', NULL,         0, 2);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1037)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1037, 1006, 900.00,  '2026-04-01', NULL,         0, 0);

-- Lease 1007 (Juffair A-01, Hamad)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1038)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1038, 1007, 1700.00, '2025-06-01', '2025-05-30', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1039)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1039, 1007, 850.00,  '2025-06-01', '2025-06-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1040)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1040, 1007, 850.00,  '2025-07-01', '2025-07-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1041)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1041, 1007, 850.00,  '2025-08-01', '2025-08-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1042)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1042, 1007, 850.00,  '2026-04-01', NULL,         0, 0);

-- Lease 1008 (Riffa G2, Noor, expired)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1043)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1043, 1008, 840.00,  '2024-01-01', '2023-12-29', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1044)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1044, 1008, 420.00,  '2024-01-01', '2024-01-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1045)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1045, 1008, 420.00,  '2024-04-01', '2024-04-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1046)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1046, 1008, 420.00,  '2024-07-01', '2024-07-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1047)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1047, 1008, 420.00,  '2024-10-01', '2024-10-03', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1048)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1048, 1008, 420.00,  '2025-01-01', '2025-01-01', 0, 1);

-- Lease 1012 (Pearl 101, Ahmed, expired)
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1049)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1049, 1012, 860.00,  '2024-01-01', '2023-12-28', 1, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1050)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1050, 1012, 430.00,  '2024-01-01', '2024-01-01', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1051)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1051, 1012, 430.00,  '2024-06-01', '2024-06-02', 0, 1);

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1052)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1052, 1012, 430.00,  '2025-01-01', '2025-01-01', 0, 1);

SET IDENTITY_INSERT [Payments] OFF;
DBCC CHECKIDENT ('Payments', RESEED, 2000);


-- Maintenance Requests
DECLARE @StaffId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'staff@property.com');

SET IDENTITY_INSERT [MaintenanceRequests] ON;

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1001)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1001, 1, 1, 1001,
            'AC Not Cooling Properly',
            'AC unit in the living room runs but does not produce cold air. Temperature stays above 28C even on maximum setting.',
            2, 1, 0, NULL, NULL, '2026-04-10', NULL, NULL);

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1002)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1002, 1, 2, 1002,
            'Water Leak Under Kitchen Sink',
            'Slow drip from the pipe joint under the kitchen sink. Water is pooling in the cabinet below.',
            0, 2, 1, @StaffId,
            'Will replace joint seal and inspect surrounding pipes.',
            '2026-04-08', '2026-04-09', NULL);

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1003)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1003, 1, 3, 1003,
            'Bedroom Ceiling Light Not Working',
            'Ceiling light fixture in master bedroom stopped working. Bulb was replaced by tenant but still no light.',
            1, 0, 3, @StaffId,
            'Replaced faulty light fitting and checked wiring. Working.',
            '2026-03-20', '2026-03-21', '2026-03-22');

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1004)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1004, 3, 4, 1,
            'Front Roller Shutter Jammed',
            'Front roller shutter is stuck halfway and cannot be fully opened or closed.',
            3, 1, 2, @StaffId,
            'Lubricated tracks and adjusted tension spring. Replacement spring ordered, ETA 3 days.',
            '2026-04-12', '2026-04-13', NULL);

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1005)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1005, 2, NULL, 1005,
            'Garden Irrigation System Failure',
            'Automated irrigation system has stopped working. Plants and grass at risk during summer.',
            0, 2, 0, NULL, NULL, '2026-04-15', NULL, NULL);

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1006)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1006, 1001, 1001, 102,
            'Annual AC Preventive Maintenance',
            'Requesting scheduled annual maintenance for the central AC unit.',
            2, 0, 4, @StaffId,
            'Filters cleaned, refrigerant topped up, electrical connections checked, system running.',
            '2025-12-01', '2025-12-05', '2025-12-06');

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1007)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1007, 1001, 1003, 102,
            'Water Heater Burst',
            'Pressurised water heater tank has burst causing water damage in the bathroom. Unit not habitable.',
            0, 3, 2, @StaffId,
            'Water supply isolated. Replacement 80L tank ordered. Drying equipment installed. ETA 5 days.',
            '2026-04-05', '2026-04-05', NULL);

IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1008)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1008, 3, 6, 1004,
            'Electrical Socket Sparking',
            'Double power socket near the server rack produces visible sparks when equipment is plugged in.',
            1, 2, 3, @StaffId,
            'Faulty socket replaced and wiring inspected. Safe to use.',
            '2026-03-28', '2026-03-28', '2026-03-29');

SET IDENTITY_INSERT [MaintenanceRequests] OFF;
DBCC CHECKIDENT ('MaintenanceRequests', RESEED, 2000);


-- Notifications for the manager account
DECLARE @ManagerId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'manager@property.com');

SET IDENTITY_INSERT [Notifications] ON;

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1001)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1001, @ManagerId,
            'New Lease Application',
            'Sara Faisal has submitted a lease application for Hamad Town Villa.',
            0, 0, '2026-04-20 09:00:00', '/Leases/Details/1009');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1002)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1002, @ManagerId,
            'Overdue Payment - Riffa G1',
            'Payment for Lease 1004 (Ahmed Hassan) is overdue. Due date was 1 Feb 2026.',
            2, 0, '2026-02-05 08:00:00', '/Payments');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1003)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1003, @ManagerId,
            'Urgent Maintenance - Juffair B-01',
            'Water heater burst in Juffair Heights Unit B-01. Repair in progress.',
            1, 0, '2026-04-05 07:30:00', '/MaintenanceRequests/Details/1007');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1004)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1004, @ManagerId,
            'Lease Expiring - Al Fateh Villa',
            'Lease for Al Fateh Villa (Mansour Salim) expires 1 Jan 2026.',
            0, 1, '2025-12-01 10:00:00', '/Leases/Details/1006');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1005)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1005, @ManagerId,
            'Maintenance Resolved - Pearl Blvd Unit 201',
            'Bedroom ceiling light in Pearl Boulevard Unit 201 has been repaired.',
            1, 1, '2026-03-22 14:00:00', '/MaintenanceRequests/Details/1003');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1006)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1006, @ManagerId,
            'Application In Screening',
            'Fatima Yousef application for Juffair Heights Studio A-02 is in screening.',
            0, 1, '2026-04-16 11:00:00', '/Leases/Details/1010');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1007)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1007, @ManagerId,
            'Payment Received - Juffair A-01',
            'Monthly rent of 850.000 BD received for Juffair Heights A-01 (Sami Mohamed).',
            2, 1, '2025-08-01 09:15:00', '/Payments');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1008)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1008, @ManagerId,
            'Overdue Payment - Al Fateh Villa',
            'Al Fateh Villa rent for Dec 2025 is overdue.',
            2, 0, '2026-01-05 08:00:00', '/Payments');

SET IDENTITY_INSERT [Notifications] OFF;
DBCC CHECKIDENT ('Notifications', RESEED, 2000);

COMMIT;
