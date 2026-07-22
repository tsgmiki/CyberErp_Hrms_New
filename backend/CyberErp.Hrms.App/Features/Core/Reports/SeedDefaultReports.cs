using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Reports;

/// <summary>
/// Seeds the standard HRMS report catalog for the CURRENT tenant (mirrors ISeedDefaultMenu /
/// ISeedDefaultWorkflows). Each entry routes through ISaveReport, so the usual validation —
/// including the stored-procedure existence check — applies. Idempotent by ReportKey: existing
/// reports are left untouched so tenant-specific customizations survive re-seeding.
/// </summary>
public interface ISeedDefaultReports { Task<int> SeedAsync(); }

public class SeedDefaultReports(
    IRepository<Report> reportRepository,
    ISaveReport saveReport,
    ILogger<SeedDefaultReports> logger) : ISeedDefaultReports
{
    private static SaveReportFieldDto F(string field, string label, string dataType, int order) =>
        new() { Field = field, Label = label, DataType = dataType, FieldOrder = order };

    private static SaveReportOutputDto O(string field, string label, int order) =>
        new() { Field = field, Label = label, FieldOrder = order };

    // Phase 1 — Workforce standard reports. DataType names must match ReportFieldDataType;
    // Select/MultiSelect field keys must have a branch in Core.hrms_ReportFieldValues.
    private static List<SaveReportDto> Catalog() =>
    [
        new()
        {
            ReportKey = "HeadcountByUnit",
            ReportName = "Headcount by Unit",
            ReportGrouping = "Workforce",
            StoredProc = "Core.hrms_Report_HeadcountByUnit",
            SortOrder = 10,
            Description = "Active workforce headcount, groupable by unit, branch, status, nature or gender, with per-group counts.",
            GridConfig = "{\"groupBy\":[\"UnitName\"],\"allowUserCustomize\":true,\"maxGroupLevels\":3,\"showGroupSummary\":true}",
            Fields =
            [
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 1),
                F("EmploymentStatus", "Status", nameof(ReportFieldDataType.Select), 2),
                F("EmploymentNature", "Nature", nameof(ReportFieldDataType.Select), 3),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("Gender", "Gender", 3),
                O("UnitName", "Unit", 4), O("BranchName", "Branch", 5), O("PositionTitle", "Position", 6),
                O("EmploymentStatus", "Status", 7), O("EmploymentNature", "Nature", 8),
                O("IsManagerial", "Managerial", 9), O("HireDate", "Hire Date", 10),
            ],
        },
        new()
        {
            ReportKey = "EmployeeDemographics",
            ReportName = "Employee Demographics",
            ReportGrouping = "Workforce",
            StoredProc = "Core.hrms_Report_EmployeeDemographics",
            SortOrder = 20,
            Description = "Workforce composition by gender and age band, with per-group counts.",
            GridConfig = "{\"groupBy\":[\"Gender\"],\"allowUserCustomize\":true,\"maxGroupLevels\":3,\"showGroupSummary\":true}",
            Fields =
            [
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 1),
                F("EmploymentStatus", "Status", nameof(ReportFieldDataType.Select), 2),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("Gender", "Gender", 3),
                O("Age", "Age", 4), O("AgeBand", "Age Band", 5), O("UnitName", "Unit", 6),
                O("EmploymentStatus", "Status", 7), O("HireDate", "Hire Date", 8),
            ],
        },
        new()
        {
            ReportKey = "NewHires",
            ReportName = "New Hires",
            ReportGrouping = "Workforce",
            StoredProc = "Core.hrms_Report_NewHires",
            SortOrder = 30,
            Description = "Employees hired within a period, newest first.",
            Fields =
            [
                F("HireDate1", "Hired From", nameof(ReportFieldDataType.Date), 1),
                F("HireDate2", "Hired To", nameof(ReportFieldDataType.Date), 2),
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 3),
                F("EmploymentNature", "Nature", nameof(ReportFieldDataType.Select), 4),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("Gender", "Gender", 3),
                O("HireDate", "Hire Date", 4), O("UnitName", "Unit", 5), O("BranchName", "Branch", 6),
                O("PositionTitle", "Position", 7), O("EmploymentNature", "Nature", 8),
                O("EmploymentStatus", "Status", 9), O("IsProbation", "On Probation", 10), O("Salary", "Salary", 11),
            ],
        },
        new()
        {
            ReportKey = "ProbationTracking",
            ReportName = "Probation Tracking",
            ReportGrouping = "Workforce",
            StoredProc = "Core.hrms_Report_ProbationTracking",
            SortOrder = 40,
            Description = "Employees currently on probation with days remaining (negative = overdue confirmation).",
            Fields =
            [
                F("ProbationEnd1", "Ending From", nameof(ReportFieldDataType.Date), 1),
                F("ProbationEnd2", "Ending To", nameof(ReportFieldDataType.Date), 2),
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 3),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("HireDate", "Hire Date", 3),
                O("ProbationEndDate", "Probation Ends", 4), O("DaysRemaining", "Days Remaining", 5),
                O("UnitName", "Unit", 6), O("PositionTitle", "Position", 7), O("EmploymentStatus", "Status", 8),
            ],
        },
        new()
        {
            ReportKey = "Terminations",
            ReportName = "Terminations & Attrition",
            ReportGrouping = "Turnover",
            StoredProc = "Core.hrms_Report_Terminations",
            SortOrder = 50,
            Description = "Termination cases with tenure at exit, groupable by type, case status, unit or branch.",
            GridConfig = "{\"groupBy\":[\"TerminationType\"],\"allowUserCustomize\":true,\"maxGroupLevels\":3,\"showGroupSummary\":true}",
            Fields =
            [
                F("LastWorkingDate1", "Left From", nameof(ReportFieldDataType.Date), 1),
                F("LastWorkingDate2", "Left To", nameof(ReportFieldDataType.Date), 2),
                F("TerminationType", "Type", nameof(ReportFieldDataType.Select), 3),
                F("TerminationStatus", "Case Status", nameof(ReportFieldDataType.Select), 4),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("UnitName", "Unit", 3),
                O("BranchName", "Branch", 4), O("PositionTitle", "Position", 5), O("TerminationType", "Type", 6),
                O("TerminationStatus", "Case Status", 7), O("NoticeDate", "Notice Date", 8),
                O("LastWorkingDate", "Last Working Day", 9), O("TenureYears", "Tenure (Years)", 10), O("Reason", "Reason", 11),
            ],
        },
        new()
        {
            ReportKey = "EmployeeMovements",
            ReportName = "Employee Movements",
            ReportGrouping = "Turnover",
            StoredProc = "Core.hrms_Report_EmployeeMovements",
            SortOrder = 60,
            Description = "Transfers, promotions and demotions in a period, with from/to placement and salary.",
            Fields =
            [
                F("EffectiveDate1", "Effective From", nameof(ReportFieldDataType.Date), 1),
                F("EffectiveDate2", "Effective To", nameof(ReportFieldDataType.Date), 2),
                F("MovementType", "Movement", nameof(ReportFieldDataType.Select), 3),
                F("MovementStatus", "Status", nameof(ReportFieldDataType.Select), 4),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("MovementType", "Movement", 3),
                O("TransferKind", "Transfer Kind", 4), O("MovementStatus", "Status", 5), O("EffectiveDate", "Effective Date", 6),
                O("FromPosition", "From Position", 7), O("ToPosition", "To Position", 8),
                O("FromSalary", "From Salary", 9), O("ToSalary", "To Salary", 10),
                O("FromBranchName", "From Branch", 11), O("ToBranchName", "To Branch", 12),
                O("ExecutedAt", "Executed", 13), O("Reason", "Reason", 14),
            ],
        },
        // ---- Phase 2: Leave & Pay ----------------------------------------------------------------
        new()
        {
            ReportKey = "LeaveBalances",
            ReportName = "Leave Balances",
            ReportGrouping = "Leave",
            StoredProc = "Core.hrms_Report_LeaveBalances",
            SortOrder = 80,
            Description = "Per-employee leave balances for a fiscal year: entitled, carried forward, adjusted, taken and remaining days.",
            Fields =
            [
                F("FiscalYearId", "Fiscal Year", nameof(ReportFieldDataType.Select), 1),
                F("LeaveTypeId", "Leave Type", nameof(ReportFieldDataType.Select), 2),
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 3),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("UnitName", "Unit", 3),
                O("LeaveTypeName", "Leave Type", 4), O("FiscalYearName", "Fiscal Year", 5), O("Entitled", "Entitled", 6),
                O("CarriedForward", "Carried Forward", 7), O("Adjusted", "Adjusted", 8),
                O("Taken", "Taken", 9), O("Remaining", "Remaining", 10),
            ],
        },
        new()
        {
            ReportKey = "LeaveTaken",
            ReportName = "Leave Taken",
            ReportGrouping = "Leave",
            StoredProc = "Core.hrms_Report_LeaveTaken",
            SortOrder = 90,
            Description = "Leave periods taken (one row per request line), filterable by period, type and request status.",
            Fields =
            [
                F("StartDate1", "From", nameof(ReportFieldDataType.Date), 1),
                F("StartDate2", "To", nameof(ReportFieldDataType.Date), 2),
                F("LeaveTypeId", "Leave Type", nameof(ReportFieldDataType.Select), 3),
                F("LeaveStatus", "Request Status", nameof(ReportFieldDataType.Select), 4),
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 5),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("UnitName", "Unit", 3),
                O("LeaveTypeName", "Leave Type", 4), O("StartDate", "From", 5), O("EndDate", "To", 6),
                O("DayPart", "Day Part", 7), O("WorkingDays", "Working Days", 8),
                O("RequestStatus", "Status", 9), O("SubmittedDate", "Submitted", 10),
            ],
        },
        new()
        {
            ReportKey = "SalaryRegister",
            ReportName = "Salary Register",
            ReportGrouping = "Compensation",
            StoredProc = "Core.hrms_Report_SalaryRegister",
            SortOrder = 100,
            Description = "Base salary register with grade and step, groupable by unit, branch, grade or status, with per-group headcount and salary totals.",
            GridConfig = "{\"groupBy\":[\"UnitName\"],\"allowUserCustomize\":true,\"maxGroupLevels\":3,\"showGroupSummary\":true}",
            Fields =
            [
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 1),
                F("JobGradeId", "Job Grade", nameof(ReportFieldDataType.Select), 2),
                F("EmploymentStatus", "Status", nameof(ReportFieldDataType.Select), 3),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("UnitName", "Unit", 3),
                O("BranchName", "Branch", 4), O("PositionTitle", "Position", 5), O("JobGradeName", "Job Grade", 6),
                O("StepName", "Step", 7), O("EmploymentStatus", "Status", 8),
                O("HireDate", "Hire Date", 9), O("Salary", "Salary", 10),
            ],
        },
        // ---- Phase 3: Cases & Pipeline -----------------------------------------------------------
        new()
        {
            ReportKey = "DisciplinaryCases",
            ReportName = "Disciplinary Cases",
            ReportGrouping = "Cases",
            StoredProc = "Core.hrms_Report_DisciplinaryCases",
            SortOrder = 110,
            Description = "Disciplinary case history: violation, measure taken, case status and reward/promotion blocks.",
            Fields =
            [
                F("ViolationDate1", "Violation From", nameof(ReportFieldDataType.Date), 1),
                F("ViolationDate2", "Violation To", nameof(ReportFieldDataType.Date), 2),
                F("MeasureType", "Measure", nameof(ReportFieldDataType.Select), 3),
                F("DisciplinaryStatus", "Case Status", nameof(ReportFieldDataType.Select), 4),
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 5),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("UnitName", "Unit", 3),
                O("ViolationDate", "Violation Date", 4), O("ViolationType", "Violation", 5), O("MeasureType", "Measure", 6),
                O("DisciplinaryStatus", "Case Status", 7), O("EffectiveDate", "Effective", 8), O("ValidUntil", "Valid Until", 9),
                O("AffectsPromotion", "Blocks Promotion", 10), O("AffectsReward", "Blocks Reward", 11), O("Resolution", "Resolution", 12),
            ],
        },
        new()
        {
            ReportKey = "TrainingCompletion",
            ReportName = "Training Completion",
            ReportGrouping = "Training",
            StoredProc = "Core.hrms_Report_TrainingCompletion",
            SortOrder = 120,
            Description = "Training enrollments with attendance, assessment score, completion and feedback per session.",
            Fields =
            [
                F("SessionStart1", "Session From", nameof(ReportFieldDataType.Date), 1),
                F("SessionStart2", "Session To", nameof(ReportFieldDataType.Date), 2),
                F("TrainingCourseId", "Course", nameof(ReportFieldDataType.Select), 3),
                F("EnrollmentStatus", "Status", nameof(ReportFieldDataType.Select), 4),
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 5),
            ],
            FieldOutputs =
            [
                O("EmployeeNumber", "Employee #", 1), O("FullName", "Full Name", 2), O("UnitName", "Unit", 3),
                O("CourseName", "Course", 4), O("SessionStart", "Session From", 5), O("SessionEnd", "Session To", 6),
                O("DeliveryMode", "Delivery", 7), O("EnrollmentStatus", "Status", 8), O("AttendancePercent", "Attendance %", 9),
                O("AssessmentScore", "Score", 10), O("CompletedOn", "Completed On", 11), O("FeedbackRating", "Feedback", 12),
            ],
        },
        new()
        {
            ReportKey = "RecruitmentPipeline",
            ReportName = "Recruitment Pipeline",
            ReportGrouping = "Recruitment",
            StoredProc = "Core.hrms_Report_RecruitmentPipeline",
            SortOrder = 130,
            Description = "Applications across the hiring pipeline, groupable by stage, vacancy or unit, with per-group counts.",
            GridConfig = "{\"groupBy\":[\"ApplicationStage\"],\"allowUserCustomize\":true,\"maxGroupLevels\":3,\"showGroupSummary\":true}",
            Fields =
            [
                F("AppliedAt1", "Applied From", nameof(ReportFieldDataType.Date), 1),
                F("AppliedAt2", "Applied To", nameof(ReportFieldDataType.Date), 2),
                F("ApplicationStage", "Stage", nameof(ReportFieldDataType.Select), 3),
                F("RequisitionStatus", "Requisition Status", nameof(ReportFieldDataType.Select), 4),
            ],
            FieldOutputs =
            [
                O("CandidateNumber", "Candidate #", 1), O("CandidateName", "Candidate", 2), O("RequisitionNumber", "Requisition #", 3),
                O("RequisitionTitle", "Vacancy", 4), O("UnitName", "Unit", 5), O("ApplicationStage", "Stage", 6),
                O("RequisitionStatus", "Req. Status", 7), O("AppliedAt", "Applied", 8),
                O("ScreeningScore", "Screening", 9), O("Source", "Source", 10),
            ],
        },
        new()
        {
            ReportKey = "VacantPositions",
            ReportName = "Vacant Positions",
            ReportGrouping = "Workforce",
            StoredProc = "Core.hrms_Report_VacantPositions",
            SortOrder = 70,
            Description = "Open (vacant) positions of the establishment with their requirements.",
            Fields =
            [
                F("OrganizationUnitId", "Units", nameof(ReportFieldDataType.MultiSelect), 1),
                F("PositionClassId", "Position Class", nameof(ReportFieldDataType.Select), 2),
            ],
            FieldOutputs =
            [
                O("PositionCode", "Position Code", 1), O("PositionTitle", "Position Title", 2), O("UnitName", "Unit", 3),
                O("BranchName", "Branch", 4), O("MinQualifications", "Qualifications", 5),
                O("MinExperienceYears", "Min Exp (Years)", 6), O("VacantSince", "Vacant Since", 7),
            ],
        },
    ];

    public async Task<int> SeedAsync()
    {
        var existingKeys = await reportRepository.GetAll()
            .Select(r => r.ReportKey)
            .ToListAsync();
        var existing = new HashSet<string>(existingKeys, StringComparer.OrdinalIgnoreCase);

        var created = 0;
        foreach (var dto in Catalog())
        {
            if (existing.Contains(dto.ReportKey)) continue;
            await saveReport.SaveAsync(dto);
            created++;
        }

        logger.LogInformation("Standard report seeding created {Created} report(s)", created);
        return created;
    }
}
