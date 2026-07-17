using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <inheritdoc />
    public partial class ReportScheduleNormalizationAndProcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriteriaJson",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "Recipients",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.RenameColumn(
                name: "OutputFieldsJson",
                schema: "Core",
                table: "hrms_ReportSchedule",
                newName: "MailBody");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FrequencyWeekly",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHideRecipients",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MailSubject",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OutputFormat",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduleStartDate",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeOfTheDay",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FieldOutput",
                schema: "Core",
                table: "hrms_ReportRun",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsScheduled",
                schema: "Core",
                table: "hrms_ReportRun",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "hrms_ReportRunRecipient",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportRunRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportRunRecipient_hrms_ReportRun_ReportRunId",
                        column: x => x.ReportRunId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReportRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReportScheduleFieldOutput",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FieldOrder = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportScheduleFieldOutput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportScheduleFieldOutput_hrms_ReportSchedule_ReportScheduleId",
                        column: x => x.ReportScheduleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReportSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReportScheduleFieldValue",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportScheduleFieldValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportScheduleFieldValue_hrms_ReportSchedule_ReportScheduleId",
                        column: x => x.ReportScheduleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReportSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hrms_ReportScheduleRecipient",
                schema: "Core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hrms_ReportScheduleRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_hrms_ReportScheduleRecipient_hrms_ReportSchedule_ReportScheduleId",
                        column: x => x.ReportScheduleId,
                        principalSchema: "Core",
                        principalTable: "hrms_ReportSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportRunRecipient_ReportRunId",
                schema: "Core",
                table: "hrms_ReportRunRecipient",
                column: "ReportRunId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportScheduleFieldOutput_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldOutput",
                column: "ReportScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportScheduleFieldValue_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleFieldValue",
                column: "ReportScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_hrms_ReportScheduleRecipient_ReportScheduleId",
                schema: "Core",
                table: "hrms_ReportScheduleRecipient",
                column: "ReportScheduleId");

            // ---------------------------------------------------------------------------------------
            // The reporting engine's schedule/history stored procedures, ported from the legacy MySQL
            // module to SQL Server. NOTE: the legacy _x_Report* procedure BODIES are not present in the
            // reference source tree (they live only in the legacy database); each procedure below is a
            // faithful reconstruction from the legacy C# calling contract (parameter names, IN/OUT
            // directions, and result-set shapes) operating on this system's normalized tables.
            // ---------------------------------------------------------------------------------------

            // 1) _x_ReportActivate — toggle a report definition's active flag.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportActivate
    @ReportId UNIQUEIDENTIFIER,
    @IsActive BIT,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Core.hrms_Report
       SET IsActive = @IsActive,
           UpdatedAt = SYSUTCDATETIME(),
           RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");

            // 2) _x_ReportDelete — delete a report; DB-level ON DELETE CASCADE removes its fields,
            //    output columns, restrictions, saved filters, and schedules (+ schedule children).
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportDelete
    @ReportId UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Core.hrms_Report
     WHERE Id = @ReportId
       AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId);
END");

            // 3) _x_ReportFieldOutputRead — a report's output columns; when a schedule id is supplied,
            //    each column is flagged isShow=1 if that schedule selected it (with saved label/sort).
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportFieldOutputRead
    @ReportKey NVARCHAR(100),
    @ReportScheduleId UNIQUEIDENTIFIER = NULL,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ReportId UNIQUEIDENTIFIER =
        (SELECT TOP 1 Id FROM Core.hrms_Report
          WHERE ReportKey = @ReportKey
            AND (@TenantId IS NULL OR @TenantId = '' OR TenantId = @TenantId));

    SELECT
        CASE WHEN @ReportScheduleId IS NULL THEN 1
             WHEN so.Id IS NOT NULL THEN 1 ELSE 0 END AS IsShow,
        fo.Field AS Field,
        COALESCE(so.Label, fo.Label) AS Label,
        COALESCE(so.SortOrder, 0) AS SortOrder,
        COALESCE(so.FieldOrder, fo.FieldOrder) AS FieldOrder
    FROM Core.hrms_ReportFieldOutput fo
    LEFT JOIN Core.hrms_ReportScheduleFieldOutput so
        ON so.ReportScheduleId = @ReportScheduleId AND so.Field = fo.Field
    WHERE fo.ReportId = @ReportId
    ORDER BY COALESCE(so.FieldOrder, fo.FieldOrder), fo.Label;
END");

            // 4) _x_ReportClientSchedule — INSERT/UPDATE the schedule header; @ReportScheduleId is
            //    InputOutput (NULL/unknown ⇒ generate a new id and return it).
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientSchedule
    @ReportScheduleId UNIQUEIDENTIFIER OUTPUT,
    @TenantId NVARCHAR(450),
    @UserId UNIQUEIDENTIFIER = NULL,
    @ReportId UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @IsScheduled BIT,
    @MailSubject NVARCHAR(300) = NULL,
    @MailBody NVARCHAR(MAX) = NULL,
    @IsHideRecipients BIT = 0,
    @Frequency NVARCHAR(20),
    @FrequencyWeekly INT = 0,
    @TimeOfTheDay INT = 0,
    @ScheduleStartDate DATE = NULL,
    @OutputFormat INT = 1,
    @CronExpression NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF @ReportScheduleId IS NULL
       OR NOT EXISTS (SELECT 1 FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId)
    BEGIN
        SET @ReportScheduleId = NEWID();
        INSERT INTO Core.hrms_ReportSchedule
            (Id, TenantId, ReportId, Name, IsScheduled, IsActive, MailSubject, MailBody, IsHideRecipients,
             Frequency, FrequencyWeekly, TimeOfTheDay, ScheduleStartDate, OutputFormat, CronExpression,
             CreatedAt, RowVersion)
        VALUES
            (@ReportScheduleId, @TenantId, @ReportId, @Name, @IsScheduled, 1, @MailSubject, @MailBody, @IsHideRecipients,
             @Frequency, @FrequencyWeekly, @TimeOfTheDay, @ScheduleStartDate, @OutputFormat, @CronExpression,
             SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE
    BEGIN
        UPDATE Core.hrms_ReportSchedule
           SET Name = @Name, IsScheduled = @IsScheduled, MailSubject = @MailSubject, MailBody = @MailBody,
               IsHideRecipients = @IsHideRecipients, Frequency = @Frequency, FrequencyWeekly = @FrequencyWeekly,
               TimeOfTheDay = @TimeOfTheDay, ScheduleStartDate = @ScheduleStartDate, OutputFormat = @OutputFormat,
               CronExpression = @CronExpression, UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
         WHERE Id = @ReportScheduleId;
    END
END");

            // 5) _x_ReportClientScheduleDelete — pisModifyOnly=1 clears the three child tables only
            //    (for a re-save); 0 deletes children + header.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleDelete
    @ReportScheduleId UNIQUEIDENTIFIER,
    @IsModifyOnly INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Core.hrms_ReportScheduleRecipient  WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM Core.hrms_ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;
    DELETE FROM Core.hrms_ReportScheduleFieldOutput WHERE ReportScheduleId = @ReportScheduleId;
    IF @IsModifyOnly = 0
        DELETE FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId;
END");

            // 6) _x_ReportClientScheduleEnable — grid enable/disable toggle.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleEnable
    @ReportScheduleId UNIQUEIDENTIFIER,
    @Enabled INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Core.hrms_ReportSchedule
       SET IsActive = CASE WHEN @Enabled = 1 THEN 1 ELSE 0 END,
           UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
     WHERE Id = @ReportScheduleId;
END");

            // 7) _x_ReportClientScheduleFieldValue — append one saved criteria value.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleFieldValue
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Value NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO Core.hrms_ReportScheduleFieldValue
        (Id, ReportScheduleId, ReportKey, Field, Value, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Value, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");

            // 8) _x_ReportClientScheduleFieldOutput — append one selected output column.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleFieldOutput
    @ReportScheduleId UNIQUEIDENTIFIER,
    @ReportKey NVARCHAR(100),
    @Field NVARCHAR(100),
    @Label NVARCHAR(200),
    @FieldOrder INT = 0,
    @SortOrder INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tenant NVARCHAR(450) = (SELECT TOP 1 TenantId FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId);
    INSERT INTO Core.hrms_ReportScheduleFieldOutput
        (Id, ReportScheduleId, ReportKey, Field, Label, FieldOrder, SortOrder, TenantId, CreatedAt, RowVersion)
    VALUES
        (NEWID(), @ReportScheduleId, @ReportKey, @Field, @Label, @FieldOrder, @SortOrder, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
END");

            // 9) _x_ReportClientScheduleRecipient — @Type: 'Add' inserts a user/role/e-mail recipient
            //    (resolving a user's e-mail snapshot); 'ListUsers'/'ListRoles' return the tenant's
            //    users/roles flagged with whether this schedule has assigned them.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleRecipient
    @Type NVARCHAR(20),
    @ReportScheduleId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER = NULL,
    @RoleId UNIQUEIDENTIFIER = NULL,
    @Email NVARCHAR(300) = NULL,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Type = 'Add'
    BEGIN
        DECLARE @Tenant NVARCHAR(450) =
            COALESCE(NULLIF(@TenantId, ''), (SELECT TOP 1 TenantId FROM Core.hrms_ReportSchedule WHERE Id = @ReportScheduleId));
        DECLARE @ResolvedEmail NVARCHAR(300) = @Email;
        IF @ResolvedEmail IS NULL AND @UserId IS NOT NULL
            SET @ResolvedEmail = (SELECT TOP 1 Email FROM Core.[User] WHERE Id = @UserId);
        INSERT INTO Core.hrms_ReportScheduleRecipient
            (Id, ReportScheduleId, UserId, RoleId, Email, TenantId, CreatedAt, RowVersion)
        VALUES
            (NEWID(), @ReportScheduleId, @UserId, @RoleId, @ResolvedEmail, @Tenant, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE IF @Type = 'ListUsers'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, u.Id AS UserId, u.UserName AS UserName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned, u.Email AS Email
        FROM Core.[User] u
        LEFT JOIN Core.hrms_ReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.UserId = u.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR u.TenantId = @TenantId)
        ORDER BY u.UserName;
    END
    ELSE IF @Type = 'ListRoles'
    BEGIN
        SELECT @ReportScheduleId AS ReportScheduleId, ro.Id AS RoleId, ro.Name AS RoleName,
               CAST(CASE WHEN r.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssigned
        FROM Core.Role ro
        LEFT JOIN Core.hrms_ReportScheduleRecipient r
            ON r.ReportScheduleId = @ReportScheduleId AND r.RoleId = ro.Id
        WHERE (@TenantId IS NULL OR @TenantId = '' OR ro.TenantId = @TenantId)
        ORDER BY ro.Name;
    END
END");

            // 10) _x_ReportClientScheduleRead — @Type 'Read' (one header by schedule id) or 'List'
            //     (all schedules for a report id), joined to the report for name/key/proc.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportClientScheduleRead
    @Type NVARCHAR(20),
    @Id UNIQUEIDENTIFIER,
    @TenantId NVARCHAR(450) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Type = 'Read'
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc
          FROM Core.hrms_ReportSchedule s
          JOIN Core.hrms_Report r ON r.Id = s.ReportId
         WHERE s.Id = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);
    ELSE
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc
          FROM Core.hrms_ReportSchedule s
          JOIN Core.hrms_Report r ON r.Id = s.ReportId
         WHERE s.ReportId = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId)
         ORDER BY s.Name;
END");

            // 11) _x_ReportGenerateGetScheduleInfo — everything a background run needs, in 4 result
            //     sets: header (with StoredProc), saved criteria, resolved recipient e-mails
            //     (direct + user + role-expanded), and selected output columns.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportGenerateGetScheduleInfo
    @TenantId NVARCHAR(450) = NULL,
    @ReportScheduleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
           s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
           s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
           s.OutputFormat, s.CronExpression, r.StoredProc
      FROM Core.hrms_ReportSchedule s
      JOIN Core.hrms_Report r ON r.Id = s.ReportId
     WHERE s.Id = @ReportScheduleId
       AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);

    SELECT Field, Value FROM Core.hrms_ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;

    SELECT DISTINCT e.Email FROM (
        SELECT rec.Email AS Email
          FROM Core.hrms_ReportScheduleRecipient rec
         WHERE rec.ReportScheduleId = @ReportScheduleId AND rec.Email IS NOT NULL AND rec.Email <> ''
        UNION
        SELECT u.Email
          FROM Core.hrms_ReportScheduleRecipient rec
          JOIN Core.[User] u ON u.Id = rec.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
        UNION
        SELECT u.Email
          FROM Core.hrms_ReportScheduleRecipient rec
          JOIN Core.UserRole ur ON ur.RoleId = rec.RoleId
          JOIN Core.[User] u ON u.Id = ur.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
    ) e
    WHERE e.Email IS NOT NULL AND e.Email <> '';

    SELECT 1 AS IsShow, Field, Label, SortOrder, FieldOrder
      FROM Core.hrms_ReportScheduleFieldOutput
     WHERE ReportScheduleId = @ReportScheduleId
     ORDER BY FieldOrder;
END");

            // 12) _x_ReportGenerateSendToHistory — write one ReportRun row (+ its recipient rows from
            //     a ';'-separated e-mail list). RunSeconds is stored both as-is and as DurationMs.
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE Core.hrms_ReportGenerateSendToHistory
    @TenantId NVARCHAR(450),
    @ReportKey NVARCHAR(100),
    @IsScheduled BIT = 0,
    @Criteria NVARCHAR(MAX) = NULL,
    @FieldOutput NVARCHAR(MAX) = NULL,
    @TotalRecords INT = 0,
    @RunSeconds INT = 0,
    @RanBy NVARCHAR(200) = NULL,
    @Recipients NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RunId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Core.hrms_ReportRun
        (Id, TenantId, ReportKey, CriteriaJson, [RowCount], DurationMs, RanBy, IsScheduled, FieldOutput, CreatedAt, RowVersion)
    VALUES
        (@RunId, @TenantId, @ReportKey, ISNULL(@Criteria, '{}'), @TotalRecords, @RunSeconds * 1000, @RanBy,
         @IsScheduled, @FieldOutput, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));

    IF @Recipients IS NOT NULL AND @Recipients <> ''
        INSERT INTO Core.hrms_ReportRunRecipient (Id, ReportRunId, UserId, Email, TenantId, CreatedAt, RowVersion)
        SELECT NEWID(), @RunId, NULL, LTRIM(RTRIM(value)), @TenantId, SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID())
          FROM STRING_SPLIT(@Recipients, ';')
         WHERE LTRIM(RTRIM(value)) <> '';
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var proc in new[]
            {
                "hrms_ReportActivate", "hrms_ReportDelete", "hrms_ReportFieldOutputRead",
                "hrms_ReportClientSchedule", "hrms_ReportClientScheduleDelete", "hrms_ReportClientScheduleEnable",
                "hrms_ReportClientScheduleFieldValue", "hrms_ReportClientScheduleFieldOutput",
                "hrms_ReportClientScheduleRecipient", "hrms_ReportClientScheduleRead",
                "hrms_ReportGenerateGetScheduleInfo", "hrms_ReportGenerateSendToHistory",
            })
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS Core.{proc};");

            migrationBuilder.DropTable(
                name: "hrms_ReportRunRecipient",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ReportScheduleFieldOutput",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ReportScheduleFieldValue",
                schema: "Core");

            migrationBuilder.DropTable(
                name: "hrms_ReportScheduleRecipient",
                schema: "Core");

            migrationBuilder.DropColumn(
                name: "Frequency",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "FrequencyWeekly",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "IsHideRecipients",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "MailSubject",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "OutputFormat",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "ScheduleStartDate",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "TimeOfTheDay",
                schema: "Core",
                table: "hrms_ReportSchedule");

            migrationBuilder.DropColumn(
                name: "FieldOutput",
                schema: "Core",
                table: "hrms_ReportRun");

            migrationBuilder.DropColumn(
                name: "IsScheduled",
                schema: "Core",
                table: "hrms_ReportRun");

            migrationBuilder.RenameColumn(
                name: "MailBody",
                schema: "Core",
                table: "hrms_ReportSchedule",
                newName: "OutputFieldsJson");

            migrationBuilder.AddColumn<string>(
                name: "CriteriaJson",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Recipients",
                schema: "Core",
                table: "hrms_ReportSchedule",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }
    }
}
