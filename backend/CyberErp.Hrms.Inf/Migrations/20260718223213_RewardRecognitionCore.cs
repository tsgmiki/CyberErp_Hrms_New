using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class RewardRecognitionCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_IsPublic",
                schema: "dbo",
                table: "hrmsEmployeeRecognition");

            migrationBuilder.AddColumn<decimal>(
                name: "AutoGrantMinScore",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Criteria",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonetaryValue",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PointsValue",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RewardKind",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceRef",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrmsAwardCategory",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Criteria = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_hrmsAwardCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsRecognitionProgram",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecognitionBadgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsRecognitionProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsRecognitionProgram_hrmsRecognitionBadge_RecognitionBadgeId",
                        column: x => x.RecognitionBadgeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsRecognitionBadge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsRewardDisbursement",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecognitionBadgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeRecognitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsRewardDisbursement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsRewardDisbursement_hrmsEmployeeRecognition_EmployeeRecognitionId",
                        column: x => x.EmployeeRecognitionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployeeRecognition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_hrmsRewardDisbursement_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsRewardDisbursement_hrmsRecognitionBadge_RecognitionBadgeId",
                        column: x => x.RecognitionBadgeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsRecognitionBadge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsRewardPointsTransaction",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsRewardPointsTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsRewardPointsTransaction_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsRewardNomination",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomineeEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecognitionBadgeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecognitionProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NominatedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NominatedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    GrantedRecognitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsRewardNomination", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsRewardNomination_hrmsEmployee_NomineeEmployeeId",
                        column: x => x.NomineeEmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsRewardNomination_hrmsRecognitionBadge_RecognitionBadgeId",
                        column: x => x.RecognitionBadgeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsRecognitionBadge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsRewardNomination_hrmsRecognitionProgram_RecognitionProgramId",
                        column: x => x.RecognitionProgramId,
                        principalSchema: "dbo",
                        principalTable: "hrmsRecognitionProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRecognitionBadge_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                column: "AwardCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_IsPublic_RecognizedOn",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                columns: new[] { "TenantId", "IsPublic", "RecognizedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsAwardCategory_TenantId_Name",
                schema: "dbo",
                table: "hrmsAwardCategory",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRecognitionProgram_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRecognitionProgram",
                column: "RecognitionBadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRecognitionProgram_TenantId_Name",
                schema: "dbo",
                table: "hrmsRecognitionProgram",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardDisbursement_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardDisbursement_EmployeeRecognitionId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "EmployeeRecognitionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardDisbursement_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                column: "RecognitionBadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardDisbursement_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardDisbursement_TenantId_Status",
                schema: "dbo",
                table: "hrmsRewardDisbursement",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardNomination_NomineeEmployeeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "NomineeEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardNomination_RecognitionBadgeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "RecognitionBadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardNomination_RecognitionProgramId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                column: "RecognitionProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardNomination_TenantId_NomineeEmployeeId",
                schema: "dbo",
                table: "hrmsRewardNomination",
                columns: new[] { "TenantId", "NomineeEmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardNomination_TenantId_Status",
                schema: "dbo",
                table: "hrmsRewardNomination",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardPointsTransaction_EmployeeId",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsRewardPointsTransaction_TenantId_EmployeeId_TransactionDate",
                schema: "dbo",
                table: "hrmsRewardPointsTransaction",
                columns: new[] { "TenantId", "EmployeeId", "TransactionDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_hrmsRecognitionBadge_hrmsAwardCategory_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge",
                column: "AwardCategoryId",
                principalSchema: "dbo",
                principalTable: "hrmsAwardCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_hrmsRecognitionBadge_hrmsAwardCategory_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropTable(
                name: "hrmsAwardCategory",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsRewardDisbursement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsRewardNomination",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsRewardPointsTransaction",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsRecognitionProgram",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_hrmsRecognitionBadge_AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_IsPublic_RecognizedOn",
                schema: "dbo",
                table: "hrmsEmployeeRecognition");

            migrationBuilder.DropColumn(
                name: "AutoGrantMinScore",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropColumn(
                name: "AwardCategoryId",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropColumn(
                name: "Criteria",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropColumn(
                name: "MonetaryValue",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropColumn(
                name: "PointsValue",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropColumn(
                name: "RewardKind",
                schema: "dbo",
                table: "hrmsRecognitionBadge");

            migrationBuilder.DropColumn(
                name: "SourceRef",
                schema: "dbo",
                table: "hrmsEmployeeRecognition");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeRecognition_TenantId_IsPublic",
                schema: "dbo",
                table: "hrmsEmployeeRecognition",
                columns: new[] { "TenantId", "IsPublic" });
        }
    }
}
