using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class DropOwningTenantIdUseTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantOperation_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantRole_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantRole");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantSubSystem_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantSubSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantUser_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantUser");

            migrationBuilder.DropIndex(
                name: "IX_TenantUser_OwningTenantId_UserId",
                schema: "Core",
                table: "TenantUser");

            migrationBuilder.DropIndex(
                name: "IX_TenantSubSystem_OwningTenantId_SubSystemId",
                schema: "Core",
                table: "TenantSubSystem");

            migrationBuilder.DropIndex(
                name: "IX_TenantRole_OwningTenantId_Code",
                schema: "Core",
                table: "TenantRole");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_OwningTenantId_Link",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_OwningTenantId_OperationId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropColumn(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantUser");

            migrationBuilder.DropColumn(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantSubSystem");

            migrationBuilder.DropColumn(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantRole");

            migrationBuilder.DropColumn(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.CreateIndex(
                name: "IX_TenantUser_TenantId_UserId",
                schema: "Core",
                table: "TenantUser",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubSystem_TenantId_SubSystemId",
                schema: "Core",
                table: "TenantSubSystem",
                columns: new[] { "TenantId", "SubSystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantRole_TenantId_Code",
                schema: "Core",
                table: "TenantRole",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_TenantId_Link",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "TenantId", "Link" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantOperation_TenantId_OperationId",
                schema: "Core",
                table: "TenantOperation",
                columns: new[] { "TenantId", "OperationId" },
                unique: true);

            // ⚠️ These foreign keys are added in RAW SQL, not through the EF model, because EF cannot
            // model a relationship on a value-converted property — TenantId is a string in the CLR and
            // a uniqueidentifier in the database (logic.md §12.14). The constraint is a database
            // concern; nothing in the code navigates it.
            //
            // Exactly the three SRMS constrains: TenantOperation, TenantRole and TenantUser.
            // TenantSubSystem has no such foreign key there, so it gets none here either.
            migrationBuilder.Sql(@"
                ALTER TABLE Core.TenantOperation WITH CHECK
                    ADD CONSTRAINT FK_TenantOperation_Tenant_TenantId
                    FOREIGN KEY (TenantId) REFERENCES Core.Tenant (Id);
                ALTER TABLE Core.TenantRole WITH CHECK
                    ADD CONSTRAINT FK_TenantRole_Tenant_TenantId
                    FOREIGN KEY (TenantId) REFERENCES Core.Tenant (Id);
                ALTER TABLE Core.TenantUser WITH CHECK
                    ADD CONSTRAINT FK_TenantUser_Tenant_TenantId
                    FOREIGN KEY (TenantId) REFERENCES Core.Tenant (Id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantUser_TenantId_UserId",
                schema: "Core",
                table: "TenantUser");

            migrationBuilder.DropIndex(
                name: "IX_TenantSubSystem_TenantId_SubSystemId",
                schema: "Core",
                table: "TenantSubSystem");

            migrationBuilder.DropIndex(
                name: "IX_TenantRole_TenantId_Code",
                schema: "Core",
                table: "TenantRole");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_TenantId_Link",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.DropIndex(
                name: "IX_TenantOperation_TenantId_OperationId",
                schema: "Core",
                table: "TenantOperation");

            migrationBuilder.AddColumn<Guid>(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantUser",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantSubSystem",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantRole",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OwningTenantId",
                schema: "Core",
                table: "TenantOperation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TenantUser_OwningTenantId_UserId",
                schema: "Core",
                table: "TenantUser",
                columns: new[] { "OwningTenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubSystem_OwningTenantId_SubSystemId",
                schema: "Core",
                table: "TenantSubSystem",
                columns: new[] { "OwningTenantId", "SubSystemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantRole_OwningTenantId_Code",
                schema: "Core",
                table: "TenantRole",
                columns: new[] { "OwningTenantId", "Code" },
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_TenantOperation_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantOperation",
                column: "OwningTenantId",
                principalSchema: "Core",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantRole_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantRole",
                column: "OwningTenantId",
                principalSchema: "Core",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantSubSystem_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantSubSystem",
                column: "OwningTenantId",
                principalSchema: "Core",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantUser_Tenant_OwningTenantId",
                schema: "Core",
                table: "TenantUser",
                column: "OwningTenantId",
                principalSchema: "Core",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
