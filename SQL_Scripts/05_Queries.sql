-- ============================================
-- Property Leasing System - Useful Queries
-- Database: PropertyLeasingDB (Azure SQL)
-- Project: IT8118 Advanced Programming - Brief B
-- ============================================

-- ============================================
-- 1. Dashboard Summary Query  (multi-unit + standalone aware)
-- Available == standalone with Status=0, OR has any unit with Status=0.
-- Leased    == standalone-leased + leased units.
-- ============================================
SELECT
    (SELECT COUNT(*) FROM [Properties]) AS TotalProperties,
    (SELECT COUNT(*) FROM [Properties] p
      WHERE (p.[Status] = 0 AND NOT EXISTS (SELECT 1 FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId]))
         OR EXISTS (SELECT 1 FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId] AND u.[Status] = 0)
    ) AS AvailableProperties,
    (
        (SELECT COUNT(*) FROM [Properties] p
           WHERE p.[Status] = 1 AND NOT EXISTS (SELECT 1 FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId]))
      + (SELECT COUNT(*) FROM [Units] WHERE [Status] = 1)
    ) AS LeasedRentables,
    (SELECT COUNT(*) FROM [Units]) AS TotalUnits,
    (SELECT COUNT(*) FROM [Tenants]) AS TotalTenants,
    (SELECT COUNT(*) FROM [Leases] WHERE [Status] = 4) AS ActiveLeases,
    (SELECT COUNT(*) FROM [Leases] WHERE [Status] IN (0, 1)) AS PendingApplications,
    (SELECT COUNT(*) FROM [MaintenanceRequests] WHERE [Status] IN (0, 1, 2)) AS OpenMaintenanceRequests,
    (SELECT COUNT(*) FROM [Payments] WHERE [Status] = 2) AS OverduePayments,
    (SELECT ISNULL(SUM([Amount]), 0) FROM [Payments] WHERE [Status] = 1) AS TotalRevenue;

-- ============================================
-- 1b. Available Rentables (standalone properties + individual units)
-- Matches the tenant "browse" experience.
-- ============================================
SELECT
    p.[PropertyId],
    p.[Address],
    p.[City],
    'Standalone' AS Mode,
    NULL AS UnitNumber,
    p.[MonthlyRent]
FROM [Properties] p
WHERE p.[Status] = 0
  AND NOT EXISTS (SELECT 1 FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId])
UNION ALL
SELECT
    p.[PropertyId],
    p.[Address],
    p.[City],
    'Unit' AS Mode,
    u.[UnitNumber],
    u.[MonthlyRent]
FROM [Units] u
INNER JOIN [Properties] p ON u.[PropertyId] = p.[PropertyId]
WHERE u.[Status] = 0
ORDER BY [City], [Address];

-- ============================================
-- 2. Leases Expiring Within 30 Days
-- Helps property managers plan renewals
-- ============================================
SELECT
    l.[LeaseId],
    p.[Address],
    u.[UnitNumber],
    t.[FullName] AS TenantName,
    l.[EndDate],
    DATEDIFF(DAY, GETDATE(), l.[EndDate]) AS DaysUntilExpiry,
    l.[MonthlyRent]
FROM [Leases] l
INNER JOIN [Properties] p ON l.[PropertyId] = p.[PropertyId]
LEFT  JOIN [Units] u      ON l.[UnitId]     = u.[UnitId]
INNER JOIN [Tenants] t    ON l.[TenantId]   = t.[TenantId]
WHERE l.[Status] = 4
  AND l.[EndDate] BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE())
ORDER BY l.[EndDate];

-- ============================================
-- 3. Urgent/High Priority Open Maintenance Requests
-- ============================================
SELECT
    m.[RequestId],
    m.[Title],
    CASE m.[Priority] WHEN 2 THEN 'High' WHEN 3 THEN 'Urgent' END AS Priority,
    CASE m.[Status] WHEN 0 THEN 'Submitted' WHEN 1 THEN 'Assigned' WHEN 2 THEN 'In Progress' END AS Status,
    p.[Address] AS PropertyAddress,
    u.[UnitNumber],
    t.[FullName] AS TenantName,
    m.[DateSubmitted],
    DATEDIFF(DAY, m.[DateSubmitted], GETDATE()) AS DaysOpen
FROM [MaintenanceRequests] m
INNER JOIN [Properties] p ON m.[PropertyId] = p.[PropertyId]
LEFT  JOIN [Units] u      ON m.[UnitId]     = u.[UnitId]
INNER JOIN [Tenants] t    ON m.[TenantId]   = t.[TenantId]
WHERE m.[Status] IN (0, 1, 2)
  AND m.[Priority] IN (2, 3)
ORDER BY m.[Priority] DESC, m.[DateSubmitted] ASC;

-- ============================================
-- 4. Payment Collection Rate by Month (Current Year)
-- ============================================
SELECT
    DATENAME(MONTH, [DueDate]) AS MonthName,
    MONTH([DueDate]) AS MonthNum,
    COUNT(*) AS TotalPayments,
    SUM(CASE WHEN [Status] = 1 THEN 1 ELSE 0 END) AS PaidCount,
    SUM(CASE WHEN [Status] = 2 THEN 1 ELSE 0 END) AS OverdueCount,
    SUM([Amount]) AS TotalDue,
    SUM(CASE WHEN [Status] = 1 THEN [Amount] ELSE 0 END) AS TotalCollected,
    CAST(SUM(CASE WHEN [Status] = 1 THEN 1.0 ELSE 0 END) / NULLIF(COUNT(*), 0) * 100 AS DECIMAL(5,2)) AS CollectionRatePercent
FROM [Payments]
WHERE YEAR([DueDate]) = YEAR(GETDATE())
GROUP BY DATENAME(MONTH, [DueDate]), MONTH([DueDate])
ORDER BY MONTH([DueDate]);

-- ============================================
-- 5. Tenant Outstanding Balance
-- ============================================
SELECT
    t.[TenantId],
    t.[FullName],
    t.[Phone],
    COUNT(pay.[PaymentId]) AS OverduePayments,
    SUM(pay.[Amount]) AS TotalOutstanding,
    MIN(pay.[DueDate]) AS OldestOverdueDate
FROM [Tenants] t
INNER JOIN [Leases] l ON t.[TenantId] = l.[TenantId]
INNER JOIN [Payments] pay ON l.[LeaseId] = pay.[LeaseId]
WHERE pay.[Status] IN (0, 2)  -- Pending or Overdue
GROUP BY t.[TenantId], t.[FullName], t.[Phone]
HAVING SUM(pay.[Amount]) > 0
ORDER BY SUM(pay.[Amount]) DESC;

-- ============================================
-- 6. Property Performance Report  (multi-unit aware)
--   ListedRent = standalone rent, OR SUM of unit rents for multi-unit.
-- ============================================
SELECT
    p.[PropertyId],
    p.[Address],
    p.[City],
    CASE p.[PropertyType]
        WHEN 0 THEN 'Apartment'
        WHEN 1 THEN 'Villa'
        WHEN 2 THEN 'Shop'
        WHEN 3 THEN 'Office'
    END AS PropertyType,
    CASE WHEN EXISTS (SELECT 1 FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId])
         THEN 'Multi-unit' ELSE 'Standalone' END AS Mode,
    (SELECT COUNT(*) FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId]) AS UnitsCount,
    ISNULL(p.[MonthlyRent], (SELECT SUM(u.[MonthlyRent]) FROM [Units] u WHERE u.[PropertyId] = p.[PropertyId])) AS ListedRent,
    (SELECT COUNT(*) FROM [Leases] l WHERE l.[PropertyId] = p.[PropertyId]) AS TotalLeases,
    ISNULL((SELECT SUM(pay.[Amount]) FROM [Leases] l
            INNER JOIN [Payments] pay ON l.[LeaseId] = pay.[LeaseId]
            WHERE l.[PropertyId] = p.[PropertyId] AND pay.[Status] = 1), 0) AS RevenueCollected,
    (SELECT COUNT(*) FROM [MaintenanceRequests] m WHERE m.[PropertyId] = p.[PropertyId]) AS MaintenanceRequests
FROM [Properties] p
ORDER BY RevenueCollected DESC;
GO
