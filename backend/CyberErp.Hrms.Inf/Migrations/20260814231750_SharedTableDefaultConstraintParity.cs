using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class SharedTableDefaultConstraintParity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Africa/Nairobi",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "system",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NumberFormat",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "1,234.56",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "LandingPage",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "/",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "InAppNotifications",
                schema: "Core",
                table: "UserPreference",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "EmailNotifications",
                schema: "Core",
                table: "UserPreference",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "DateFormat",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "dd/MM/yyyy",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "ApprovalNotifications",
                schema: "Core",
                table: "UserPreference",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Core",
                table: "Subsystem",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Role",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Core",
                table: "Role",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Timezone",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Locale",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearStartMonth",
                schema: "Core",
                table: "Organization",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DefaultLanguage",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "en",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DateFormat",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "Core",
                table: "Organization",
                type: "nchar(3)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(3)",
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserNameAttempted",
                schema: "Core",
                table: "LoginTrail",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                schema: "Core",
                table: "LoginTrail",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Login",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            // ---- Four defaults EF cannot express ---------------------------------------------
            // EF always emits N'' for a string default and CONVERT([bit],(x)) for a bool. SRMS
            // stores the same VALUES spelled '' and ((x)). Identical semantics, different catalog
            // text — and the requirement is an identical catalog, so these are dropped and recreated
            // by hand. Looked up by name because SQL Server auto-named them.
            migrationBuilder.Sql(@"
DECLARE @drop nvarchar(max) = N'';
SELECT @drop = @drop + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(dc.parent_object_id))
             + N'.' + QUOTENAME(OBJECT_NAME(dc.parent_object_id))
             + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
FROM sys.default_constraints dc
JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE (dc.parent_object_id = OBJECT_ID('Core.Subsystem') AND c.name IN ('Description','LandingPath','IsActive'))
   OR (dc.parent_object_id = OBJECT_ID('Core.Role') AND c.name = 'IsPlatformRole');
IF @drop <> N'' EXEC sp_executesql @drop;

ALTER TABLE Core.Subsystem ADD CONSTRAINT DF_Subsystem_Description DEFAULT ('') FOR Description;
ALTER TABLE Core.Subsystem ADD CONSTRAINT DF_Subsystem_LandingPath DEFAULT ('') FOR LandingPath;
ALTER TABLE Core.Subsystem ADD CONSTRAINT DF_Subsystem_IsActive DEFAULT ((1)) FOR IsActive;
ALTER TABLE Core.Role ADD CONSTRAINT DF_Role_IsPlatformRole DEFAULT ((0)) FOR IsPlatformRole;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "Africa/Nairobi");

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "system");

            migrationBuilder.AlterColumn<string>(
                name: "NumberFormat",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "1,234.56");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "en");

            migrationBuilder.AlterColumn<string>(
                name: "LandingPage",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValue: "/");

            migrationBuilder.AlterColumn<bool>(
                name: "InAppNotifications",
                schema: "Core",
                table: "UserPreference",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "EmailNotifications",
                schema: "Core",
                table: "UserPreference",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateFormat",
                schema: "Core",
                table: "UserPreference",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "dd/MM/yyyy");

            migrationBuilder.AlterColumn<bool>(
                name: "ApprovalNotifications",
                schema: "Core",
                table: "UserPreference",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Core",
                table: "Subsystem",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Core",
                table: "Role",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Core",
                table: "Role",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Timezone",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Locale",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearStartMonth",
                schema: "Core",
                table: "Organization",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultLanguage",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "en");

            migrationBuilder.AlterColumn<string>(
                name: "DateFormat",
                schema: "Core",
                table: "Organization",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "Core",
                table: "Organization",
                type: "nchar(3)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nchar(3)");

            migrationBuilder.AlterColumn<string>(
                name: "UserNameAttempted",
                schema: "Core",
                table: "LoginTrail",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                schema: "Core",
                table: "LoginTrail",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Login");
        }
    }
}
