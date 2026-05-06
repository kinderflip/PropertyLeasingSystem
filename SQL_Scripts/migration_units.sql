BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [Leases] DROP CONSTRAINT [FK_Leases_Properties_PropertyId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [MaintenanceRequests] DROP CONSTRAINT [FK_MaintenanceRequests_Properties_PropertyId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Properties]') AND [c].[name] = N'Status');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Properties] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Properties] ALTER COLUMN [Status] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Properties]') AND [c].[name] = N'MonthlyRent');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Properties] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Properties] ALTER COLUMN [MonthlyRent] decimal(10,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Properties]') AND [c].[name] = N'Bedrooms');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Properties] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Properties] ALTER COLUMN [Bedrooms] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [MaintenanceRequests] ADD [UnitId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [Leases] ADD [UnitId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    CREATE TABLE [Units] (
        [UnitId] int NOT NULL IDENTITY,
        [PropertyId] int NOT NULL,
        [UnitNumber] nvarchar(20) NOT NULL,
        [UnitType] int NOT NULL,
        [Amenities] nvarchar(500) NULL,
        [SizeSqm] decimal(8,2) NOT NULL,
        [MonthlyRent] decimal(10,2) NOT NULL,
        [Status] int NOT NULL,
        [Description] nvarchar(500) NULL,
        CONSTRAINT [PK_Units] PRIMARY KEY ([UnitId]),
        CONSTRAINT [FK_Units_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([PropertyId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    EXEC(N'UPDATE [Properties] SET [Address] = N''Building 2455, Road 2832, Block 428, Seef District'', [Bedrooms] = NULL, [Description] = N''Pearl Boulevard Residences — modern apartment building near Seef Mall.'', [MonthlyRent] = NULL, [Status] = NULL
    WHERE [PropertyId] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    EXEC(N'UPDATE [Properties] SET [Address] = N''House 108, Road 3803, Block 338, Juffair'', [Description] = N''Al Fateh Villa — spacious standalone family villa with private garden.'', [MonthlyRent] = 900.0
    WHERE [PropertyId] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    EXEC(N'UPDATE [Properties] SET [Address] = N''Building 217, Road 2409, Block 924'', [Bedrooms] = NULL, [City] = N''East Riffa'', [Description] = N''Riffa Commercial Centre — mixed-use office and retail building.'', [MonthlyRent] = NULL, [PropertyType] = 3, [Status] = NULL
    WHERE [PropertyId] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    EXEC(N'UPDATE [Tenants] SET [Email] = N''ahmed.mansoori@example.bh'', [FullName] = N''Ahmed bin Mohammed Al Mansoori'', [NationalId] = N''870412345''
    WHERE [TenantId] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    EXEC(N'UPDATE [Tenants] SET [Email] = N''sara.khalifa@example.bh'', [FullName] = N''Sara bint Khalifa Al Khalifa'', [NationalId] = N''900823456''
    WHERE [TenantId] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TenantId', N'Email', N'FullName', N'NationalId', N'Phone', N'UserId') AND [object_id] = OBJECT_ID(N'[Tenants]'))
        SET IDENTITY_INSERT [Tenants] ON;
    EXEC(N'INSERT INTO [Tenants] ([TenantId], [Email], [FullName], [NationalId], [Phone], [UserId])
    VALUES (101, N''fatima.dosari@example.bh'', N''Fatima bint Isa Al Dosari'', N''920345678'', N''+97333778899'', NULL),
    (102, N''hamad.mahmood@example.bh'', N''Hamad bin Salman Al Mahmood'', N''850612345'', N''+97333224455'', NULL),
    (103, N''noor.zayani@example.bh'', N''Noor bint Ali Al Zayani'', N''940109876'', N''+97333557788'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TenantId', N'Email', N'FullName', N'NationalId', N'Phone', N'UserId') AND [object_id] = OBJECT_ID(N'[Tenants]'))
        SET IDENTITY_INSERT [Tenants] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UnitId', N'Amenities', N'Description', N'MonthlyRent', N'PropertyId', N'SizeSqm', N'Status', N'UnitNumber', N'UnitType') AND [object_id] = OBJECT_ID(N'[Units]'))
        SET IDENTITY_INSERT [Units] ON;
    EXEC(N'INSERT INTO [Units] ([UnitId], [Amenities], [Description], [MonthlyRent], [PropertyId], [SizeSqm], [Status], [UnitNumber], [UnitType])
    VALUES (1, N''AC, Balcony, Furnished kitchen'', N''First-floor 1BR apartment facing the pool.'', 450.0, 1, 60.0, 0, N''101'', 1),
    (2, N''AC, Balcony'', N''First-floor 1BR apartment with garden view.'', 470.0, 1, 62.0, 0, N''102'', 1),
    (3, N''AC, 2 balconies, Furnished kitchen, Storage room'', N''Second-floor 2BR apartment, corner unit.'', 650.0, 1, 95.0, 0, N''201'', 2),
    (4, N''Street-facing shopfront, AC'', N''Ground-floor retail unit.'', 380.0, 3, 45.0, 0, N''G1'', 5),
    (5, N''Street-facing shopfront, AC, Back storeroom'', N''Ground-floor retail unit next to the main entrance.'', 420.0, 3, 50.0, 0, N''G2'', 5),
    (6, N''AC, Private washroom, Pantry'', N''First-floor office suite.'', 550.0, 3, 70.0, 0, N''F1-Suite-7'', 4)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UnitId', N'Amenities', N'Description', N'MonthlyRent', N'PropertyId', N'SizeSqm', N'Status', N'UnitNumber', N'UnitType') AND [object_id] = OBJECT_ID(N'[Units]'))
        SET IDENTITY_INSERT [Units] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    CREATE INDEX [IX_Payments_Status_DueDate] ON [Payments] ([Status], [DueDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRequests_UnitId] ON [MaintenanceRequests] ([UnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    CREATE INDEX [IX_Leases_UnitId] ON [Leases] ([UnitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    CREATE INDEX [IX_Units_PropertyId] ON [Units] ([PropertyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    CREATE INDEX [IX_Units_Status] ON [Units] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [Leases] ADD CONSTRAINT [FK_Leases_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([PropertyId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [Leases] ADD CONSTRAINT [FK_Leases_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([UnitId]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [MaintenanceRequests] ADD CONSTRAINT [FK_MaintenanceRequests_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([PropertyId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    ALTER TABLE [MaintenanceRequests] ADD CONSTRAINT [FK_MaintenanceRequests_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([UnitId]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420171733_AddUnitsAndStandaloneSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260420171733_AddUnitsAndStandaloneSupport', N'9.0.14');
END;

COMMIT;
GO

