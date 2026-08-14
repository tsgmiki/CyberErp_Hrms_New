using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class PlatformTablesDropTenantIdAndSettingPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "SubscriptionPlanModule");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "SubscriptionPlan");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Core",
                table: "OrganizationSubscription");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Setting",
                type: "datetime2(3)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Core",
                table: "SubscriptionPlanModule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Core",
                table: "SubscriptionPlan",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "Core",
                table: "Setting",
                type: "datetime2(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(3)");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Core",
                table: "OrganizationSubscription",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
