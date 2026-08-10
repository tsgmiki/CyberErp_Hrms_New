using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleAnnualLeaveFromLeaveType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalanceTransaction",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalance",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Annual leave no longer has a LeaveType. Existing annual balances point at whichever type
            // was flagged with the Annual accrual method — repoint those rows to NULL so they keep
            // being recognised as the annual ledger. Everything else (real leave types) is untouched.
            // Scoped per tenant, since each tenant flagged its own type.
            migrationBuilder.Sql("""
                UPDATE b SET b.LeaveTypeId = NULL
                FROM [Hrms].[LeaveBalance] b
                INNER JOIN [Hrms].[LeaveType] t
                    ON t.Id = b.LeaveTypeId AND t.TenantId = b.TenantId
                WHERE t.AccrualMethod = 'Annual';
                """);

            migrationBuilder.Sql("""
                UPDATE x SET x.LeaveTypeId = NULL
                FROM [Hrms].[LeaveBalanceTransaction] x
                INNER JOIN [Hrms].[LeaveType] t
                    ON t.Id = x.LeaveTypeId AND t.TenantId = x.TenantId
                WHERE t.AccrualMethod = 'Annual';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalanceTransaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Hrms",
                table: "LeaveBalance",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
