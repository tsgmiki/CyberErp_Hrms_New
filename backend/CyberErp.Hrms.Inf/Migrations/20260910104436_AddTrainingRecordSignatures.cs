using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Hrms.TrainingRecordSignature, AuditLog.Reason and LearningAssignment.RequiresVerification —
    /// GxP option B (2026-09-10).
    /// </summary>
    /// <remarks>
    /// <para>Makes training records defensible as regulatory records: an electronic signature bound
    /// to the record, a reason recordable against a change, and a per-assignment switch for whether a
    /// verifier must counter-sign (logic §12.90).</para>
    ///
    /// <para>⚠️ TrainingRecordSignature RESTRICTS on all three parents — enrolment, obligation and
    /// employee. A signature is the evidence behind a training record; it must not vanish because one
    /// of them was removed, and the database says so as well as the application.</para>
    ///
    /// <para>⚠️ The unique index on (TrainingEnrollmentId, Meaning) is the rule that one person
    /// attests once and one verifier confirms once. A second signature of either meaning would be a
    /// correction, and a correction is a new record rather than an overwrite.</para>
    ///
    /// <para>⚠️ ContentHash is what binds a signature to what was signed. It is computed from a
    /// canonical rendering of the facts by SignedFacts.Canonical — change that function and every
    /// existing signature reads as tampered. See the remarks there before touching it.</para>
    ///
    /// <para>Additive only: nothing existing changes, and an unsigned record behaves exactly as it
    /// did before.</para>
    /// </remarks>
    public partial class AddTrainingRecordSignatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresVerification",
                schema: "Hrms",
                table: "LearningAssignment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "Hrms",
                table: "AuditLog",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainingRecordSignature",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentObligationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Meaning = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SignedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SignedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SignedStatement = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecordSignature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRecordSignature_AssignmentObligation_AssignmentObligationId",
                        column: x => x.AssignmentObligationId,
                        principalSchema: "Hrms",
                        principalTable: "AssignmentObligation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingRecordSignature_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Hrms",
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingRecordSignature_TrainingEnrollment_TrainingEnrollmentId",
                        column: x => x.TrainingEnrollmentId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecordSignature_AssignmentObligationId",
                schema: "Hrms",
                table: "TrainingRecordSignature",
                column: "AssignmentObligationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecordSignature_EmployeeId_SignedOn",
                schema: "Hrms",
                table: "TrainingRecordSignature",
                columns: new[] { "EmployeeId", "SignedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecordSignature_TrainingEnrollmentId_Meaning",
                schema: "Hrms",
                table: "TrainingRecordSignature",
                columns: new[] { "TrainingEnrollmentId", "Meaning" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainingRecordSignature",
                schema: "Hrms");

            migrationBuilder.DropColumn(
                name: "RequiresVerification",
                schema: "Hrms",
                table: "LearningAssignment");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "Hrms",
                table: "AuditLog");
        }
    }
}
