using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Data-only migration: HRQ / REQ / CND numbering moves from race-prone count+1 to the
    /// per-tenant atomic counter (hrms_NumberSequence). Each tenant's counter is seeded from its
    /// CURRENT MAX so already-issued numbers are never re-allocated.
    /// </summary>
    public partial class SeedRecruitmentNumberSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [Core].[hrms_NumberSequence] ([TenantId], [Key], [Value])
                SELECT r.[TenantId], N'HiringRequest',
                       MAX(TRY_CAST(SUBSTRING(r.[RequestNumber], 5, 20) AS BIGINT))
                FROM [Core].[hrms_HiringRequest] r
                WHERE r.[RequestNumber] LIKE N'HRQ-%'
                  AND NOT EXISTS (SELECT 1 FROM [Core].[hrms_NumberSequence] s
                                  WHERE s.[TenantId] = r.[TenantId] AND s.[Key] = N'HiringRequest')
                GROUP BY r.[TenantId]
                HAVING MAX(TRY_CAST(SUBSTRING(r.[RequestNumber], 5, 20) AS BIGINT)) IS NOT NULL;

                INSERT INTO [Core].[hrms_NumberSequence] ([TenantId], [Key], [Value])
                SELECT q.[TenantId], N'JobRequisition',
                       MAX(TRY_CAST(SUBSTRING(q.[RequisitionNumber], 5, 20) AS BIGINT))
                FROM [Core].[hrms_JobRequisition] q
                WHERE q.[RequisitionNumber] LIKE N'REQ-%'
                  AND NOT EXISTS (SELECT 1 FROM [Core].[hrms_NumberSequence] s
                                  WHERE s.[TenantId] = q.[TenantId] AND s.[Key] = N'JobRequisition')
                GROUP BY q.[TenantId]
                HAVING MAX(TRY_CAST(SUBSTRING(q.[RequisitionNumber], 5, 20) AS BIGINT)) IS NOT NULL;

                INSERT INTO [Core].[hrms_NumberSequence] ([TenantId], [Key], [Value])
                SELECT c.[TenantId], N'Candidate',
                       MAX(TRY_CAST(SUBSTRING(c.[CandidateNumber], 5, 20) AS BIGINT))
                FROM [Core].[hrms_Candidate] c
                WHERE c.[CandidateNumber] LIKE N'CND-%'
                  AND NOT EXISTS (SELECT 1 FROM [Core].[hrms_NumberSequence] s
                                  WHERE s.[TenantId] = c.[TenantId] AND s.[Key] = N'Candidate')
                GROUP BY c.[TenantId]
                HAVING MAX(TRY_CAST(SUBSTRING(c.[CandidateNumber], 5, 20) AS BIGINT)) IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [Core].[hrms_NumberSequence]
                WHERE [Key] IN (N'HiringRequest', N'JobRequisition', N'Candidate');
                """);
        }
    }
}
