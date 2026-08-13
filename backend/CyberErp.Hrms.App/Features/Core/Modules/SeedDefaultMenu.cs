using CyberErp.Hrms.App.Common.Services;
using CyberErp.Hrms.App.Common.Authorization;
using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules;

/// <summary>
/// Seeds the dynamic navigation tables (Core.Subsystem / Core.Module / Core.Operation) for the
/// CURRENT tenant with the HRMS menu that was previously hardcoded in the frontend sidebar.
/// Idempotent: subsystems match by name, modules by (subsystem, name), operations by (module, link) —
/// existing rows are left untouched so tenant-specific menu customizations survive re-seeding.
/// Icon values are lucide-react icon names resolved dynamically by the frontend.
/// </summary>
public interface ISeedDefaultMenu { Task<int> SeedAsync(); }

public class SeedDefaultMenu(
    IRepository<Subsystem> subsystemRepository,
    IRepository<Module> moduleRepository,
    IRepository<Operation> operationRepository,
    IRepository<TenantOperation> tenantOperationRepository,
    ICurrentTenantService currentTenant,
    IUnitOfWork unitOfWork,
    ITenantAuthorizationProjector projector,
    ILogger<SeedDefaultMenu> logger) : ISeedDefaultMenu
{
    private const string HrmsSubsystem = "HRMS";

    private sealed record Op(string Name, string Link, string Icon);
    private sealed record Mod(string Name, string Icon, Op[] Operations);

    // Mirrors the former static NAV_GROUPS of the frontend sidebar, in display order.
    private static readonly Mod[] Menu =
    [
        new("Personnel", "UsersRound",
        [
            new("Employees", "/employee", "Users"),
            new("My Exit", "/myExit", "DoorOpen"),
            new("Transfer Requests", "/transferRequest", "ArrowLeftRight"),
            new("Disciplinary Cases", "/disciplinaryCase", "ShieldAlert"),
            new("Termination List", "/terminationList", "UserX"),
            new("Company Assets", "/companyAsset", "Package"),
            new("Exit Questionnaire", "/exitQuestionnaire", "ClipboardList"),
            new("Custom Fields", "/employeeField", "ListPlus"),
            new("Document Templates", "/documentTemplate", "FileText"),
        ]),
        new("Organization", "Building2",
        [
            new("Branches", "/branch", "Building"),
            new("Organization Structure", "/organizationUnit", "Network"),
            new("Position Classes", "/positionClass", "BriefcaseBusiness"),
            new("Positions", "/position", "Briefcase"),
            new("Job Grades", "/jobGrade", "Layers"),
            new("Salary Scale", "/salaryScale", "Coins"),
            new("Job Categories", "/jobCategory", "Tags"),
            new("Work Locations", "/workLocation", "MapPin"),
        ]),
        new("Planning", "Target",
        [
            new("Workforce Plans", "/workforcePlan", "ClipboardList"),
            new("Establishment Overview", "/establishmentOverview", "LayoutGrid"),
        ]),
        new("Recruitment", "UserPlus",
        [
            new("Hiring Requests", "/hiringRequest", "FilePlus2"),
            new("Job Requisitions", "/jobRequisition", "Megaphone"),
            new("Candidates", "/candidate", "Users"),
            new("Applications", "/jobApplication", "ClipboardList"),
            new("Hire Employee", "/hireEmployee", "UserCheck"),
            new("Talent Pool", "/talentPool", "Star"),
            new("Offer Letter Template", "/offerLetterTemplate", "ScrollText"),
        ]),
        new("Performance", "Award",
        [
            new("Performance Dashboard", "/performanceDashboard", "BarChart3"),
            new("Organizational Objectives", "/organizationalObjective", "Goal"),
            new("Employee Goals", "/employeeGoal", "ListChecks"),
            new("Appraisals", "/appraisal", "ClipboardCheck"),
            new("My Peer Reviews", "/myPeerReviews", "Users"),
            new("Appeals", "/appraisalAppeal", "Gavel"),
            new("Calibration", "/calibration", "Scale"),
            new("Development Plans", "/developmentPlan", "GraduationCap"),
            new("Improvement Plans", "/improvementPlan", "TrendingUp"),
            new("Achievements", "/achievement", "Medal"),
            new("Recognition", "/recognition", "Sparkles"),
            new("Recognition Wall", "/recognitionWall", "Trophy"),
            new("My Points", "/myPoints", "Coins"),
            new("Award Nominations", "/rewardNomination", "ThumbsUp"),
            new("Recognition Badges", "/recognitionBadge", "Award"),
            new("Award Categories", "/awardCategory", "Tags"),
            new("Recognition Programs", "/recognitionProgram", "CalendarRange"),
            new("Reward Payouts", "/rewardDisbursement", "Banknote"),
            new("Review Cycles", "/reviewCycle", "CalendarClock"),
            new("Appraisal Templates", "/appraisalTemplate", "ClipboardType"),
            new("Rating Scales", "/ratingScale", "Gauge"),
            new("Competencies", "/competency", "Sparkles"),
            new("Competency Categories", "/competencyCategory", "Shapes"),
            new("Position Competencies", "/positionCompetency", "Target"),
        ]),
        new("Career Development", "Rocket",
        [
            new("Critical Positions", "/criticalPosition", "ShieldAlert"),
            new("Talent Reviews", "/talentReview", "Grid3x3"),
            new("Succession Plans", "/successionPlan", "GitBranchPlus"),
            new("Career Paths", "/careerPath", "Route"),
            new("Employee Career Paths", "/employeeCareerPath", "UserRoundCog"),
            new("Mentorships", "/mentorship", "Handshake"),
            new("Path Change Requests", "/careerPathChangeRequest", "GitPullRequestArrow"),
        ]),
        new("Learning", "GraduationCap",
        [
            new("My Training", "/myTraining", "UserCheck"),
            new("Communities", "/learningCommunity", "UsersRound"),
            new("Training Needs", "/trainingNeed", "ClipboardList"),
            new("Training Sessions", "/trainingSession", "CalendarDays"),
            new("Course Catalog", "/trainingCourse", "BookOpenCheck"),
            new("Training Categories", "/trainingCategory", "Tags"),
            new("Learning Paths", "/learningPath", "Route"),
            new("Certifications", "/trainingCertificate", "ScrollText"),
            new("Training Budgets", "/trainingBudget", "Wallet"),
            new("Provider Payments", "/trainingProviderPayment", "Banknote"),
        ]),
        new("Engagement", "HeartHandshake",
        [
            new("News & Announcements", "/newsFeed", "Newspaper"),
            new("Surveys & Polls", "/surveyTake", "Vote"),
            new("Suggestions", "/suggestion", "Lightbulb"),
            new("Grievances", "/grievance", "ShieldAlert"),
            new("Manage Announcements", "/announcement", "Megaphone"),
            new("Survey Builder", "/survey", "ClipboardList"),
        ]),
        new("Compensation", "Wallet",
        [
            new("My Compensation", "/myCompensation", "Wallet"),
            new("Employee Compensation", "/employeeCompensation", "Coins"),
            new("Salary Revisions", "/salaryRevision", "TrendingUp"),
            new("Increment Rules", "/salaryIncrementPolicy", "SlidersHorizontal"),
            new("Benefit Plans", "/benefitPlan", "HeartPulse"),
            new("Allowance Types", "/allowanceType", "Coins"),
            new("Income Tax & Deductions", "/taxBracket", "Landmark"),
            new("Compensation Requests", "/compensationRequest", "MessageSquareWarning"),
        ]),
        new("Medical Benefit", "HeartPulse",
        [
            new("My Medical Claims", "/myMedicalClaims", "HeartPulse"),
            new("Medical Claims", "/medicalClaim", "Receipt"),
            new("Medical Enrollment", "/medicalEnrollment", "ClipboardPlus"),
            new("Medical Plans", "/medicalPlan", "ShieldPlus"),
            new("Medical Providers", "/medicalProvider", "Stethoscope"),
            new("Service Contracts", "/medicalContract", "FileSignature"),
        ]),
        new("Insurance", "ShieldCheck",
        [
            new("My Insurance Claims", "/myInsuranceClaims", "FileHeart"),
            new("Insurance Claims", "/insuranceClaim", "FileHeart"),
            new("Insurance Policies", "/insurancePolicy", "ShieldCheck"),
        ]),
        new("Employee Loan", "HandCoins",
        [
            new("My Loans", "/myLoans", "HandCoins"),
            new("Employee Loans", "/loan", "HandCoins"),
            new("Loan Types", "/loanType", "Landmark"),
        ]),
        new("Guarantee Commitments", "Handshake",
        [
            new("My Guarantees", "/myGuarantees", "Handshake"),
            new("Guarantee Register", "/employeeGuarantee", "ScrollText"),
        ]),
        new("Trip Management", "Plane",
        [
            new("My Trips", "/myTrips", "Plane"),
            new("Business Trips", "/trip", "Plane"),
            new("Travel Budgets", "/tripBudget", "Wallet"),
            new("Per-diem Rates", "/perDiemRate", "Coins"),
        ]),
        new("Attendance & Leave", "CalendarRange",
        [
            new("Annual Leave", "/annualLeave", "CalendarCheck"),
            new("Other Leave", "/otherLeave", "CalendarHeart"),
            new("Other Leave Settings", "/otherLeaveSetting", "CalendarCog"),
            new("Annual Leave Ledger", "/annualLeaveLedger", "BookOpenCheck"),
            new("Leave Types", "/leaveType", "CalendarDays"),
            new("Leave Settings", "/annualLeaveSetting", "SlidersHorizontal"),
            new("Holidays", "/holiday", "CalendarClock"),
            new("Work Week", "/workWeekConfiguration", "CalendarRange"),
            new("Fiscal Years", "/fiscalYear", "CalendarCog"),
        ]),
        new("Reports", "BarChart3",
        [
            new("Reports", "/reports", "BarChart3"),
            new("Report Definitions", "/reportDefinition", "SlidersHorizontal"),
        ]),
        new("System", "ShieldCheck",
        [
            new("Workflow Tracking", "/workflow", "GitPullRequestArrow"),
            new("Workflow Definitions", "/workflowDefinition", "GitBranch"),
            // Operations settings: the SMTP relay and backup schedule. Gated on this link, so a
            // deployment without it cannot reach the endpoints at all — deliberate for a screen that
            // redirects the organisation's mail.
            new("Settings", "/setting", "Settings"),
            new("Clearance Departments", "/clearanceDepartment", "ClipboardCheck"),
            new("Form Builder", "/formBuilder", "LayoutGrid"),
            new("Users", "/user", "UserCog"),
            new("Roles", "/role", "KeyRound"),
            new("User Roles", "/userRole", "UserCheck"),
            new("Role Permissions", "/rolePermission", "ShieldCheck"),
            new("Subsystems", "/subsystem", "Boxes"),
            new("Menu Modules", "/module", "PanelsTopLeft"),
            new("Menu Operations", "/operation", "ListTree"),
            new("Audit Trail", "/auditLog", "ScrollText"),
        ]),
    ];

    public async Task<int> SeedAsync()
    {
        var created = 0;

        var hrms = await subsystemRepository.GetAll().FirstOrDefaultAsync(s => s.Name == HrmsSubsystem);
        if (hrms is null)
        {
            hrms = Subsystem.Create(HrmsSubsystem, "HRMS", 1);
            await subsystemRepository.AddAsync(hrms);
            created++;
        }

        var existingModules = await moduleRepository.GetAll()
            .Where(m => m.SubsystemId == hrms.Id)
            .ToListAsync();
        var existingOperations = await operationRepository.GetAll().ToListAsync();

        for (var mi = 0; mi < Menu.Length; mi++)
        {
            var def = Menu[mi];

            // The menu group is an OPERATION with no parent. A Core.Module row is still written
            // alongside it, sharing the SAME Id: SubscriptionPlanModule and TenantSubscriptionAddOn
            // have foreign keys into that table, and the migration established parent-Id == Module-Id
            // as an invariant. Navigation itself reads only the operation hierarchy.
            var group = existingOperations.FirstOrDefault(o => o.ModuleId == null && o.Name == def.Name);
            if (group is null)
            {
                group = Operation.CreateParent(hrms.Id, def.Name, def.Icon, (mi + 1) * 10);
                await operationRepository.AddAsync(group);
                created++;
            }

            if (!existingModules.Any(m => m.Name == def.Name))
            {
                await moduleRepository.AddAsync(
                    Module.CreateWithId(group.Id, hrms.Id, def.Name, def.Icon, (mi + 1) * 10));
                created++;
            }

            for (var oi = 0; oi < def.Operations.Length; oi++)
            {
                var op = def.Operations[oi];
                var exists = existingOperations.Any(o => o.ModuleId == group.Id && o.Link == op.Link);
                if (exists) continue;

                // hrms.Id is the subsystem every seeded group hangs off, so the denormalised
                // SubSystemId is known here without a lookup.
                await operationRepository.AddAsync(
                    Operation.Create(group.Id, op.Name, op.Link, string.Empty, op.Icon,
                        (oi + 1) * 10, hrms.Id));
                created++;
            }
        }

        if (created > 0)
        {
            await unitOfWork.SaveChangesAsync();

            // ⚠️ Tenant copies are made HERE, not by the projector. Core.Operation went global on
            // 2026-08-13, so the projector only updates copies that already exist — a seeded
            // operation with no copy is a screen nobody can be granted.
            var tenantId = currentTenant.GetCurrentTenantId();
            if (tenantId is not null && tenantId != Guid.Empty)
            {
                var copied = await tenantOperationRepository.GetAll()
                    .Select(o => o.OperationId).ToListAsync();
                foreach (var op in await operationRepository.GetAll()
                             .Where(o => !copied.Contains(o.Id)).ToListAsync())
                {
                    await tenantOperationRepository.AddAsync(TenantOperation.Create(
                        tenantId.Value, op.SubSystemId, op.Id, op.ModuleId,
                        op.Name, op.Link, op.Icon, op.DisplayOrder, op.IsActive));
                }
                await unitOfWork.SaveChangesAsync();
            }

            await projector.SyncAsync();
            logger.LogInformation("Seeded {Count} navigation rows (subsystem/modules/operations)", created);
        }

        return created;
    }
}
