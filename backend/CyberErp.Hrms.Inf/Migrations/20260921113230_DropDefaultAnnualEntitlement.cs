using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Drops <c>Hrms.AnnualLeaveSetting.DefaultAnnualEntitlement</c> (logic §12.100).
    /// </summary>
    /// <remarks>
    /// ⚠️ DATA LOSS, AND INTENTIONAL. The column held 16.00 on both live policies and was the source
    /// of two defects: the Home dashboard reported it as a spendable balance for employees whose
    /// ledger had never been generated (§12.99), and <c>LeaveBalanceService</c> handed it to EVERY
    /// accruing leave type as an implicit allowance, keyed on the fiscal year rather than the leave
    /// type. Entitlement now comes only from the ledger.
    ///
    /// <para><c>Down</c> restores the column at its original precision but NOT its values — the figure
    /// was configuration, not records, and re-entering it on the Annual Leave Setting form is the
    /// supported way back.</para>
    /// </remarks>
    public partial class DropDefaultAnnualEntitlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultAnnualEntitlement",
                schema: "Hrms",
                table: "AnnualLeaveSetting");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DefaultAnnualEntitlement",
                schema: "Hrms",
                table: "AnnualLeaveSetting",
                type: "decimal(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
