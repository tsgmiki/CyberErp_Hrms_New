using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddActingAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActingCompensationEnabled",
                schema: "Hrms",
                table: "DelegationPolicy",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActingCompensationMinDays",
                schema: "Hrms",
                table: "DelegationPolicy",
                type: "int",
                nullable: false,
                // ⚠️ 90, not EF's generated 0. A stored threshold of zero means EVERY
                // delegation exceeds it — a one-day stand-in would raise an acting-pay
                // proposal the moment somebody switched the feature on. The entity default is
                // 90 ("3 months") and the column has to agree with it.
                defaultValue: 90);

            migrationBuilder.AddColumn<bool>(
                name: "RecordActingExperience",
                schema: "Hrms",
                table: "DelegationPolicy",
                type: "bit",
                nullable: false,
                // ⚠️ true, matching the entity. Generated as false, which would silently drop
                // the half of the client's rule that says the period counts as work
                // experience — invisibly, since nothing fails when a row is simply not written.
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "ActingAssignment",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DelegationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoveringForEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PositionTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ActingSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OriginalSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ConcludedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ExperienceRecorded = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActingAssignment", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActingAssignment_TenantId_DelegationId",
                schema: "Hrms",
                table: "ActingAssignment",
                columns: new[] { "TenantId", "DelegationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ActingAssignment_TenantId_EmployeeId",
                schema: "Hrms",
                table: "ActingAssignment",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ActingAssignment_TenantId_Status_EndDate",
                schema: "Hrms",
                table: "ActingAssignment",
                columns: new[] { "TenantId", "Status", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActingAssignment",
                schema: "Hrms");

            migrationBuilder.DropColumn(
                name: "ActingCompensationEnabled",
                schema: "Hrms",
                table: "DelegationPolicy");

            migrationBuilder.DropColumn(
                name: "ActingCompensationMinDays",
                schema: "Hrms",
                table: "DelegationPolicy");

            migrationBuilder.DropColumn(
                name: "RecordActingExperience",
                schema: "Hrms",
                table: "DelegationPolicy");
        }
    }
}
