-- ============================================================
-- RICH DEMO DATA — Property Leasing System
-- Run this in: Azure Portal → PropertyLeasingDB → Query Editor
-- Safe to re-run (all inserts use IF NOT EXISTS checks)
-- ============================================================

BEGIN TRANSACTION;

-- ─────────────────────────────────────────────────────────────
-- ENUM REFERENCE
-- PropertyType:    Apartment=0, Villa=1, Shop=2, Office=3
-- PropertyStatus:  Available=0, Leased=1, UnderMaintenance=2
-- UnitType:        Studio=0, OneBedroom=1, TwoBedroom=2, ThreeBedroom=3, Office=4, Shop=5, Other=6
-- UnitStatus:      Available=0, Leased=1, UnderMaintenance=2
-- LeaseStatus:     Application=0, Screening=1, Approved=2, Rejected=3, Active=4, Renewal=5, Expired=6, Terminated=7
-- PaymentType:     Rent=0, Deposit=1, Fine=2
-- PaymentStatus:   Pending=0, Completed=1, Overdue=2
-- MaintCategory:   Plumbing=0, Electrical=1, HVAC=2, General=3
-- MaintStatus:     Submitted=0, Assigned=1, InProgress=2, Resolved=3, Closed=4
-- MaintPriority:   Low=0, Medium=1, High=2, Urgent=3
-- NotifType:       LeaseUpdate=0, MaintenanceUpdate=1, PaymentReminder=2, General=3
-- ─────────────────────────────────────────────────────────────


-- ═══════════════════════════════════════════════════════════
-- 1. NEW PROPERTIES  (IDs 1001, 1002)
-- ═══════════════════════════════════════════════════════════
SET IDENTITY_INSERT [Properties] ON;

-- Multi-unit apartment building in Juffair
IF NOT EXISTS (SELECT 1 FROM Properties WHERE PropertyId = 1001)
    INSERT INTO Properties (PropertyId, Address, City, PropertyType, Bedrooms, MonthlyRent, [Status], [Description])
    VALUES (1001, 'Building 18, Road 1612, Block 216, Juffair', 'Manama', 0,
            NULL, NULL, NULL,
            'Juffair Heights — modern apartment tower near the diplomatic area and US Naval Base.');

-- Standalone villa in Hamad Town
IF NOT EXISTS (SELECT 1 FROM Properties WHERE PropertyId = 1002)
    INSERT INTO Properties (PropertyId, Address, City, PropertyType, Bedrooms, MonthlyRent, [Status], [Description])
    VALUES (1002, 'Villa 224, Road 4514, Block 945, Hamad Town', 'Hamad Town', 1,
            3, 750.00, 0,
            'Al Zahrani Villa — spacious 3-bedroom family villa with private parking and garden.');

SET IDENTITY_INSERT [Properties] OFF;


-- ═══════════════════════════════════════════════════════════
-- 2. NEW UNITS  (IDs 1001-1003, all for Juffair Heights)
-- ═══════════════════════════════════════════════════════════
SET IDENTITY_INSERT [Units] ON;

-- A-01: 3-Bedroom, Leased
IF NOT EXISTS (SELECT 1 FROM Units WHERE UnitId = 1001)
    INSERT INTO Units (UnitId, PropertyId, UnitNumber, UnitType, Amenities, SizeSqm, MonthlyRent, [Status], [Description])
    VALUES (1001, 1001, 'A-01', 3,
            'AC, 3 balconies, Fully furnished kitchen, Covered parking, Sea view',
            120.00, 850.00, 1,
            'Third-floor 3BR apartment with panoramic sea view.');

-- A-02: Studio, Available
IF NOT EXISTS (SELECT 1 FROM Units WHERE UnitId = 1002)
    INSERT INTO Units (UnitId, PropertyId, UnitNumber, UnitType, Amenities, SizeSqm, MonthlyRent, [Status], [Description])
    VALUES (1002, 1001, 'A-02', 0,
            'AC, Compact kitchen, High-speed internet, Building gym access',
            35.00, 320.00, 0,
            'Compact studio ideal for working professionals.');

-- B-01: 2-Bedroom, Under Maintenance (water heater burst)
IF NOT EXISTS (SELECT 1 FROM Units WHERE UnitId = 1003)
    INSERT INTO Units (UnitId, PropertyId, UnitNumber, UnitType, Amenities, SizeSqm, MonthlyRent, [Status], [Description])
    VALUES (1003, 1001, 'B-01', 2,
            'AC, Balcony, Furnished kitchen, Storage room',
            85.00, 580.00, 2,
            'Second-floor 2BR — currently under maintenance (water heater replacement).');

SET IDENTITY_INSERT [Units] OFF;


-- ═══════════════════════════════════════════════════════════
-- 3. NEW TENANTS  (IDs 1001-1005)
-- ═══════════════════════════════════════════════════════════
SET IDENTITY_INSERT [Tenants] ON;

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1001)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1001, 'Khalid bin Faisal Al Rumaihi',    'khalid.rumaihi@example.bh',  '+97333661122', '880312345', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1002)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1002, 'Mariam bint Yousif Al Kooheji',   'mariam.kooheji@example.bh',  '+97333772233', '920745678', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1003)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1003, 'Abdullah bin Jassim Al Baker',    'abdullah.baker@example.bh',  '+97333883344', '950218765', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1004)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1004, 'Latifa bint Hassan Al Maliki',    'latifa.maliki@example.bh',   '+97333994455', '890567890', NULL);

IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 1005)
    INSERT INTO Tenants (TenantId, FullName, Email, Phone, NationalId, UserId)
    VALUES (1005, 'Tariq bin Omar Al Buainain',      'tariq.buainain@example.bh',  '+97333115566', '970134567', NULL);

SET IDENTITY_INSERT [Tenants] OFF;


-- ═══════════════════════════════════════════════════════════
-- 4. UPDATE UNIT & PROPERTY STATUSES to match leases below
-- ═══════════════════════════════════════════════════════════

-- Units with active leases → Leased
UPDATE Units SET [Status] = 1 WHERE UnitId IN (1, 2, 3, 4, 6, 1001);
-- Riffa G2 lease expired → Available
UPDATE Units SET [Status] = 0 WHERE UnitId = 5;
-- Juffair B-01 water heater → UnderMaintenance
UPDATE Units SET [Status] = 2 WHERE UnitId = 1003;

-- Al Fateh Villa has an active standalone lease → Leased
UPDATE Properties SET [Status] = 1 WHERE PropertyId = 2;
-- Hamad Town Villa is new, no active lease → Available
UPDATE Properties SET [Status] = 0 WHERE PropertyId = 1002;


-- ═══════════════════════════════════════════════════════════
-- 5. LEASES  (IDs 1001-1012)
-- ═══════════════════════════════════════════════════════════
SET IDENTITY_INSERT [Leases] ON;

-- 1001 · Pearl Blvd Unit 101 → Khalid · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1001)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1001, 1, 1, 1001, '2025-02-01', '2026-02-01', 450.00, 4,
            '2025-01-10', '2025-01-20',
            'Long-term residency preferred.',
            'Employment verified at Alba. Credit check passed.');

-- 1002 · Pearl Blvd Unit 102 → Mariam · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1002)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1002, 1, 2, 1002, '2025-04-01', '2026-04-01', 470.00, 4,
            '2025-03-12', '2025-03-22',
            'Single occupant, professional.',
            'Government employee. References confirmed.');

-- 1003 · Pearl Blvd Unit 201 → Abdullah · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1003)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1003, 1, 3, 1003, '2025-07-01', '2026-07-01', 650.00, 4,
            '2025-06-15', '2025-06-25',
            'Family of 3 — requires 2BR.',
            'Income statement and family ID verified.');

-- 1004 · Riffa G1 (Shop) → Ahmed · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1004)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1004, 3, 4, 1, '2025-03-01', '2026-03-01', 380.00, 4,
            '2025-02-08', '2025-02-18',
            'Retail business — mobile accessories.',
            'Commercial registration verified. Trade licence confirmed.');

-- 1005 · Riffa F1-Suite-7 (Office) → Latifa · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1005)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1005, 3, 6, 1004, '2025-05-01', '2026-05-01', 550.00, 4,
            '2025-04-10', '2025-04-20',
            'Law firm — 3 staff members.',
            'CR and trade licence verified. References checked.');

-- 1006 · Al Fateh Villa (standalone) → Tariq · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1006)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1006, 2, NULL, 1005, '2025-01-01', '2026-01-01', 900.00, 4,
            '2024-12-10', '2024-12-20',
            'Family of 5. Long-term stay preferred.',
            'Bank statement and salary slip verified. Good rental history.');

-- 1007 · Juffair Heights A-01 → Hamad · ACTIVE
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1007)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1007, 1001, 1001, 102, '2025-06-01', '2026-06-01', 850.00, 4,
            '2025-05-15', '2025-05-25',
            'Prefers sea-view unit. Interested in long-term.',
            'Employment letter from Bapco. Income well above threshold.');

-- 1008 · Riffa G2 (Shop) → Noor · EXPIRED
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1008)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1008, 3, 5, 103, '2024-01-01', '2025-01-01', 420.00, 6,
            '2023-12-05', '2023-12-15',
            'Seasonal retail — gifts and home decor.',
            'Trade licence presented. Deposit paid upfront.');

-- 1009 · Hamad Town Villa (standalone) → Sara · APPLICATION
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1009)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1009, 1002, NULL, 2, '2026-05-01', '2027-05-01', 750.00, 0,
            '2026-04-20', NULL,
            'Interested in 1-year lease with renewal option. Viewing completed.',
            NULL);

-- 1010 · Juffair Studio A-02 → Fatima · SCREENING
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1010)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1010, 1001, 1002, 101, '2026-05-01', '2027-05-01', 320.00, 1,
            '2026-04-15', NULL,
            'Studio needed for work placement — 12-month contract.',
            'Awaiting employment confirmation letter from employer.');

-- 1011 · Juffair Studio A-02 → Noor · REJECTED (earlier applicant)
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1011)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1011, 1001, 1002, 103, '2025-09-01', '2026-09-01', 320.00, 3,
            '2025-08-20', NULL,
            'Looking for affordable unit close to work.',
            'Income insufficient for rent-to-income ratio requirement.');

-- 1012 · Pearl Blvd Unit 101 → Ahmed · EXPIRED (historical, before Khalid)
IF NOT EXISTS (SELECT 1 FROM Leases WHERE LeaseId = 1012)
    INSERT INTO Leases (LeaseId, PropertyId, UnitId, TenantId, StartDate, EndDate, MonthlyRent, [Status],
                        ApplicationDate, ApprovalDate, ApplicationNotes, ScreeningNotes)
    VALUES (1012, 1, 1, 1, '2024-01-01', '2025-01-01', 430.00, 6,
            '2023-12-01', '2023-12-10',
            'First-time lease application.',
            'ID and salary slip verified. Background check clear.');

SET IDENTITY_INSERT [Leases] OFF;
DBCC CHECKIDENT ('Leases', RESEED, 2000);


-- ═══════════════════════════════════════════════════════════
-- 6. PAYMENTS  (IDs 1001-1055)
-- ═══════════════════════════════════════════════════════════
SET IDENTITY_INSERT [Payments] ON;

-- ── Lease 1001 · Pearl 101 · Khalid · 450 BD/mo (Feb 2025 – Feb 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1001)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1001, 1001, 900.00,  '2025-02-01', '2025-01-30', 1, 1); -- Deposit · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1002)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1002, 1001, 450.00,  '2025-02-01', '2025-02-01', 0, 1); -- Feb · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1003)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1003, 1001, 450.00,  '2025-03-01', '2025-03-02', 0, 1); -- Mar · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1004)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1004, 1001, 450.00,  '2025-04-01', '2025-04-01', 0, 1); -- Apr · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1005)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1005, 1001, 450.00,  '2025-05-01', '2025-05-03', 0, 1); -- May · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1006)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1006, 1001, 50.00,   '2025-05-15', '2025-05-20', 2, 1); -- Late fine · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1007)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1007, 1001, 450.00,  '2025-06-01', '2025-06-01', 0, 1); -- Jun · Completed

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1008)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1008, 1001, 450.00,  '2026-03-01', NULL,         0, 2); -- Mar 26 · OVERDUE

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1009)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1009, 1001, 450.00,  '2026-04-01', NULL,         0, 0); -- Apr 26 · Pending

-- ── Lease 1002 · Pearl 102 · Mariam · 470 BD/mo (Apr 2025 – Apr 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1010)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1010, 1002, 940.00,  '2025-04-01', '2025-03-30', 1, 1); -- Deposit · Completed

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
    VALUES (1014, 1002, 470.00,  '2026-04-01', NULL,         0, 0); -- Pending

-- ── Lease 1003 · Pearl 201 · Abdullah · 650 BD/mo (Jul 2025 – Jul 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1015)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1015, 1003, 1300.00, '2025-07-01', '2025-06-28', 1, 1); -- Deposit

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
    VALUES (1019, 1003, 650.00,  '2026-04-01', NULL,         0, 0); -- Pending

-- ── Lease 1004 · Riffa G1 · Ahmed · 380 BD/mo (Mar 2025 – Mar 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1020)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1020, 1004, 760.00,  '2025-03-01', '2025-02-28', 1, 1); -- Deposit

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
    VALUES (1024, 1004, 380.00,  '2026-02-01', NULL,         0, 2); -- OVERDUE

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1025)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1025, 1004, 380.00,  '2026-03-01', NULL,         0, 0); -- Pending

-- ── Lease 1005 · Riffa Suite-7 · Latifa · 550 BD/mo (May 2025 – May 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1026)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1026, 1005, 1100.00, '2025-05-01', '2025-04-29', 1, 1); -- Deposit

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
    VALUES (1030, 1005, 550.00,  '2026-04-01', NULL,         0, 0); -- Pending

-- ── Lease 1006 · Al Fateh Villa · Tariq · 900 BD/mo (Jan 2025 – Jan 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1031)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1031, 1006, 1800.00, '2025-01-01', '2024-12-30', 1, 1); -- Deposit

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
    VALUES (1036, 1006, 900.00,  '2025-12-01', NULL,         0, 2); -- OVERDUE

IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1037)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1037, 1006, 900.00,  '2026-04-01', NULL,         0, 0); -- Pending

-- ── Lease 1007 · Juffair A-01 · Hamad · 850 BD/mo (Jun 2025 – Jun 2026) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1038)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1038, 1007, 1700.00, '2025-06-01', '2025-05-30', 1, 1); -- Deposit

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
    VALUES (1042, 1007, 850.00,  '2026-04-01', NULL,         0, 0); -- Pending

-- ── Lease 1008 · Riffa G2 · Noor · EXPIRED (all completed) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1043)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1043, 1008, 840.00,  '2024-01-01', '2023-12-29', 1, 1); -- Deposit

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

-- ── Lease 1012 · Pearl 101 · Ahmed · EXPIRED historical (all completed) ──
IF NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentId = 1049)
    INSERT INTO Payments (PaymentId, LeaseId, Amount, DueDate, PaymentDate, PaymentType, [Status])
    VALUES (1049, 1012, 860.00,  '2024-01-01', '2023-12-28', 1, 1); -- Deposit

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


-- ═══════════════════════════════════════════════════════════
-- 7. MAINTENANCE REQUESTS  (IDs 1001-1008)
-- ═══════════════════════════════════════════════════════════
DECLARE @StaffId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'staff@property.com');

SET IDENTITY_INSERT [MaintenanceRequests] ON;

-- 1001 · Pearl Blvd Unit 1 · AC not cooling · HVAC · Medium · Submitted
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1001)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1001, 1, 1, 1001,
            'AC Not Cooling Properly',
            'The air conditioning unit in the living room is running but not producing cold air. Temperature stays above 28°C even on maximum setting.',
            2, 1, 0, NULL, NULL, '2026-04-10', NULL, NULL);

-- 1002 · Pearl Blvd Unit 2 · Sink leak · Plumbing · High · Assigned
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1002)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1002, 1, 2, 1002,
            'Water Leak Under Kitchen Sink',
            'Slow drip from the pipe joint under the kitchen sink. Water is pooling in the cabinet below and causing minor water damage.',
            0, 2, 1, @StaffId,
            'Will replace the joint seal and inspect surrounding pipes for corrosion.',
            '2026-04-08', '2026-04-09', NULL);

-- 1003 · Pearl Blvd Unit 3 · Broken light fitting · Electrical · Low · Resolved
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1003)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1003, 1, 3, 1003,
            'Bedroom Ceiling Light Not Working',
            'The ceiling light fixture in the master bedroom stopped working. Bulb was already replaced by tenant but still no light.',
            1, 0, 3, @StaffId,
            'Replaced faulty light fitting and checked wiring. Tested — confirmed working.',
            '2026-03-20', '2026-03-21', '2026-03-22');

-- 1004 · Riffa G1 · Roller shutter jammed · General · Medium · InProgress
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1004)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1004, 3, 4, 1,
            'Front Roller Shutter Jammed',
            'The front roller shutter is stuck halfway and cannot be fully opened or closed. This is directly affecting business operations during trading hours.',
            3, 1, 2, @StaffId,
            'Lubricated tracks and adjusted tension spring. Replacement spring part has been ordered — ETA 3 days.',
            '2026-04-12', '2026-04-13', NULL);

-- 1005 · Al Fateh Villa (standalone) · Irrigation broken · Plumbing · High · Submitted
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1005)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1005, 2, NULL, 1005,
            'Garden Irrigation System Failure',
            'The automated irrigation system has completely stopped working. Garden plants and grass are at risk given summer temperatures exceeding 40°C.',
            0, 2, 0, NULL, NULL, '2026-04-15', NULL, NULL);

-- 1006 · Juffair A-01 · Annual AC service · HVAC · Low · Closed
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1006)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1006, 1001, 1001, 102,
            'Annual AC Preventive Maintenance',
            'Requesting scheduled annual maintenance for the central AC unit as stipulated in the tenancy agreement (Section 4.2).',
            2, 0, 4, @StaffId,
            'Full AC service complete: filters cleaned, refrigerant topped up, electrical connections checked, all components tested — system running optimally.',
            '2025-12-01', '2025-12-05', '2025-12-06');

-- 1007 · Juffair B-01 (Unit 1003) · Water heater burst · Plumbing · Urgent · InProgress
--       (This is why Unit 1003 has Status = UnderMaintenance)
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1007)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1007, 1001, 1003, 102,
            'Water Heater Burst — Unit Uninhabitable',
            'The pressurised water heater tank has burst, causing significant water damage to the bathroom and utility room. Unit is currently uninhabitable and cannot be leased until repairs are complete.',
            0, 3, 2, @StaffId,
            'Water supply isolated at mains. Replacement 80L tank ordered. Industrial drying equipment installed. Estimated completion: 5 working days.',
            '2026-04-05', '2026-04-05', NULL);

-- 1008 · Riffa Suite-7 · Sparking socket · Electrical · High · Resolved
IF NOT EXISTS (SELECT 1 FROM MaintenanceRequests WHERE RequestId = 1008)
    INSERT INTO MaintenanceRequests
        (RequestId, PropertyId, UnitId, TenantId, Title, [Description], Category, Priority, [Status],
         AssignedStaffId, StaffNotes, DateSubmitted, DateAssigned, DateResolved)
    VALUES (1008, 3, 6, 1004,
            'Electrical Socket Sparking Near Server Rack',
            'One of the double power sockets near the server rack is producing visible sparks when equipment is plugged in. Immediate safety concern — devices have been unplugged.',
            1, 2, 3, @StaffId,
            'Faulty socket replaced and surrounding ring-circuit wiring inspected. No further faults found. Safe to use.',
            '2026-03-28', '2026-03-28', '2026-03-29');

SET IDENTITY_INSERT [MaintenanceRequests] OFF;
DBCC CHECKIDENT ('MaintenanceRequests', RESEED, 2000);


-- ═══════════════════════════════════════════════════════════
-- 8. NOTIFICATIONS  (IDs 1001-1008, for the manager account)
-- ═══════════════════════════════════════════════════════════
DECLARE @ManagerId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'manager@property.com');

SET IDENTITY_INSERT [Notifications] ON;

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1001)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1001, @ManagerId,
            'New Lease Application',
            'Sara bint Khalifa has submitted a lease application for Hamad Town Villa. Please review.',
            0, 0, '2026-04-20 09:00:00', '/Leases/Details/1009');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1002)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1002, @ManagerId,
            'Overdue Payment — Riffa G1',
            'Payment for Lease #1004 (Ahmed Al Mansoori, Riffa G1) is overdue. Due date was 1 Feb 2026.',
            2, 0, '2026-02-05 08:00:00', '/Payments');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1003)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1003, @ManagerId,
            'Urgent Maintenance — Juffair B-01',
            'Water heater burst in Juffair Heights Unit B-01. Unit is currently uninhabitable. Repair in progress.',
            1, 0, '2026-04-05 07:30:00', '/MaintenanceRequests/Details/1007');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1004)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1004, @ManagerId,
            'Lease Expiring — Al Fateh Villa',
            'Lease for Al Fateh Villa (Tariq Al Buainain) expires 1 Jan 2026. Consider issuing a renewal notice.',
            0, 1, '2025-12-01 10:00:00', '/Leases/Details/1006');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1005)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1005, @ManagerId,
            'Maintenance Resolved — Pearl Blvd Unit 201',
            'Bedroom ceiling light in Pearl Boulevard Unit 201 has been repaired and confirmed working by maintenance staff.',
            1, 1, '2026-03-22 14:00:00', '/MaintenanceRequests/Details/1003');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1006)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1006, @ManagerId,
            'Application In Screening — Juffair Studio',
            'Fatima Al Dosari application for Juffair Heights Studio A-02 is now in screening stage. Employment letter pending.',
            0, 1, '2026-04-16 11:00:00', '/Leases/Details/1010');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1007)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1007, @ManagerId,
            'Payment Received — Juffair A-01',
            'Monthly rent of 850.000 BD received for Juffair Heights A-01 (Hamad Al Mahmood). Aug 2025.',
            2, 1, '2025-08-01 09:15:00', '/Payments');

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE NotificationId = 1008)
    INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, IsRead, CreatedAt, LinkUrl)
    VALUES (1008, @ManagerId,
            'Overdue Payment — Al Fateh Villa',
            'Al Fateh Villa rent for Dec 2025 is overdue. Please contact tenant Tariq Al Buainain at +97333115566.',
            2, 0, '2026-01-05 08:00:00', '/Payments');

SET IDENTITY_INSERT [Notifications] OFF;
DBCC CHECKIDENT ('Notifications', RESEED, 2000);


COMMIT;

-- ─────────────────────────────────────────────────────────────
-- QUICK VERIFICATION (run separately after COMMIT)
-- ─────────────────────────────────────────────────────────────
-- SELECT 'Properties'        , COUNT(*) FROM Properties
-- UNION SELECT 'Units'       , COUNT(*) FROM Units
-- UNION SELECT 'Tenants'     , COUNT(*) FROM Tenants
-- UNION SELECT 'Leases'      , COUNT(*) FROM Leases
-- UNION SELECT 'Payments'    , COUNT(*) FROM Payments
-- UNION SELECT 'Maintenance' , COUNT(*) FROM MaintenanceRequests
-- UNION SELECT 'Notifications', COUNT(*) FROM Notifications
