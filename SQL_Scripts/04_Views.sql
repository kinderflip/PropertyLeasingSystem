-- ============================================
-- Property Leasing System - SQL Views
-- Database: PropertyLeasingDB (Azure SQL)
-- Project: IT8118 Advanced Programming - Brief B
-- ============================================

-- ============================================
-- View: Active Leases with Property and Tenant Details
-- ============================================
CREATE OR ALTER VIEW [dbo].[vw_ActiveLeases]
AS
    SELECT
        l.[LeaseId],
        p.[Address] AS PropertyAddress,
        p.[City] AS PropertyCity,
        u.[UnitNumber],
        CASE p.[PropertyType]
            WHEN 0 THEN 'Apartment'
            WHEN 1 THEN 'Villa'
            WHEN 2 THEN 'Shop'
            WHEN 3 THEN 'Office'
        END AS PropertyType,
        t.[FullName] AS TenantName,
        t.[Email] AS TenantEmail,
        t.[Phone] AS TenantPhone,
        l.[StartDate],
        l.[EndDate],
        l.[MonthlyRent],
        DATEDIFF(MONTH, l.[StartDate], l.[EndDate]) AS LeaseDurationMonths,
        DATEDIFF(DAY, GETDATE(), l.[EndDate]) AS DaysRemaining,
        CASE l.[Status]
            WHEN 0 THEN 'Application'
            WHEN 1 THEN 'Screening'
            WHEN 2 THEN 'Approved'
            WHEN 3 THEN 'Rejected'
            WHEN 4 THEN 'Active'
            WHEN 5 THEN 'Renewal'
            WHEN 6 THEN 'Expired'
            WHEN 7 THEN 'Terminated'
        END AS StatusName
    FROM [Leases] l
    INNER JOIN [Properties] p ON l.[PropertyId] = p.[PropertyId]
    LEFT  JOIN [Units] u      ON l.[UnitId]     = u.[UnitId]
    INNER JOIN [Tenants] t    ON l.[TenantId]   = t.[TenantId]
    WHERE l.[Status] = 4;  -- Active
GO

-- ============================================
-- View: Overdue Payments Summary
-- ============================================
CREATE OR ALTER VIEW [dbo].[vw_OverduePayments]
AS
    SELECT
        pay.[PaymentId],
        pay.[Amount],
        pay.[DueDate],
        DATEDIFF(DAY, pay.[DueDate], GETDATE()) AS DaysOverdue,
        CASE pay.[PaymentType]
            WHEN 0 THEN 'Rent'
            WHEN 1 THEN 'Deposit'
            WHEN 2 THEN 'Fine'
        END AS PaymentType,
        t.[FullName] AS TenantName,
        t.[Phone] AS TenantPhone,
        p.[Address] AS PropertyAddress,
        u.[UnitNumber],
        l.[LeaseId]
    FROM [Payments] pay
    INNER JOIN [Leases] l    ON pay.[LeaseId] = l.[LeaseId]
    INNER JOIN [Tenants] t   ON l.[TenantId]  = t.[TenantId]
    INNER JOIN [Properties] p ON l.[PropertyId] = p.[PropertyId]
    LEFT  JOIN [Units] u     ON l.[UnitId]     = u.[UnitId]
    WHERE pay.[Status] = 2;  -- Overdue
GO

-- ============================================
-- View: Open Maintenance Requests
-- ============================================
CREATE OR ALTER VIEW [dbo].[vw_OpenMaintenanceRequests]
AS
    SELECT
        m.[RequestId],
        m.[Title],
        m.[Description],
        CASE m.[Category]
            WHEN 0 THEN 'Plumbing'
            WHEN 1 THEN 'Electrical'
            WHEN 2 THEN 'HVAC'
            WHEN 3 THEN 'General'
        END AS Category,
        CASE m.[Priority]
            WHEN 0 THEN 'Low'
            WHEN 1 THEN 'Medium'
            WHEN 2 THEN 'High'
            WHEN 3 THEN 'Urgent'
        END AS Priority,
        CASE m.[Status]
            WHEN 0 THEN 'Submitted'
            WHEN 1 THEN 'Assigned'
            WHEN 2 THEN 'In Progress'
        END AS Status,
        m.[DateSubmitted],
        DATEDIFF(DAY, m.[DateSubmitted], GETDATE()) AS DaysOpen,
        p.[Address] AS PropertyAddress,
        un.[UnitNumber],
        t.[FullName] AS TenantName,
        staff.[FullName] AS AssignedStaffName
    FROM [MaintenanceRequests] m
    INNER JOIN [Properties] p    ON m.[PropertyId] = p.[PropertyId]
    LEFT  JOIN [Units] un        ON m.[UnitId]     = un.[UnitId]
    INNER JOIN [Tenants] t       ON m.[TenantId]   = t.[TenantId]
    LEFT  JOIN [AspNetUsers] staff ON m.[AssignedStaffId] = staff.[Id]
    WHERE m.[Status] IN (0, 1, 2);  -- Submitted, Assigned, InProgress
GO

-- ============================================
-- View: Property Revenue Summary
-- ============================================
CREATE OR ALTER VIEW [dbo].[vw_PropertyRevenueSummary]
AS
    SELECT
        p.[PropertyId],
        p.[Address],
        p.[City],
        CASE WHEN EXISTS (SELECT 1 FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId])
             THEN 'Multi-unit' ELSE 'Standalone' END AS Mode,
        ISNULL(p.[MonthlyRent], (SELECT SUM(u.[MonthlyRent]) FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId])) AS ListedRent,
        (SELECT COUNT(*) FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId]) AS UnitsCount,
        CASE p.[Status]
            WHEN 0 THEN 'Available'
            WHEN 1 THEN 'Leased'
            WHEN 2 THEN 'Under Maintenance'
            ELSE   'Managed-by-units'
        END AS PropertyStatus,
        (SELECT COUNT(*) FROM [Leases] l WHERE l.[PropertyId] = p.[PropertyId]) AS TotalLeases,
        ISNULL((SELECT SUM(pay.[Amount]) FROM [Leases] l
                INNER JOIN [Payments] pay ON l.[LeaseId] = pay.[LeaseId]
                WHERE l.[PropertyId] = p.[PropertyId] AND pay.[Status] = 1), 0) AS TotalCollected,
        ISNULL((SELECT SUM(pay.[Amount]) FROM [Leases] l
                INNER JOIN [Payments] pay ON l.[LeaseId] = pay.[LeaseId]
                WHERE l.[PropertyId] = p.[PropertyId] AND pay.[Status] IN (0, 2)), 0) AS TotalOutstanding
    FROM [Properties] p;
GO

-- ============================================
-- View: Tenant Payment History
-- ============================================
CREATE OR ALTER VIEW [dbo].[vw_TenantPaymentHistory]
AS
    SELECT
        t.[TenantId],
        t.[FullName] AS TenantName,
        t.[Email],
        t.[Phone],
        COUNT(pay.[PaymentId]) AS TotalPayments,
        ISNULL(SUM(pay.[Amount]), 0) AS TotalAmount,
        ISNULL(SUM(CASE WHEN pay.[Status] = 1 THEN pay.[Amount] ELSE 0 END), 0) AS TotalPaid,
        ISNULL(SUM(CASE WHEN pay.[Status] = 2 THEN pay.[Amount] ELSE 0 END), 0) AS TotalOverdue,
        COUNT(CASE WHEN pay.[Status] = 2 THEN 1 END) AS OverdueCount
    FROM [Tenants] t
    LEFT JOIN [Leases] l ON t.[TenantId] = l.[TenantId]
    LEFT JOIN [Payments] pay ON l.[LeaseId] = pay.[LeaseId]
    GROUP BY t.[TenantId], t.[FullName], t.[Email], t.[Phone];
GO

PRINT 'Views created successfully.';
GO
