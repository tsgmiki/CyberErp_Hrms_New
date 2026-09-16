using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Gives each report schedule its own scheduling time zone (logic §12.93).
    /// </summary>
    /// <remarks>
    /// <para>⚠️ NO BACK-FILL, ON PURPOSE. Existing schedules keep <c>TimeZoneId = NULL</c>, which the
    /// application reads as "the organisation default" — exactly how they behave today. A migration
    /// cannot read <c>Hangfire:TimeZone</c> from configuration, so back-filling would mean guessing a
    /// zone and stamping the guess onto every schedule already in the database.</para>
    ///
    /// <para>⚠️ Each <c>Sql()</c> is its OWN batch, which is why there is no <c>GO</c> anywhere below —
    /// <c>CREATE OR ALTER PROCEDURE</c> must begin its batch, and a <c>GO</c> inside one of these
    /// strings is a syntax error rather than a separator (memory.md §4).</para>
    /// </remarks>
    public partial class AddReportScheduleTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                schema: "Hrms",
                table: "ReportSchedule",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(UpsertProc(withTimeZone: true));
            migrationBuilder.Sql(ReadProc(withTimeZone: true));
            migrationBuilder.Sql(ScheduleInfoProc(withTimeZone: true));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ScheduleInfoProc(withTimeZone: false));
            migrationBuilder.Sql(ReadProc(withTimeZone: false));
            migrationBuilder.Sql(UpsertProc(withTimeZone: false));

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                schema: "Hrms",
                table: "ReportSchedule");
        }

        /// <summary>
        /// The schedule-header upsert (reference _x_ReportClientSchedule), with and without the zone.
        /// </summary>
        /// <remarks>
        /// Both variants come from one template so <c>Down</c> restores the *exact* previous definition
        /// rather than a hand-retyped approximation that drifts from it.
        /// </remarks>
        private static string UpsertProc(bool withTimeZone) => $@"-- ===BATCH===
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientSchedule]
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
    @CronExpression NVARCHAR(100){(withTimeZone ? "," : string.Empty)}
    {(withTimeZone ? "@TimeZoneId NVARCHAR(100) = NULL" : string.Empty)}
AS
BEGIN
    SET NOCOUNT ON;
    IF @ReportScheduleId IS NULL
       OR NOT EXISTS (SELECT 1 FROM Hrms.ReportSchedule WHERE Id = @ReportScheduleId)
    BEGIN
        SET @ReportScheduleId = NEWID();
        INSERT INTO Hrms.ReportSchedule
            (Id, TenantId, ReportId, Name, IsScheduled, IsActive, MailSubject, MailBody, IsHideRecipients,
             Frequency, FrequencyWeekly, TimeOfTheDay, ScheduleStartDate, OutputFormat, CronExpression,
             {(withTimeZone ? "TimeZoneId, " : string.Empty)}CreatedAt, RowVersion)
        VALUES
            (@ReportScheduleId, @TenantId, @ReportId, @Name, @IsScheduled, 1, @MailSubject, @MailBody, @IsHideRecipients,
             @Frequency, @FrequencyWeekly, @TimeOfTheDay, @ScheduleStartDate, @OutputFormat, @CronExpression,
             {(withTimeZone ? "@TimeZoneId, " : string.Empty)}SYSUTCDATETIME(), CONVERT(varbinary(8), NEWID()));
    END
    ELSE
    BEGIN
        UPDATE Hrms.ReportSchedule
           SET Name = @Name, IsScheduled = @IsScheduled, MailSubject = @MailSubject, MailBody = @MailBody,
               IsHideRecipients = @IsHideRecipients, Frequency = @Frequency, FrequencyWeekly = @FrequencyWeekly,
               TimeOfTheDay = @TimeOfTheDay, ScheduleStartDate = @ScheduleStartDate, OutputFormat = @OutputFormat,
               CronExpression = @CronExpression, {(withTimeZone ? "TimeZoneId = @TimeZoneId, " : string.Empty)}UpdatedAt = SYSUTCDATETIME(), RowVersion = CONVERT(varbinary(8), NEWID())
         WHERE Id = @ReportScheduleId;
    END
END";

        /// <summary>The schedule-header read (reference _x_ReportClientScheduleRead).</summary>
        private static string ReadProc(bool withTimeZone)
        {
            var zone = withTimeZone ? ", s.TimeZoneId" : string.Empty;
            return $@"-- ===BATCH===
CREATE OR ALTER PROCEDURE [Hrms].[ReportClientScheduleRead]
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
               s.OutputFormat, s.CronExpression, r.StoredProc{zone}
          FROM Hrms.ReportSchedule s
          JOIN Hrms.Report r ON r.Id = s.ReportId
         WHERE s.Id = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);
    ELSE
        SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
               s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
               s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
               s.OutputFormat, s.CronExpression, r.StoredProc{zone}
          FROM Hrms.ReportSchedule s
          JOIN Hrms.Report r ON r.Id = s.ReportId
         WHERE s.ReportId = @Id
           AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId)
         ORDER BY s.Name;
END";
        }

        /// <summary>
        /// The multi-result set the schedule RUNNER and the edit form both read
        /// (reference _x_ReportGenerateGetScheduleInfo).
        /// </summary>
        /// <remarks>
        /// ⚠️ ITS FIRST RESULT SET IS ALSO MAPPED TO <c>ScheduleRow</c>. That is easy to miss — the
        /// obvious proc to change is the one named "...ScheduleRead" — but Dapper binds a positional
        /// record by constructor, so adding a column to the record and not to THIS proc leaves it
        /// unable to materialise at all: the edit form and every scheduled report run fail with
        /// "a parameterless default constructor ... is required" (logic §12.93).
        /// </remarks>
        private static string ScheduleInfoProc(bool withTimeZone)
        {
            var zone = withTimeZone ? ", s.TimeZoneId" : string.Empty;
            return $@"-- ===BATCH===
CREATE OR ALTER PROCEDURE [Hrms].[ReportGenerateGetScheduleInfo]
    @TenantId NVARCHAR(450) = NULL,
    @ReportScheduleId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.Id AS ReportScheduleId, s.ReportId, r.ReportKey, r.ReportName, s.Name,
           s.IsScheduled, s.IsActive, s.MailSubject, s.MailBody, s.IsHideRecipients,
           s.Frequency, s.FrequencyWeekly, s.TimeOfTheDay, s.ScheduleStartDate,
           s.OutputFormat, s.CronExpression, r.StoredProc{zone}
      FROM Hrms.ReportSchedule s
      JOIN Hrms.Report r ON r.Id = s.ReportId
     WHERE s.Id = @ReportScheduleId
       AND (@TenantId IS NULL OR @TenantId = '' OR s.TenantId = @TenantId);

    SELECT Field, Value FROM Hrms.ReportScheduleFieldValue WHERE ReportScheduleId = @ReportScheduleId;

    SELECT DISTINCT e.Email FROM (
        SELECT rec.Email AS Email
          FROM Hrms.ReportScheduleRecipient rec
         WHERE rec.ReportScheduleId = @ReportScheduleId AND rec.Email IS NOT NULL AND rec.Email <> ''
        UNION
        SELECT u.Email
          FROM Hrms.ReportScheduleRecipient rec
          JOIN Core.[User] u ON u.Id = rec.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
        UNION
        SELECT u.Email
          FROM Hrms.ReportScheduleRecipient rec
          JOIN Core.UserRole ur ON ur.RoleId = rec.RoleId
          JOIN Core.[User] u ON u.Id = ur.UserId
         WHERE rec.ReportScheduleId = @ReportScheduleId
    ) e
    WHERE e.Email IS NOT NULL AND e.Email <> '';

    SELECT 1 AS IsShow, Field, Label, SortOrder, FieldOrder
      FROM Hrms.ReportScheduleFieldOutput
     WHERE ReportScheduleId = @ReportScheduleId
     ORDER BY FieldOrder;
END";
        }
    }
}
