-- ============================================
-- Property Leasing System - Stored Procedures
-- Database: PropertyLeasingDB (Azure SQL)
-- Project: IT8118 Advanced Programming - Brief B
-- ============================================

-- ============================================
-- SP: Get Property Occupancy Summary
-- Returns count and percentage of properties by status
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPropertyOccupancySummary]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CASE [Status]
            WHEN 0 THEN 'Available'
            WHEN 1 THEN 'Leased'
            WHEN 2 THEN 'Under Maintenance'
        END AS StatusName,
        COUNT(*) AS PropertyCount,
        CAST(COUNT(*) * 100.0 / NULLIF((SELECT COUNT(*) FROM [Properties]), 0) AS DECIMAL(5,2)) AS Percentage,
        SUM([MonthlyRent]) AS TotalRent,
        AVG([MonthlyRent]) AS AverageRent
    FROM [Properties]
    GROUP BY [Status]
    ORDER BY [Status];
END
GO

-- ============================================
-- SP: Get Overdue Payments with Tenant Details
-- Returns all overdue payments with tenant and property info
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetOverduePayments]
AS
BEGIN
    SET NOCOUNT ON;

    -- First, auto-flag overdue payments
    UPDATE [Payments]
    SET [Status] = 2  -- Overdue
    WHERE [Status] = 0  -- Pending
      AND [DueDate] < CAST(GETDATE() AS DATE);

    -- Then return them with details
    SELECT
        p.[PaymentId],
        p.[Amount],
        p.[DueDate],
        DATEDIFF(DAY, p.[DueDate], GETDATE()) AS DaysOverdue,
        t.[FullName] AS TenantName,
        t.[Phone] AS TenantPhone,
        t.[Email] AS TenantEmail,
        pr.[Address] AS PropertyAddress,
        pr.[City] AS PropertyCity,
        l.[LeaseId]
    FROM [Payments] p
    INNER JOIN [Leases] l ON p.[LeaseId] = l.[LeaseId]
    INNER JOIN [Tenants] t ON l.[TenantId] = t.[TenantId]
    INNER JOIN [Properties] pr ON l.[PropertyId] = pr.[PropertyId]
    WHERE p.[Status] = 2
    ORDER BY p.[DueDate] ASC;
END
GO

-- ============================================
-- SP: Get Maintenance Resolution Statistics
-- Returns avg resolution time and counts by category/priority
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetMaintenanceStatistics]
AS
BEGIN
    SET NOCOUNT ON;

    -- Overall stats
    SELECT
        COUNT(*) AS TotalRequests,
        SUM(CASE WHEN [Status] IN (0, 1, 2) THEN 1 ELSE 0 END) AS OpenRequests,
        SUM(CASE WHEN [Status] IN (3, 4) THEN 1 ELSE 0 END) AS ResolvedRequests,
        AVG(CASE
            WHEN [DateResolved] IS NOT NULL
            THEN CAST(DATEDIFF(DAY, [DateSubmitted], [DateResolved]) AS FLOAT)
        END) AS AvgResolutionDays
    FROM [MaintenanceRequests];

    -- By category
    SELECT
        CASE [Category]
            WHEN 0 THEN 'Plumbing'
            WHEN 1 THEN 'Electrical'
            WHEN 2 THEN 'HVAC'
            WHEN 3 THEN 'General'
        END AS CategoryName,
        COUNT(*) AS TotalCount,
        SUM(CASE WHEN [Status] IN (0, 1, 2) THEN 1 ELSE 0 END) AS OpenCount,
        SUM(CASE WHEN [Status] IN (3, 4) THEN 1 ELSE 0 END) AS ResolvedCount
    FROM [MaintenanceRequests]
    GROUP BY [Category]
    ORDER BY [Category];

    -- By priority
    SELECT
        CASE [Priority]
            WHEN 0 THEN 'Low'
            WHEN 1 THEN 'Medium'
            WHEN 2 THEN 'High'
            WHEN 3 THEN 'Urgent'
        END AS PriorityName,
        COUNT(*) AS TotalCount,
        AVG(CASE
            WHEN [DateResolved] IS NOT NULL
            THEN CAST(DATEDIFF(DAY, [DateSubmitted], [DateResolved]) AS FLOAT)
        END) AS AvgResolutionDays
    FROM [MaintenanceRequests]
    GROUP BY [Priority]
    ORDER BY [Priority];
END
GO

-- ============================================
-- SP: Get Lease Lifecycle Summary
-- Returns lease counts and revenue by status
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetLeaseSummary]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CASE [Status]
            WHEN 0 THEN 'Application'
            WHEN 1 THEN 'Screening'
            WHEN 2 THEN 'Approved'
            WHEN 3 THEN 'Rejected'
            WHEN 4 THEN 'Active'
            WHEN 5 THEN 'Renewal'
            WHEN 6 THEN 'Expired'
            WHEN 7 THEN 'Terminated'
        END AS StatusName,
        COUNT(*) AS LeaseCount,
        SUM([MonthlyRent]) AS TotalMonthlyRent,
        AVG(DATEDIFF(MONTH, [StartDate], [EndDate])) AS AvgDurationMonths
    FROM [Leases]
    GROUP BY [Status]
    ORDER BY [Status];
END
GO

-- ============================================
-- SP: Get Monthly Revenue Report
-- Returns monthly payment collection totals
-- ============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_GetMonthlyRevenue]
    @Year INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Year IS NULL
        SET @Year = YEAR(GETDATE());

    SELECT
        MONTH(p.[DueDate]) AS MonthNumber,
        DATENAME(MONTH, p.[DueDate]) AS MonthName,
        SUM(p.[Amount]) AS TotalDue,
        SUM(CASE WHEN p.[Status] = 1 THEN p.[Amount] ELSE 0 END) AS TotalCollected,
        SUM(CASE WHEN p.[Status] = 2 THEN p.[Amount] ELSE 0 END) AS TotalOverdue,
        SUM(CASE WHEN p.[Status] = 0 THEN p.[Amount] ELSE 0 END) AS TotalPending,
        COUNT(*) AS PaymentCount
    FROM [Payments] p
    WHERE YEAR(p.[DueDate]) = @Year
    GROUP BY MONTH(p.[DueDate]), DATENAME(MONTH, p.[DueDate])
    ORDER BY MONTH(p.[DueDate]);
END
GO

PRINT 'Stored procedures created successfully.';
GO
