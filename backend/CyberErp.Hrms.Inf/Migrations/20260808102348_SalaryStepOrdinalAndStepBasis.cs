using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberErp.Hrms.Inf.Migrations
{
    /// <summary>
    /// Adds <c>lupStep.Ordinal</c> — the numeric rung a step occupies on its pay ladder — so a salary
    /// revision can do arithmetic on steps ("current step + 2.5"). Until now the only step identifiers
    /// were <c>Name</c>/<c>Code</c>, which are free text and inconsistent between tenants, so nothing
    /// could be ordered or added reliably.
    /// </summary>
    public partial class SalaryStepOrdinalAndStepBasis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ordinal",
                schema: "Core",
                table: "lupStep",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // ---- Backfill -------------------------------------------------------------------
            // Existing codes carry no dependable ordering: one tenant uses "01"(Base), "1".."8",
            // "11"(Ceiling); others use "S1" / "ST1". So rank per tenant by the number embedded in the
            // code (first digit onwards), falling back to the code itself when there is no number, and
            // number the result 1..N.
            //
            // For the tenant above that yields Base=1, steps 1-8 = 2..9, Ceiling=10 — i.e. named rungs
            // land at the bottom and top, which is what those names mean. It is still an INFERENCE:
            // ordinals are what pay now depends on, so they should be eyeballed once per tenant
            // (SELECT Name, Code, Ordinal FROM Core.lupStep ORDER BY TenantId, Ordinal) and corrected
            // where a tenant's ladder does not follow that reading.
            migrationBuilder.Sql(@"
;WITH ranked AS (
    SELECT  Id,
            ROW_NUMBER() OVER (
                PARTITION BY TenantId
                ORDER BY
                    CASE WHEN PATINDEX('%[0-9]%', Code) > 0
                         THEN TRY_CONVERT(int, SUBSTRING(Code, PATINDEX('%[0-9]%', Code), 10))
                    END,            -- NULLs (no digits) sort first in SQL Server; tie-broken below
                    Code,
                    Name
            ) AS rn
    FROM Core.lupStep
)
UPDATE s SET s.Ordinal = r.rn
FROM Core.lupStep s
INNER JOIN ranked r ON r.Id = s.Id;");

            // ---- Read path ------------------------------------------------------------------
            // A step revision reads (JobGradeId, StepId, Salary) for the targeted grades in one shot.
            // The existing IX_coreSalaryScale_TenantId_JobGradeId_StepId keys that exactly; adding
            // Salary as an INCLUDE makes the read index-only (no key lookups into the clustered index).
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_coreSalaryScale_TenantId_JobGradeId_StepId'
           AND object_id = OBJECT_ID('Core.coreSalaryScale'))
    DROP INDEX IX_coreSalaryScale_TenantId_JobGradeId_StepId ON Core.coreSalaryScale;

CREATE NONCLUSTERED INDEX IX_coreSalaryScale_TenantId_JobGradeId_StepId
    ON Core.coreSalaryScale (TenantId, JobGradeId, StepId)
    INCLUDE (Salary);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_coreSalaryScale_TenantId_JobGradeId_StepId'
           AND object_id = OBJECT_ID('Core.coreSalaryScale'))
    DROP INDEX IX_coreSalaryScale_TenantId_JobGradeId_StepId ON Core.coreSalaryScale;

CREATE NONCLUSTERED INDEX IX_coreSalaryScale_TenantId_JobGradeId_StepId
    ON Core.coreSalaryScale (TenantId, JobGradeId, StepId);");

            migrationBuilder.DropColumn(
                name: "Ordinal",
                schema: "Core",
                table: "lupStep");
        }
    }
}
