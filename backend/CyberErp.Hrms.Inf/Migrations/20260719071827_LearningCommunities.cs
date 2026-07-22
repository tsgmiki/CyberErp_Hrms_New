using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class LearningCommunities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsLearningCommunity",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLearningCommunity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLearningCommunity_hrmsTrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLearningCommunityMember",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningCommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsModerator = table.Column<bool>(type: "bit", nullable: false),
                    JoinedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLearningCommunityMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLearningCommunityMember_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsLearningCommunityMember_hrmsLearningCommunity_LearningCommunityId",
                        column: x => x.LearningCommunityId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLearningCommunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLearningCommunityPost",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningCommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLearningCommunityPost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLearningCommunityPost_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsLearningCommunityPost_hrmsLearningCommunity_LearningCommunityId",
                        column: x => x.LearningCommunityId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLearningCommunity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunity_TenantId_Name",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunity_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                column: "TrainingCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunityMember_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunityMember_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                column: "LearningCommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunityMember_TenantId_LearningCommunityId_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityMember",
                columns: new[] { "TenantId", "LearningCommunityId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunityPost_EmployeeId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunityPost_LearningCommunityId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                column: "LearningCommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningCommunityPost_TenantId_LearningCommunityId_ParentPostId",
                schema: "dbo",
                table: "hrmsLearningCommunityPost",
                columns: new[] { "TenantId", "LearningCommunityId", "ParentPostId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsLearningCommunityMember",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLearningCommunityPost",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLearningCommunity",
                schema: "dbo");
        }
    }
}
