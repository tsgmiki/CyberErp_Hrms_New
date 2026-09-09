using System;
using CyberErp.Hrms.Inf.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Hrms.CourseCompetency — which competencies a training course develops (2026-09-09).
    /// </summary>
    /// <remarks>
    /// <para>Phase 1 of the LMS work: the join that lets a competency gap name a course. Until now a
    /// <c>TrainingNeed</c> could reference a competency but a <c>TrainingCourse</c> could not, so the
    /// suggestion engine could say a person scored low on something and could list courses, with
    /// nothing connecting the two.</para>
    ///
    /// <para>⚠️ Delete rules mirror <c>PositionCompetency</c>, and for the same reasons: the mapping
    /// dies with its COURSE (cascade — a mapping without its course is meaningless), while a
    /// competency in use anywhere cannot be deleted (restrict). Only one cascade path reaches this
    /// table, so SQL Server raises no multiple-cascade-path error.</para>
    ///
    /// <para>⚠️ Additive only. No existing row, column or behaviour changes: an unmapped course
    /// behaves exactly as it does today, and the suggestion engine falls back to its current wording
    /// when a gap has no course mapped to it.</para>
    /// </remarks>
    // Hand-written, so it carries its own [Migration] attribute: EF discovers migrations by that
    // attribute, which the scaffolder normally emits into the generated .Designer.cs. Without it the
    // file compiles, sits in the folder, and is silently never applied.
    //
    // Written by hand rather than scaffolded because the installed EF tools (9.0.5) are a major
    // version behind the runtime, and `migrations add` in that state does not update
    // HrmsDbContextModelSnapshot — leaving the next scaffold to "rediscover" this table and emit it
    // twice. The snapshot is updated alongside this file instead.
    [DbContext(typeof(HrmsDbContext))]
    [Migration("20260909120000_AddCourseCompetency")]
    public partial class AddCourseCompetency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseCompetency",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // datetime2(3) for CreatedAt and (7) for UpdatedAt is the convention across
                    // every table here — verified against the live schema, not assumed.
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCompetency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseCompetency_TrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseCompetency_Competency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "Hrms",
                        principalTable: "Competency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // One row per pair: mapping the same competency twice says nothing extra and would
            // double-count the course in a recommendation list.
            migrationBuilder.CreateIndex(
                name: "IX_CourseCompetency_TrainingCourseId_CompetencyId",
                schema: "Hrms",
                table: "CourseCompetency",
                columns: ["TrainingCourseId", "CompetencyId"],
                unique: true);

            // The recommendation reads competency-first ("which courses teach X?"), which the unique
            // index above cannot serve — its leading column is the course.
            migrationBuilder.CreateIndex(
                name: "IX_CourseCompetency_CompetencyId",
                schema: "Hrms",
                table: "CourseCompetency",
                column: "CompetencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CourseCompetency", schema: "Hrms");
        }
    }
}
