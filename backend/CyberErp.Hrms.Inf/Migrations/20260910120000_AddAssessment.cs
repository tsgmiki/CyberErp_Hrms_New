using System;
using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Assessment — question banks, quizzes inside a course module, and graded attempts (2026-09-10).
    /// </summary>
    /// <remarks>
    /// <para>Phase 4 of the LMS work. Phase 3 made completion an observation but deliberately left
    /// <c>TrainingEnrollment.AssessmentScore</c> NULL, because there was nothing measuring it. These
    /// seven tables are what measures it.</para>
    ///
    /// <para>⚠️ The quiz hangs off the CONTENT MODULE, not the course. A module belongs to a course
    /// version, which freezes on publish — so a published quiz is frozen too, with no separate rule
    /// to write and no way for the two to disagree (logic §12.87).</para>
    ///
    /// <para>⚠️ Question carries TWO optional parents: a bank question is a reusable template, an
    /// assessment question is a frozen copy, and importing copies rows between them. Both FKs
    /// cascade; they are unrelated roots, so there is no shared ancestor and SQL Server raises no
    /// multiple-cascade-path error.</para>
    ///
    /// <para>⚠️ Attempts are RESTRICTED against the assessment and cascade only from the enrolment.
    /// An attempt is the evidence behind a pass; a published assessment is never deleted anyway, and
    /// letting one take its attempts with it would erase the proof.</para>
    ///
    /// <para>Additive only. A course with no Quiz module behaves exactly as it did after phase 3.</para>
    /// </remarks>
    // Hand-written, so it carries its own [Migration] attribute — see the phase-3 migration for why
    // (EF tools 9.0.5 against a 10.x runtime). The model snapshot is edited alongside it in all
    // three passes: property blocks, relationship blocks, and the navigation blocks at the end.
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260910120000_AddAssessment")]
    public partial class AddAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionBank",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_QuestionBank", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Assessment",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PassMark = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: true),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    ShuffleQuestions = table.Column<bool>(type: "bit", nullable: false),
                    RevealAnswers = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessment_ContentModule_ContentModuleId",
                        column: x => x.ContentModuleId,
                        principalSchema: "Hrms",
                        principalTable: "ContentModule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Points = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Question_QuestionBank_QuestionBankId",
                        column: x => x.QuestionBankId,
                        principalSchema: "Hrms",
                        principalTable: "QuestionBank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Question_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Hrms",
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOption",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionOption_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Hrms",
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAttempt",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    PointsAwarded = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    PointsPossible = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    ScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Passed = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempt_TrainingEnrollment_TrainingEnrollmentId",
                        column: x => x.TrainingEnrollmentId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempt_Assessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Hrms",
                        principalTable: "Assessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttemptAnswer",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttemptAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttemptAnswer_AssessmentAttempt_AssessmentAttemptId",
                        column: x => x.AssessmentAttemptId,
                        principalSchema: "Hrms",
                        principalTable: "AssessmentAttempt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttemptAnswer_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Hrms",
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttemptAnswerOption",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttemptAnswerOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttemptAnswerOption_AttemptAnswer_AttemptAnswerId",
                        column: x => x.AttemptAnswerId,
                        principalSchema: "Hrms",
                        principalTable: "AttemptAnswer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttemptAnswerOption_QuestionOption_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalSchema: "Hrms",
                        principalTable: "QuestionOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBank_TenantId_Name",
                schema: "Hrms",
                table: "QuestionBank",
                columns: ["TenantId", "Name"],
                unique: true);

            // One assessment per module: a Quiz module IS its assessment.
            migrationBuilder.CreateIndex(
                name: "IX_Assessment_ContentModuleId",
                schema: "Hrms",
                table: "Assessment",
                column: "ContentModuleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionBankId_SortOrder",
                schema: "Hrms",
                table: "Question",
                columns: ["QuestionBankId", "SortOrder"]);

            migrationBuilder.CreateIndex(
                name: "IX_Question_AssessmentId_SortOrder",
                schema: "Hrms",
                table: "Question",
                columns: ["AssessmentId", "SortOrder"]);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOption_QuestionId_SortOrder",
                schema: "Hrms",
                table: "QuestionOption",
                columns: ["QuestionId", "SortOrder"]);

            // The retake counter cannot collide even if two tabs start an attempt at once.
            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_TrainingEnrollmentId_AssessmentId_AttemptNumber",
                schema: "Hrms",
                table: "AssessmentAttempt",
                columns: ["TrainingEnrollmentId", "AssessmentId", "AttemptNumber"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempt_AssessmentId",
                schema: "Hrms",
                table: "AssessmentAttempt",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswer_AssessmentAttemptId_QuestionId",
                schema: "Hrms",
                table: "AttemptAnswer",
                columns: ["AssessmentAttemptId", "QuestionId"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswer_QuestionId",
                schema: "Hrms",
                table: "AttemptAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswerOption_AttemptAnswerId_QuestionOptionId",
                schema: "Hrms",
                table: "AttemptAnswerOption",
                columns: ["AttemptAnswerId", "QuestionOptionId"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttemptAnswerOption_QuestionOptionId",
                schema: "Hrms",
                table: "AttemptAnswerOption",
                column: "QuestionOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Child-first, following the reference chain back up.
            migrationBuilder.DropTable(name: "AttemptAnswerOption", schema: "Hrms");
            migrationBuilder.DropTable(name: "AttemptAnswer", schema: "Hrms");
            migrationBuilder.DropTable(name: "AssessmentAttempt", schema: "Hrms");
            migrationBuilder.DropTable(name: "QuestionOption", schema: "Hrms");
            migrationBuilder.DropTable(name: "Question", schema: "Hrms");
            migrationBuilder.DropTable(name: "Assessment", schema: "Hrms");
            migrationBuilder.DropTable(name: "QuestionBank", schema: "Hrms");
        }
    }
}
