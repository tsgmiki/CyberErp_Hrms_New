using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ExitInterviewSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsExitInterview",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    RecordedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsExitInterview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsExitInterview_hrmsEmployeeTermination_TerminationId",
                        column: x => x.TerminationId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployeeTermination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsExitQuestionnaire",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsExitQuestionnaire", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTerminationSettlement",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApprovedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PaidOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PaidReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTerminationSettlement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTerminationSettlement_hrmsEmployeeTermination_TerminationId",
                        column: x => x.TerminationId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployeeTermination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSettlementLine",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminationSettlementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAutoSuggested = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSettlementLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSettlementLine_hrmsTerminationSettlement_TerminationSettlementId",
                        column: x => x.TerminationSettlementId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTerminationSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsExitInterview_TenantId_TerminationId",
                schema: "dbo",
                table: "hrmsExitInterview",
                columns: new[] { "TenantId", "TerminationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsExitInterview_TerminationId",
                schema: "dbo",
                table: "hrmsExitInterview",
                column: "TerminationId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSettlementLine_TenantId_TerminationSettlementId",
                schema: "dbo",
                table: "hrmsSettlementLine",
                columns: new[] { "TenantId", "TerminationSettlementId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSettlementLine_TerminationSettlementId",
                schema: "dbo",
                table: "hrmsSettlementLine",
                column: "TerminationSettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTerminationSettlement_TenantId_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationSettlement",
                columns: new[] { "TenantId", "TerminationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTerminationSettlement_TerminationId",
                schema: "dbo",
                table: "hrmsTerminationSettlement",
                column: "TerminationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsExitInterview",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsExitQuestionnaire",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSettlementLine",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTerminationSettlement",
                schema: "dbo");
        }
    }
}
