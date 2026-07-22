using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class TrainingCertification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hrmsEmployeeTrainingCertificate",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrainingEnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CertificateNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IssuedOn = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsEmployeeTrainingCertificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeTrainingCertificate_hrmsEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "dbo",
                        principalTable: "hrmsEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeTrainingCertificate_hrmsTrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_hrmsEmployeeTrainingCertificate_hrmsTrainingEnrollment_TrainingEnrollmentId",
                        column: x => x.TrainingEnrollmentId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingEnrollment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLearningPath",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TargetPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_hrmsLearningPath", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLearningPath_hrmsPosition_TargetPositionId",
                        column: x => x.TargetPositionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hrmsTrainingProviderPayment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsTrainingProviderPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsTrainingProviderPayment_hrmsTrainingSession_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "hrmsLearningPathStep",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LearningPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrmsLearningPathStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrmsLearningPathStep_hrmsLearningPath_LearningPathId",
                        column: x => x.LearningPathId,
                        principalSchema: "dbo",
                        principalTable: "hrmsLearningPath",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hrmsLearningPathStep_hrmsTrainingCourse_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalSchema: "dbo",
                        principalTable: "hrmsTrainingCourse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TenantId_CertificateNo",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                columns: new[] { "TenantId", "CertificateNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TenantId_EmployeeId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                columns: new[] { "TenantId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TenantId_ExpiresOn",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                columns: new[] { "TenantId", "ExpiresOn" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TrainingCourseId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "TrainingCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsEmployeeTrainingCertificate_TrainingEnrollmentId",
                schema: "dbo",
                table: "hrmsEmployeeTrainingCertificate",
                column: "TrainingEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningPath_TargetPositionId",
                schema: "dbo",
                table: "hrmsLearningPath",
                column: "TargetPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningPath_TenantId_Name",
                schema: "dbo",
                table: "hrmsLearningPath",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningPathStep_LearningPathId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                column: "LearningPathId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningPathStep_TenantId_LearningPathId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                columns: new[] { "TenantId", "LearningPathId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsLearningPathStep_TrainingCourseId",
                schema: "dbo",
                table: "hrmsLearningPathStep",
                column: "TrainingCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingProviderPayment_TenantId_Status",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingProviderPayment_TenantId_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                columns: new[] { "TenantId", "TrainingSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_hrmsTrainingProviderPayment_TrainingSessionId",
                schema: "dbo",
                table: "hrmsTrainingProviderPayment",
                column: "TrainingSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hrmsEmployeeTrainingCertificate",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLearningPathStep",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsTrainingProviderPayment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "hrmsLearningPath",
                schema: "dbo");
        }
    }
}
