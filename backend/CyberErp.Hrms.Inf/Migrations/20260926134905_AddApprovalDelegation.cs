using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalDelegation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalDelegation",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllProcesses = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    RevokedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RevocationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDelegation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DelegationPolicy",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinDelegateExperienceYears = table.Column<int>(type: "int", nullable: false),
                    MinSalaryRatioPercent = table.Column<int>(type: "int", nullable: false),
                    RequireManagerialDelegate = table.Column<bool>(type: "bit", nullable: false),
                    MaxDelegationDays = table.Column<int>(type: "int", nullable: false),
                    DefaultApprovalLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AllowSelfServiceDelegation = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegationPolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalDelegationScope",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DelegationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDelegationScope", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalDelegationScope_ApprovalDelegation_DelegationId",
                        column: x => x.DelegationId,
                        principalSchema: "Hrms",
                        principalTable: "ApprovalDelegation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegation_Delegate_Window",
                schema: "Hrms",
                table: "ApprovalDelegation",
                columns: new[] { "TenantId", "ToEmployeeId", "IsRevoked", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegation_TenantId_FromEmployeeId_IsRevoked",
                schema: "Hrms",
                table: "ApprovalDelegation",
                columns: new[] { "TenantId", "FromEmployeeId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDelegationScope_DelegationId_EntityType",
                schema: "Hrms",
                table: "ApprovalDelegationScope",
                columns: new[] { "DelegationId", "EntityType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DelegationPolicy_TenantId",
                schema: "Hrms",
                table: "DelegationPolicy",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalDelegationScope",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "DelegationPolicy",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "ApprovalDelegation",
                schema: "Hrms");
        }
    }
}
