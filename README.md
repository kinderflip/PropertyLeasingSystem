# Property Leasing System

IT8118 Advanced Programming — Brief B group project.
A three-project ASP.NET Core 9 solution:

- **PropertyLeasingAPI** — REST API secured with JWT
- **PropertyLeasingMVC** — staff/tenant-facing website
- **PropertyLeasingReports** — read-only manager reporting portal

## Deployed URLs

- API: https://propertyleasing-api-s8g4-dpf3dha4acf4e4gy.westeurope-01.azurewebsites.net
- MVC: https://propertyleasing-mvc-s8g4-amf0eugmfgdqemb3.westeurope-01.azurewebsites.net
- Reports: https://propertyleasing-reports-s8g4-cbdwg3h2h5fjg8b9.westeurope-01.azurewebsites.net

## Demo credentials

| Role | Email | Password |
|---|---|---|
| Property Manager | manager@property.com | Manager123 |
| Maintenance Staff | staff@property.com | Staff123 |
| Tenant | register a new account at /Account/Register | — |

## Running locally

The solution is configured to default to **LocalDB** out of the box. Visual Studio installs LocalDB automatically, so a fresh clone runs without extra setup.

1. Open `PropertyLeasingSystem.sln` in Visual Studio 2022 or later.
2. Right-click the solution → **Configure Startup Projects** → select **Multiple startup projects** and set **PropertyLeasingAPI**, **PropertyLeasingMVC**, and **PropertyLeasingReports** to **Start**.
3. In **Package Manager Console**, create the database the first time:
   ```
   Update-Database -Project PropertyLeasingAPI -StartupProject PropertyLeasingMVC
   ```
4. (Optional but recommended for a populated demo) Load the rich demo data:
   - Open `SQL_Scripts/rich_demo_data.sql` in Visual Studio
   - Connect to `(localdb)\MSSQLLocalDB` → database `PropertyLeasingDB`
   - Execute the script (`Ctrl+Shift+E`)
   - This adds extra leases, payments, maintenance requests, and notifications so the Reports app has data to show
5. Press **F5** to run all three projects.

The manager and staff login accounts are created automatically the first time the MVC project starts. Roles, three properties, six units, and five tenants come from the EF migration seed data. Leases, payments and maintenance history come from the optional rich demo script above.

## Running against the deployed Azure SQL

If you prefer to point the local apps at the same database the deployed apps use, replace `DefaultConnection` in `PropertyLeasingAPI/appsettings.Development.json` and `PropertyLeasingMVC/appsettings.Development.json` with the s8g4 connection string. You will also need to add your client IP to the Azure SQL firewall rules.

## Database scripts

The submission ZIP includes:

- `Schema.sql` — creates the tables, constraints, and relationships
- `SeedData.sql` — seeds Identity roles, demo users (with PBKDF2 password hashes), and business data

These are an alternative to running EF migrations.
