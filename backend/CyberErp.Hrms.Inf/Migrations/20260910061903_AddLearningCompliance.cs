using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Hrms.LearningAssignment and Hrms.AssignmentObligation — mandatory training and the obligations
    /// it produces (2026-09-10).
    /// </summary>
    /// <remarks>
    /// <para>Phase 5 of the LMS work. Enrolment is one person choosing one session; an assignment is
    /// an obligation the organisation places on a POPULATION and then chases — and it keeps applying
    /// to people who join afterwards, which a list of enrolments cannot do (logic §12.88).</para>
    ///
    /// <para>⚠️ LearningAssignment.AudienceId is deliberately NOT a foreign key. It points at an
    /// organizational unit, a position class or a branch depending on Audience, and no single FK can
    /// express that; the save handler validates it against the right table instead.</para>
    ///
    /// <para>⚠️ AssignmentObligation RESTRICTS on the employee rather than cascading. An obligation is
    /// a compliance record, and deleting a person must not quietly erase the evidence that they were
    /// required to be trained.</para>
    ///
    /// <para>⚠️ There is no Overdue status column: overdue is Pending AND DueOn &lt; today, derived
    /// wherever it is needed. A stored flag would need a nightly sweep purely to keep itself honest,
    /// and would be wrong for the rest of the day whenever that sweep failed.</para>
    ///
    /// <para>Additive only. Nothing existing changes, and with no assignment configured the nightly
    /// sweep does nothing at all.</para>
    /// </remarks>
    public partial class AddLearningCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningAssignment",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AudienceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncludeSubUnits = table.Column<bool>(type: "bit", nullable: false),
                    DueWithinDays = table.Column<int>(type: "int", nullable: false),
                    FixedDueOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    RecurrenceMonths = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_LearningAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningAssignment_TrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentObligation",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CycleNumber = table.Column<int>(type: "int", nullable: false),
                    AssignedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    DueOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TrainingEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastReminderOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    EscalatedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    WaivedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentObligation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentObligation_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Hrms",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentObligation_LearningAssignment_LearningAssignmentId",
                        column: x => x.LearningAssignmentId,
                        principalSchema: "Hrms",
                        principalTable: "LearningAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentObligation_EmployeeId_Status",
                schema: "Hrms",
                table: "AssignmentObligation",
                columns: new[] { "EmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentObligation_LearningAssignmentId_EmployeeId_CycleNumber",
                schema: "Hrms",
                table: "AssignmentObligation",
                columns: new[] { "LearningAssignmentId", "EmployeeId", "CycleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentObligation_TenantId_Status_DueOn",
                schema: "Hrms",
                table: "AssignmentObligation",
                columns: new[] { "TenantId", "Status", "DueOn" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningAssignment_TenantId_IsActive",
                schema: "Hrms",
                table: "LearningAssignment",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningAssignment_TrainingCourseId",
                schema: "Hrms",
                table: "LearningAssignment",
                column: "TrainingCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentObligation",
                schema: "Hrms");

            migrationBuilder.DropTable(
                name: "LearningAssignment",
                schema: "Hrms");
        }
    }
}
