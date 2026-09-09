using System;
using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Hrms.CourseVersion, ContentModule and ModuleProgress — course content and observed
    /// completion (2026-09-09).
    /// </summary>
    /// <remarks>
    /// <para>Phase 3 of the LMS work. Until now a course was metadata with no content, and completion
    /// was a value HR typed into an enrolment. These three tables let a course carry versioned
    /// material and let completion be DERIVED from what a learner actually did.</para>
    ///
    /// <para>⚠️ ModuleProgress cascades from the ENROLMENT and restricts on the MODULE. Only one
    /// cascade path may reach a table in SQL Server, and the enrolment is the right owner: progress
    /// without its enrolment is meaningless, whereas a published version's modules are never deleted.</para>
    ///
    /// <para>⚠️ No binary column anywhere. A Document module references an existing
    /// Hrms.EmployeeDocument (whose bytes are inline, which suits a PDF); a Video module is a URL to
    /// content hosted elsewhere. Course video inline in SQL Server would not scale, and object
    /// storage is an infrastructure decision this product has not taken — referencing keeps that
    /// decision open instead of pre-empting it (logic §12.86).</para>
    ///
    /// <para>Additive only: nothing existing changes, and a course with no version behaves exactly as
    /// it does today.</para>
    /// </remarks>
    // Hand-written, so it carries its own [Migration] attribute: EF discovers migrations by that
    // attribute, which the scaffolder normally emits into the generated .Designer.cs. Without it the
    // file compiles, sits in the folder, and is silently never applied. Written by hand because the
    // installed EF tools (9.0.5) are a major version behind the runtime and would not update the
    // model snapshot; the snapshot is edited alongside this file — BOTH passes, the property blocks
    // and the relationship blocks, or `database update` reports pending model changes.
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260909180000_AddCourseContent")]
    public partial class AddCourseContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseVersion",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangeNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PublishedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    RetiredOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseVersion_TrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentModule",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentModule_CourseVersion_CourseVersionId",
                        column: x => x.CourseVersionId,
                        principalSchema: "Hrms",
                        principalTable: "CourseVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleProgress",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CompletedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    SecondsSpent = table.Column<int>(type: "int", nullable: false),
                    LastPosition = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleProgress_TrainingEnrollment_TrainingEnrollmentId",
                        column: x => x.TrainingEnrollmentId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleProgress_ContentModule_ContentModuleId",
                        column: x => x.ContentModuleId,
                        principalSchema: "Hrms",
                        principalTable: "ContentModule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // One version number per course.
            migrationBuilder.CreateIndex(
                name: "IX_CourseVersion_TrainingCourseId_VersionNumber",
                schema: "Hrms",
                table: "CourseVersion",
                columns: ["TrainingCourseId", "VersionNumber"],
                unique: true);

            // The player reads a version's modules in order.
            migrationBuilder.CreateIndex(
                name: "IX_ContentModule_CourseVersionId_SortOrder",
                schema: "Hrms",
                table: "ContentModule",
                columns: ["CourseVersionId", "SortOrder"]);

            // One progress row per (enrolment, module) — a second visit updates rather than inserts.
            migrationBuilder.CreateIndex(
                name: "IX_ModuleProgress_TrainingEnrollmentId_ContentModuleId",
                schema: "Hrms",
                table: "ModuleProgress",
                columns: ["TrainingEnrollmentId", "ContentModuleId"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleProgress_ContentModuleId",
                schema: "Hrms",
                table: "ModuleProgress",
                column: "ContentModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Dropped child-first: ModuleProgress references ContentModule, which references
            // CourseVersion.
            migrationBuilder.DropTable(name: "ModuleProgress", schema: "Hrms");
            migrationBuilder.DropTable(name: "ContentModule", schema: "Hrms");
            migrationBuilder.DropTable(name: "CourseVersion", schema: "Hrms");
        }
    }
}
