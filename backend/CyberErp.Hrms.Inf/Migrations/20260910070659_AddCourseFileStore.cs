using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Hrms.CourseFile — the course-file store, and the ContentModule column that points at it
    /// (2026-09-10).
    /// </summary>
    /// <remarks>
    /// <para>Phase 3 modelled a Document module and then left it unreachable, because the only file
    /// table available was <c>EmployeeDocument</c> — scoped to ONE employee, so course material
    /// stored there is readable by exactly one person. This table is owned by the COURSE, which is
    /// the sentence that could not be expressed before (logic §12.89).</para>
    ///
    /// <para>⚠️ Rows are IMMUTABLE by design — the entity exposes no way to change the bytes. That is
    /// what keeps a published course version frozen: its Document module points at material that
    /// cannot move underneath the people who completed it. Replacing material means uploading a new
    /// file and repointing a draft.</para>
    ///
    /// <para>⚠️ DocumentId is RENAMED to CourseFileId rather than added alongside. The column has
    /// never held a value — the Document kind was unreachable — so this carries no data risk, and
    /// leaving a name that points at the wrong table would mislead every later reader.</para>
    ///
    /// <para>Bytes are inline varbinary(max), as for every other attachment in this product. Video is
    /// still a URL and the upload allow-list excludes media extensions on purpose: streaming from SQL
    /// Server does not scale, and permitting an .mp4 here would undo that decision one row at a time.</para>
    /// </remarks>
    public partial class AddCourseFileStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocumentId",
                schema: "Hrms",
                table: "ContentModule",
                newName: "CourseFileId");

            migrationBuilder.CreateTable(
                name: "CourseFile",
                schema: "Hrms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseFile_TrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "Hrms",
                        principalTable: "TrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseFile_TrainingCourseId",
                schema: "Hrms",
                table: "CourseFile",
                column: "TrainingCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseFile",
                schema: "Hrms");

            migrationBuilder.RenameColumn(
                name: "CourseFileId",
                schema: "Hrms",
                table: "ContentModule",
                newName: "DocumentId");
        }
    }
}
