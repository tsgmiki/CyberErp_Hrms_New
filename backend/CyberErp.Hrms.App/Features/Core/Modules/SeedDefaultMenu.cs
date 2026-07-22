using CyberErp.Hrms.App.Common.Repositories;
using CyberErp.Hrms.Dom.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CyberErp.Hrms.App.Features.Core.Modules;

/// <summary>
/// Seeds the dynamic navigation tables (dbo.coreSubsystem / dbo.coreModule / dbo.coreOperation) for the
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
    IUnitOfWork unitOfWork,
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
            var module = existingModules.FirstOrDefault(m => m.Name == def.Name);
            if (module is null)
            {
                module = Module.Create(hrms.Id, def.Name, def.Icon, (mi + 1) * 10);
                await moduleRepository.AddAsync(module);
                created++;
            }

            for (var oi = 0; oi < def.Operations.Length; oi++)
            {
                var op = def.Operations[oi];
                var exists = existingOperations.Any(o => o.ModuleId == module.Id && o.Link == op.Link);
                if (exists) continue;

                await operationRepository.AddAsync(
                    Operation.Create(module.Id, op.Name, op.Link, string.Empty, op.Icon, (oi + 1) * 10));
                created++;
            }
        }

        if (created > 0)
        {
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} navigation rows (subsystem/modules/operations)", created);
        }

        return created;
    }
}
