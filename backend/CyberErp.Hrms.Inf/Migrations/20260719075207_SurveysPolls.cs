using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SurveysPolls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsSurvey",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsPoll = table.Column<bool>(type: "bit", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    QuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpensOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ClosesOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSurvey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSurveyCompletion",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSurveyCompletion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSurveyCompletion_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsSurveyCompletion_hrmsSurvey_SurveyId",
                        column: x => x.SurveyId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsSurveyResponse",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SurveyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnswersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsSurveyResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsSurveyResponse_hrmsSurvey_SurveyId",
                        column: x => x.SurveyId,
                        principalSchema: "dbo",
                        principalTable: "hrmsSurvey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSurvey_TenantId_Status",
                schema: "dbo",
                table: "hrmsSurvey",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSurveyCompletion_EmployeeId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSurveyCompletion_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSurveyCompletion_TenantId_SurveyId_EmployeeId",
                schema: "dbo",
                table: "hrmsSurveyCompletion",
                columns: new[] { "TenantId", "SurveyId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSurveyResponse_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyResponse",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsSurveyResponse_TenantId_SurveyId",
                schema: "dbo",
                table: "hrmsSurveyResponse",
                columns: new[] { "TenantId", "SurveyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsSurveyCompletion",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSurveyResponse",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsSurvey",
                schema: "dbo");
        }
    }
}
