using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Core.User: AccountStatus becomes a bit, PhoneNumber becomes nullable (2026-08-18).
    /// </summary>
    /// <remarks>
    /// ⚠️ THIS DIVERGES FROM SRMS, WHICH SHARES THIS DATABASE. cybererp_srms keeps AccountStatus as
    /// nvarchar(40) and reads it into a `string`, gating sign-in on `AccountStatus == "Active"`
    /// (AuthenticationServiceExtensions). After this migration SRMS cannot authenticate against CERP
    /// until it is updated to match. Applied deliberately on that understanding — see handoff 0133.
    /// </remarks>
    public partial class UserAccountStatusBitAndNullablePhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "Core",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // ⚠️ HAND-WRITTEN, AND DELIBERATELY SPLIT INTO SEPARATE Sql() CALLS.
            //
            // EF scaffolded a plain AlterColumn to bit, which fails: SQL Server cannot convert the
            // string 'Active' to bit. So the column is rebuilt with an explicit mapping.
            //
            // The rebuild CANNOT be one Sql() call. SQL Server compiles a whole batch before running
            // any of it, so an UPDATE naming a column that an earlier ALTER in the SAME batch adds
            // fails to parse: "Invalid column name 'AccountStatus_bit'". Normally you would separate
            // them with GO, but GO is a client directive, not T-SQL, and EF does not accept it.
            // Each migrationBuilder.Sql() IS its own batch — that is the separator to use.
            //
            // Safe to drop and recreate: no index, check constraint or computed column references
            // AccountStatus (verified against sys.indexes / sys.check_constraints first).

            // 1. Drop the server-named default constraint (DF__User__AccountSta__…).
            migrationBuilder.Sql(@"
                DECLARE @df sysname;
                SELECT @df = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID(N'Core.[User]') AND c.name = N'AccountStatus';
                IF @df IS NOT NULL EXEC(N'ALTER TABLE Core.[User] DROP CONSTRAINT [' + @df + N']');
            ");

            // 2. Add the replacement column.
            migrationBuilder.Sql(@"
                ALTER TABLE Core.[User] ADD AccountStatus_bit bit NOT NULL
                    CONSTRAINT DF_User_AccountStatus_tmp DEFAULT(1);
            ");

            // 3. Map the values — 'Active' => 1, anything else (Suspended | Locked | Invited) => 0.
            migrationBuilder.Sql(@"
                UPDATE Core.[User]
                SET AccountStatus_bit = CASE WHEN AccountStatus = N'Active' THEN 1 ELSE 0 END;
            ");

            // 4. Swap the old column out and rename the new one into its place.
            migrationBuilder.Sql(@"
                ALTER TABLE Core.[User] DROP CONSTRAINT DF_User_AccountStatus_tmp;
                ALTER TABLE Core.[User] DROP COLUMN AccountStatus;
            ");
            migrationBuilder.Sql(@"EXEC sp_rename N'Core.User.AccountStatus_bit', N'AccountStatus', N'COLUMN';");
            migrationBuilder.Sql(@"ALTER TABLE Core.[User] ADD CONSTRAINT DF_User_AccountStatus DEFAULT(1) FOR AccountStatus;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mirror of Up, split for the same reason. ⚠️ The four original states cannot be
            // recovered — only Active/Suspended survive, because the bit never carried the rest.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'DF_User_AccountStatus', N'D') IS NOT NULL
                    ALTER TABLE Core.[User] DROP CONSTRAINT DF_User_AccountStatus;
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE Core.[User] ADD AccountStatus_str nvarchar(20) NOT NULL
                    CONSTRAINT DF_User_AccountStatus_tmp DEFAULT(N'Active');
            ");
            migrationBuilder.Sql(@"
                UPDATE Core.[User]
                SET AccountStatus_str = CASE WHEN AccountStatus = 1 THEN N'Active' ELSE N'Suspended' END;
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE Core.[User] DROP CONSTRAINT DF_User_AccountStatus_tmp;
                ALTER TABLE Core.[User] DROP COLUMN AccountStatus;
            ");
            migrationBuilder.Sql(@"EXEC sp_rename N'Core.User.AccountStatus_str', N'AccountStatus', N'COLUMN';");
            migrationBuilder.Sql(@"ALTER TABLE Core.[User] ADD CONSTRAINT DF_User_AccountStatus DEFAULT(N'Active') FOR AccountStatus;");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "Core",
                table: "User",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
