using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class CommunityEngagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                schema: "dbo",
                table: "hrmsLearningCommunity",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "hrmsCommunityPostReaction",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningCommunityPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_hrmsCommunityPostReaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsCommunityPostReaction_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsCommunityPostReaction_hrmsLearningCommunityPost_LearningCommunityPostId",
                        column: x => x.LearningCommunityPostId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLearningCommunityPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCommunityPostReaction_EmployeeId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCommunityPostReaction_LearningCommunityPostId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                column: "LearningCommunityPostId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsCommunityPostReaction_TenantId_LearningCommunityPostId_EmployeeId",
                schema: "dbo",
                table: "hrmsCommunityPostReaction",
                columns: new[] { "TenantId", "LearningCommunityPostId", "EmployeeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsCommunityPostReaction",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "dbo",
                table: "hrmsLearningCommunity");

            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "dbo",
                table: "hrmsLearningCommunity");
        }
    }
}
