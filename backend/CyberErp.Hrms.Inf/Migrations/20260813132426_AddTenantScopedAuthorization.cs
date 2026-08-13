using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantScopedAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantOperation",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwningTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Filter = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantOperation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantOperation_Operation_OperationId",
                        column: x => x.OperationId,
                        principalSchema: "Core",
                        principalTable: "Operation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantOperation_Tenant_OwningTenantId",
                        column: x => x.OwningTenantId,
                        principalSchema: "Core",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantRole",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwningTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCustomized = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantRole_Role_SourceTemplateId",
                        column: x => x.SourceTemplateId,
                        principalSchema: "Core",
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TenantRole_Tenant_OwningTenantId",
                        column: x => x.OwningTenantId,
                        principalSchema: "Core",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSubSystem",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwningTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubSystemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSubSystem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSubSystem_Subsystem_SubSystemId",
                        column: x => x.SubSystemId,
                        principalSchema: "Core",
                        principalTable: "Subsystem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantSubSystem_Tenant_OwningTenantId",
                        column: x => x.OwningTenantId,
                        principalSchema: "Core",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUser",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwningTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsDefaultTenant = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUser_Tenant_OwningTenantId",
                        column: x => x.OwningTenantId,
                        principalSchema: "Core",
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantUser_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Core",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantRolePermission",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    CanExport = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantRolePermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantRolePermission_TenantOperation_TenantOperationId",
                        column: x => x.TenantOperationId,
                        principalSchema: "Core",
                        principalTable: "TenantOperation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantRolePermission_TenantRole_TenantRoleId",
                        column: x => x.TenantRoleId,
                        principalSchema: "Core",
                        principalTable: "TenantRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantUserRole",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantUserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantUserRole_TenantRole_TenantRoleId",
                        column: x => x.TenantRoleId,
                        principalSchema: "Core",
                        principalTable: "TenantRole",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TenantUserRole_TenantUser_TenantUserId",
                        column: x => x.TenantUserId,
                        principalSchema: "Core",
                        principalTable: "TenantUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_OperationId",
                schema: "Core",
                table: "TenantOperation",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_OwningTenantId_Link",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "OwningTenantId", "Link" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_OwningTenantId_OperationId",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "OwningTenantId", "OperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantRole_OwningTenantId_Code",
                schema: "Core",
                table: "TenantRole",
                columns: new[] { "OwningTenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantRole_SourceTemplateId",
                schema: "Core",
                table: "TenantRole",
                column: "SourceTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantRolePermission_TenantOperationId",
                schema: "Core",
                table: "TenantRolePermission",
                column: "TenantOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantRolePermission_TenantRoleId_TenantOperationId",
                schema: "Core",
                table: "TenantRolePermission",
                columns: new[] { "TenantRoleId", "TenantOperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubSystem_OwningTenantId_SubSystemId",
                schema: "Core",
                table: "TenantSubSystem",
                columns: new[] { "OwningTenantId", "SubSystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubSystem_SubSystemId",
                schema: "Core",
                table: "TenantSubSystem",
                column: "SubSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUser_OwningTenantId_UserId",
                schema: "Core",
                table: "TenantUser",
                columns: new[] { "OwningTenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantUser_UserId",
                schema: "Core",
                table: "TenantUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserRole_TenantRoleId",
                schema: "Core",
                table: "TenantUserRole",
                column: "TenantRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUserRole_TenantUserId_TenantRoleId",
                schema: "Core",
                table: "TenantUserRole",
                columns: new[] { "TenantUserId", "TenantRoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantRolePermission",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "TenantSubSystem",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "TenantUserRole",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "TenantOperation",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "TenantRole",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "TenantUser",
                schema: "Core");
        }
    }
}
