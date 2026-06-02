using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyLeasingAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 1,
                column: "Description",
                value: "Pearl Boulevard Residences");

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 2,
                column: "Description",
                value: "Al Fateh Villa - standalone family villa");

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 3,
                column: "Description",
                value: "Riffa Commercial Centre");

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 1,
                column: "Description",
                value: "First-floor 1BR apartment.");

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 2,
                column: "Description",
                value: "First-floor 1BR apartment.");

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 3,
                columns: new[] { "Amenities", "Description" },
                values: new object[] { "AC, 2 balconies, Furnished kitchen", "Second-floor 2BR apartment." });

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 5,
                columns: new[] { "Amenities", "Description" },
                values: new object[] { "Street-facing shopfront, AC", "Ground-floor retail unit." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 1,
                column: "Description",
                value: "Pearl Boulevard Residences — modern apartment building near Seef Mall.");

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 2,
                column: "Description",
                value: "Al Fateh Villa — spacious standalone family villa with private garden.");

            migrationBuilder.UpdateData(
                table: "Properties",
                keyColumn: "PropertyId",
                keyValue: 3,
                column: "Description",
                value: "Riffa Commercial Centre — mixed-use office and retail building.");

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 1,
                column: "Description",
                value: "First-floor 1BR apartment facing the pool.");

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 2,
                column: "Description",
                value: "First-floor 1BR apartment with garden view.");

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 3,
                columns: new[] { "Amenities", "Description" },
                values: new object[] { "AC, 2 balconies, Furnished kitchen, Storage room", "Second-floor 2BR apartment, corner unit." });

            migrationBuilder.UpdateData(
                table: "Units",
                keyColumn: "UnitId",
                keyValue: 5,
                columns: new[] { "Amenities", "Description" },
                values: new object[] { "Street-facing shopfront, AC, Back storeroom", "Ground-floor retail unit next to the main entrance." });
        }
    }
}
