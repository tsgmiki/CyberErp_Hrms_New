using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AnnualLeaveSettingAllowHalfDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Default TRUE, matching the entity default: half-day annual leave stays available unless an
            // org turns it off. EF's generated default of false would have silently disabled it.
            migrationBuilder.AddColumn<bool>(
                name: "AllowHalfDay",
                schema: "Hrms",
                table: "AnnualLeaveSetting",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // Carry over each tenant's ACTUAL previous behaviour: this flag used to live on whichever
            // LeaveType was flagged with the Annual accrual method. Tenants that had half-days switched
            // off keep them off; tenants with no such type keep the default above.
            migrationBuilder.Sql("""
                UPDATE s SET s.AllowHalfDay = t.AllowHalfDay
                FROM [Hrms].[AnnualLeaveSetting] s
                CROSS APPLY (
                    SELECT TOP 1 lt.AllowHalfDay
                    FROM [Hrms].[LeaveType] lt
                    WHERE lt.TenantId = s.TenantId AND lt.AccrualMethod = 'Annual' AND lt.IsActive = 1
                    ORDER BY lt.CreatedAt
                ) t;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowHalfDay",
                schema: "Hrms",
                table: "AnnualLeaveSetting");
        }
    }
}
