using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TrainingCatalogNeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsTrainingCategory",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_hrmsTrainingCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTrainingCourse",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrainingCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Objectives = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TargetAudience = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Prerequisites = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DurationHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    DeliveryMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CpdHours = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_hrmsTrainingCourse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingCourse_hrmsTrainingCategory_TrainingCategoryId",
                        column: x => x.TrainingCategoryId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTrainingNeed",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    NeedType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NeededBy = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    RequestedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecidedOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    FulfilledOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTrainingNeed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingNeed_hrmsCompetency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "dbo",
                        principalTable: "hrmsCompetency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingNeed_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingNeed_hrmsTrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingCategory_TenantId_Name",
                schema: "dbo",
                table: "hrmsTrainingCategory",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingCourse_TenantId_Name",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingCourse_TenantId_TrainingCategoryId",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                columns: new[] { "TenantId", "TrainingCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingCourse_TrainingCategoryId",
                schema: "dbo",
                table: "hrmsTrainingCourse",
                column: "TrainingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingNeed_CompetencyId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingNeed_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingNeed_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingNeed_TenantId_Status",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingNeed_TrainingCourseId",
                schema: "dbo",
                table: "hrmsTrainingNeed",
                column: "TrainingCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsTrainingNeed",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTrainingCourse",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTrainingCategory",
                schema: "dbo");
        }
    }
}
