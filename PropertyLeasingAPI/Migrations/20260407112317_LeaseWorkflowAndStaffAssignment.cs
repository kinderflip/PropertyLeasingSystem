using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyLeasingAPI.Migrations
{
    /// <inheritdoc />
    public partial class LeaseWorkflowAndStaffAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedStaffId",
                table: "MaintenanceRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAssigned",
                table: "MaintenanceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "MaintenanceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StaffNotes",
                table: "MaintenanceRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "Leases",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ApplicationNotes",
                table: "Leases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Leases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScreeningNotes",
                table: "Leases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequests_AssignedStaffId",
                table: "MaintenanceRequests",
                column: "AssignedStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequests_AspNetUsers_AssignedStaffId",
                table: "MaintenanceRequests",
                column: "AssignedStaffId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequests_AspNetUsers_AssignedStaffId",
                table: "MaintenanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequests_AssignedStaffId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "AssignedStaffId",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "DateAssigned",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "StaffNotes",
                table: "MaintenanceRequests");

            migrationBuilder.DropColumn(
                name: "ApplicationDate",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "ApplicationNotes",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "ScreeningNotes",
                table: "Leases");
        }
    }
}
