using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// AuditLog.Reason bounded to 500 characters (2026-09-10).
    /// </summary>
    /// <remarks>
    /// <para>The column was added minutes earlier by AddTrainingRecordSignatures and left unbounded
    /// by omission. Bounding it now matters because Hrms.AuditLog is about to become an append-only
    /// LEDGER table, whose column definitions cannot be altered afterwards — this is the last
    /// opportunity to get the shape right without another create-copy-swap (logic §12.90).</para>
    ///
    /// <para>⚠️ The scaffolder warns about possible data loss. It cannot be: the column has existed
    /// for minutes and holds no rows. Verified before applying.</para>
    /// </remarks>
    public partial class BoundAuditReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                schema: "Hrms",
                table: "AuditLog",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                schema: "Hrms",
                table: "AuditLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
