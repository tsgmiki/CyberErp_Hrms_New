using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeTransferRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RelocationExpense",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedByEmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferKind",
                schema: "dbo",
                table: "hrmsEmployeeMovement",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelocationExpense",
                schema: "dbo",
                table: "hrmsEmployeeMovement");

            migrationBuilder.DropColumn(
                name: "RequestedByEmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeMovement");

            migrationBuilder.DropColumn(
                name: "TransferKind",
                schema: "dbo",
                table: "hrmsEmployeeMovement");
        }
    }
}
