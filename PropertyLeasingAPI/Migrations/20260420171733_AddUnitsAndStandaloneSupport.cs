using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PropertyLeasingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsAndStandaloneSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyRent",
                table: "Properties",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Bedrooms",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "MaintenanceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Leases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitType = table.Column<int>(type: "int", nullable: false),
                    Amenities = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SizeSqm = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    MonthlyRent = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitId);
                    table.ForeignKey(
                        name: "FK_Units_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "PropertyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 1,
                columns: new[] { "Address", "Bedrooms", "Description", "MonthlyRent", "Status" },
                values: new object[] { "Building 2455, Road 2832, Block 428, Seef District", null, "Pearl Boulevard Residences — modern apartment building near Seef Mall.", null, null });

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 2,
                columns: new[] { "Address", "Description", "MonthlyRent" },
                values: new object[] { "House 108, Road 3803, Block 338, Juffair", "Al Fateh Villa — spacious standalone family villa with private garden.", 900m });

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 3,
                columns: new[] { "Address", "Bedrooms", "City", "Description", "MonthlyRent", "PropertyType", "Status" },
                values: new object[] { "Building 217, Road 2409, Block 924", null, "East Riffa", "Riffa Commercial Centre — mixed-use office and retail building.", null, 3, null });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 1,
                columns: new[] { "Email", "FullName", "NationalId" },
                values: new object[] { "ahmed.mansoori@example.bh", "Ahmed bin Mohammed Al Mansoori", "870412345" });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 2,
                columns: new[] { "Email", "FullName", "NationalId" },
                values: new object[] { "sara.khalifa@example.bh", "Sara bint Khalifa Al Khalifa", "900823456" });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantId", "Email", "FullName", "NationalId", "Phone", "UserId" },
                values: new object[,]
                {
                    { 101, "fatima.dosari@example.bh", "Fatima bint Isa Al Dosari", "920345678", "+97333778899", null },
                    { 102, "hamad.mahmood@example.bh", "Hamad bin Salman Al Mahmood", "850612345", "+97333224455", null },
                    { 103, "noor.zayani@example.bh", "Noor bint Ali Al Zayani", "940109876", "+97333557788", null }
                });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "UnitId", "Amenities", "Description", "MonthlyRent", "PropertyId", "SizeSqm", "Status", "UnitNumber", "UnitType" },
                values: new object[,]
                {
                    { 1, "AC, Balcony, Furnished kitchen", "First-floor 1BR apartment facing the pool.", 450m, 1, 60m, 0, "101", 1 },
                    { 2, "AC, Balcony", "First-floor 1BR apartment with garden view.", 470m, 1, 62m, 0, "102", 1 },
                    { 3, "AC, 2 balconies, Furnished kitchen, Storage room", "Second-floor 2BR apartment, corner unit.", 650m, 1, 95m, 0, "201", 2 },
                    { 4, "Street-facing shopfront, AC", "Ground-floor retail unit.", 380m, 3, 45m, 0, "G1", 5 },
                    { 5, "Street-facing shopfront, AC, Back storeroom", "Ground-floor retail unit next to the main entrance.", 420m, 3, 50m, 0, "G2", 5 },
                    { 6, "AC, Private washroom, Pantry", "First-floor office suite.", 550m, 3, 70m, 0, "F1-Suite-7", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_DueDate",
                table: "Payments",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_UnitId",
                table: "MaintenanceRequests",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_UnitId",
                table: "Leases",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_PropertyId",
                table: "Units",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Status",
                table: "Units",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Units_UnitId",
                table: "Leases",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Units_UnitId",
                table: "MaintenanceRequests",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases");

            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Units_UnitId",
                table: "Leases");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_Units_UnitId",
                table: "MaintenanceRequests");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_DueDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_UnitId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Leases_UnitId",
                table: "Leases");

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 103);

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Leases");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyRent",
                table: "Properties",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Bedrooms",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 1,
                columns: new[] { "Address", "Bedrooms", "Description", "MonthlyRent", "Status" },
                values: new object[] { "123 Pearl Boulevard", 2, "Modern apartment near the city center", 350m, 0 });

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 2,
                columns: new[] { "Address", "Description", "MonthlyRent" },
                values: new object[] { "45 Seef District", "Spacious villa with private garden", 800m });

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 3,
                columns: new[] { "Address", "Bedrooms", "City", "Description", "MonthlyRent", "PropertyType", "Status" },
                values: new object[] { "78 Riffa Valley Road", 0, "Riffa", "Commercial shop in busy area", 500m, 2, 0 });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 1,
                columns: new[] { "Email", "FullName", "NationalId" },
                values: new object[] { "ahmed@email.com", "Ahmed Al Mansoori", "880112345" });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 2,
                columns: new[] { "Email", "FullName", "NationalId" },
                values: new object[] { "sara@email.com", "Sara Al Khalifa", "920567890" });

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Properties_PropertyId",
                table: "Leases",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_Properties_PropertyId",
                table: "MaintenanceRequests",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "PropertyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
