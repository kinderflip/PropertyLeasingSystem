-- ============================================
-- Property Leasing System - Useful Queries
-- Database: PropertyLeasingDB (Azure SQL)
-- Project: IT8118 Advanced Programming - Brief B
-- ============================================

-- ============================================
-- 1. Dashboard Summary Query
-- Quick overview of all system metrics
-- ============================================
SELECT
    (SELECT COUNT(*) FROM [Properties]) AS TotalProperties,
    (SELECT COUNT(*) FROM [Properties] WHERE [Status] = 0) AS AvailableProperties,
    (SELECT COUNT(*) FROM [Properties] WHERE [Status] = 1) AS LeasedProperties,
    (SELECT COUNT(*) FROM [Tenants]) AS TotalTenants,
    (SELECT COUNT(*) FROM [Leases] WHERE [Status] = 4) AS ActiveLeases,
    (SELECT COUNT(*) FROM [Leases] WHERE [Status] IN (0, 1)) AS PendingApplications,
    (SELECT COUNT(*) FROM [MaintenanceRequests] WHERE [Status] IN (0, 1, 2)) AS OpenMaintenanceRequests,
    (SELECT COUNT(*) FROM [Payments] WHERE [Status] = 2) AS OverduePayments,
    (SELECT ISNULL(SUM([Amount]), 0) FROM [Payments] WHERE [Status] = 1) AS TotalRevenue;

-- ============================================
-- 2. Leases Expiring Within 30 Days
-- Helps property managers plan renewals
-- ============================================
SELECT
    l.[LeaseId],
    p.[Address],
    t.[FullName] AS TenantName,
    l.[EndDate],
    DATEDIFF(DAY, GETDATE(), l.[EndDate]) AS DaysUntilExpiry,
    l.[MonthlyRent]
FROM [Leases] l
INNER JOIN [Properties] p ON l.[PropertyId] = p.[PropertyId]
INNER JOIN [Tenants] t ON l.[TenantId] = t.[TenantId]
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
    t.[FullName] AS TenantName,
    m.[DateSubmitted],
    DATEDIFF(DAY, m.[DateSubmitted], GETDATE()) AS DaysOpen
FROM [MaintenanceRequests] m
INNER JOIN [Properties] p ON m.[PropertyId] = p.[PropertyId]
INNER JOIN [Tenants] t ON m.[TenantId] = t.[TenantId]
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
-- 6. Property Performance Report
-- Revenue generated per property
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
    p.[MonthlyRent] AS ListedRent,
    COUNT(DISTINCT l.[LeaseId]) AS TotalLeases,
    ISNULL(SUM(CASE WHEN pay.[Status] = 1 THEN pay.[Amount] END), 0) AS RevenueCollected,
    COUNT(DISTINCT m.[RequestId]) AS MaintenanceRequests
FROM [Properties] p
LEFT JOIN [Leases] l ON p.[PropertyId] = l.[PropertyId]
LEFT JOIN [Payments] pay ON l.[LeaseId] = pay.[LeaseId]
LEFT JOIN [MaintenanceRequests] m ON p.[PropertyId] = m.[PropertyId]
GROUP BY p.[PropertyId], p.[Address], p.[City], p.[PropertyType], p.[MonthlyRent]
ORDER BY RevenueCollected DESC;
GO
