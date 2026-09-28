using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionEntitlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PositionEntitlement",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BenefitPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GrantedWhenActing = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionEntitlement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PositionEntitlement_AllowanceType_AllowanceTypeId",
                        column: x => x.AllowanceTypeId,
                        principalSchema: "Hrms",
                        principalTable: "AllowanceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionEntitlement_BenefitPlan_BenefitPlanId",
                        column: x => x.BenefitPlanId,
                        principalSchema: "Hrms",
                        principalTable: "BenefitPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PositionEntitlement_PositionClass_PositionClassId",
                        column: x => x.PositionClassId,
                        principalSchema: "Hrms",
                        principalTable: "PositionClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntitlement_AllowanceTypeId",
                schema: "Hrms",
                table: "PositionEntitlement",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntitlement_BenefitPlanId",
                schema: "Hrms",
                table: "PositionEntitlement",
                column: "BenefitPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntitlement_PositionClassId",
                schema: "Hrms",
                table: "PositionEntitlement",
                column: "PositionClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PositionEntitlement_TenantId_PositionClassId_IsActive",
                schema: "Hrms",
                table: "PositionEntitlement",
                columns: new[] { "TenantId", "PositionClassId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PositionEntitlement",
                schema: "Hrms");
        }
    }
}
