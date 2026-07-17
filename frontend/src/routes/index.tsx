import { Routes, Route } from "react-router-dom";
import { lazy, memo } from "react";
import ProtectedRoute from "@/components/common/protectedRoute";
const LoginPage = memo(lazy(() => import("@/pages/auth/login/page")));
const LoginOutPage = memo(lazy(() => import("@/pages/auth/logout/page")));
const RegisterPage = memo(lazy(() => import("@/pages/auth/register/page")));
const LandingPage = memo(lazy(() => import("@/pages/home/landingPage")));
const HomePage = memo(lazy(() => import("@/pages/home/homePage")));
const Dashboard = memo(lazy(() => import("@/pages/home/dashboard.tsx")));
const ModulePage = memo(lazy(() => import("@/pages/admin/module")));
const OperationPage = memo(lazy(() => import("@/pages/admin/operation")));
const RolePage = memo(lazy(() => import("@/pages/admin/role")));
const RolePermissionPage = memo(
  lazy(() => import("@/pages/admin/rolePermission")),
);
const UserPage = memo(lazy(() => import("@/pages/admin/user")));
const UserRolePage = memo(lazy(() => import("@/pages/admin/userRole")));
// Organizational Structure (HRMS §3.1)
const OrganizationUnitPage = memo(lazy(() => import("@/pages/admin/organizationUnit")));
const PositionPage = memo(lazy(() => import("@/pages/admin/position")));
const PositionClassPage = memo(lazy(() => import("@/pages/admin/positionClass")));
const JobGradePage = memo(lazy(() => import("@/pages/admin/jobGrade")));
const SalaryScalePage = memo(lazy(() => import("@/pages/admin/salaryScale")));
const LeaveTypePage = memo(lazy(() => import("@/pages/admin/leaveType")));
const HolidayPage = memo(lazy(() => import("@/pages/admin/holiday")));
const LeaveRequestPage = memo(lazy(() => import("@/pages/admin/leaveRequest")));
const LeaveBalancePage = memo(lazy(() => import("@/pages/admin/leaveBalance")));
const FiscalYearPage = memo(lazy(() => import("@/pages/admin/fiscalYear")));
const AnnualLeaveSettingPage = memo(lazy(() => import("@/pages/admin/annualLeaveSetting")));
const AnnualLeaveLedgerPage = memo(lazy(() => import("@/pages/admin/annualLeaveLedger")));
const WorkWeekConfigurationPage = memo(lazy(() => import("@/pages/admin/workWeekConfiguration")));
const AnnualLeavePage = memo(lazy(() => import("@/pages/admin/annualLeave")));
const ReportViewerPage = memo(lazy(() => import("@/pages/admin/reportViewer")));
const ReportDefinitionPage = memo(lazy(() => import("@/pages/admin/reportDefinition")));
const ReportResultPage = memo(lazy(() => import("@/pages/admin/reportResult")));
const JobCategoryPage = memo(lazy(() => import("@/pages/admin/jobCategory")));
const WorkLocationPage = memo(lazy(() => import("@/pages/admin/workLocation")));
const BranchPage = memo(lazy(() => import("@/pages/admin/branch")));
const AuditLogPage = memo(lazy(() => import("@/pages/admin/auditLog")));
const EmployeePage = memo(lazy(() => import("@/pages/admin/employee")));
const EmployeeFieldPage = memo(lazy(() => import("@/pages/admin/employeeField")));
const FormBuilderPage = memo(lazy(() => import("@/pages/admin/formBuilder")));
const DocumentTemplatePage = memo(lazy(() => import("@/pages/admin/documentTemplate")));
const WorkflowPage = memo(lazy(() => import("@/pages/admin/workflow")));
const WorkflowDefinitionPage = memo(lazy(() => import("@/pages/admin/workflowDefinition")));
const TerminationListPage = memo(lazy(() => import("@/pages/admin/terminationList")));
const ClearanceDepartmentPage = memo(lazy(() => import("@/pages/admin/clearanceDepartment")));
const WorkforcePlanPage = memo(lazy(() => import("@/pages/admin/workforcePlan")));
const EstablishmentOverviewPage = memo(lazy(() => import("@/pages/admin/establishmentOverview")));
const HiringRequestPage = memo(lazy(() => import("@/pages/admin/hiringRequest")));
const JobRequisitionPage = memo(lazy(() => import("@/pages/admin/jobRequisition")));
const CandidatePage = memo(lazy(() => import("@/pages/admin/candidate")));
const JobApplicationPage = memo(lazy(() => import("@/pages/admin/jobApplication")));
const TalentPoolPage = memo(lazy(() => import("@/pages/admin/talentPool")));
const HireEmployeePage = memo(lazy(() => import("@/pages/admin/hireEmployee")));
const OfferLetterTemplatePage = memo(lazy(() => import("@/pages/admin/offerLetterTemplate")));
const RatingScalePage = memo(lazy(() => import("@/pages/admin/ratingScale")));
const CompetencyCategoryPage = memo(lazy(() => import("@/pages/admin/competencyCategory")));
const CompetencyPage = memo(lazy(() => import("@/pages/admin/competency")));
const PositionCompetencyPage = memo(lazy(() => import("@/pages/admin/positionCompetency")));
const CriticalPositionPage = memo(lazy(() => import("@/pages/admin/criticalPosition")));
const TalentReviewPage = memo(lazy(() => import("@/pages/admin/talentReview")));
const SuccessionPlanPage = memo(lazy(() => import("@/pages/admin/successionPlan")));
const CareerPathPage = memo(lazy(() => import("@/pages/admin/careerPath")));
const EmployeeCareerPathPage = memo(lazy(() => import("@/pages/admin/employeeCareerPath")));
const MentorshipPage = memo(lazy(() => import("@/pages/admin/mentorship")));
const CareerPathChangeRequestPage = memo(lazy(() => import("@/pages/admin/careerPathChangeRequest")));
const ReviewCyclePage = memo(lazy(() => import("@/pages/admin/reviewCycle")));
const AppraisalTemplatePage = memo(lazy(() => import("@/pages/admin/appraisalTemplate")));
const OrganizationalObjectivePage = memo(lazy(() => import("@/pages/admin/organizationalObjective")));
const EmployeeGoalPage = memo(lazy(() => import("@/pages/admin/employeeGoal")));
const AppraisalPage = memo(lazy(() => import("@/pages/admin/appraisal")));
const CalibrationPage = memo(lazy(() => import("@/pages/admin/calibration")));
const DevelopmentPlanPage = memo(lazy(() => import("@/pages/admin/developmentPlan")));
const ImprovementPlanPage = memo(lazy(() => import("@/pages/admin/improvementPlan")));
const AchievementPage = memo(lazy(() => import("@/pages/admin/achievement")));
const RecognitionBadgePage = memo(lazy(() => import("@/pages/admin/recognitionBadge")));
const RecognitionPage = memo(lazy(() => import("@/pages/admin/recognition")));
const AppraisalAppealPage = memo(lazy(() => import("@/pages/admin/appraisalAppeal")));
const PerformanceDashboardPage = memo(lazy(() => import("@/pages/admin/performanceDashboard")));
export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/logout" element={<LoginOutPage />} />
      {/* Generated report opens in a NEW TAB as a dedicated FULL-SCREEN grid — no app shell
          (header / sidebar / footer / nav). Kept OUTSIDE the HomePage layout on purpose. */}
      <Route
        path="/reportResult"
        element={
          <ProtectedRoute>
            <ReportResultPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/landing"
        element={
          <ProtectedRoute>
            <LandingPage />
          </ProtectedRoute>
        }
      />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <HomePage />
          </ProtectedRoute>
        }
      >
        <Route index element={<Dashboard />} />
        <Route path="module" element={<ModulePage />} />
        <Route path="operation" element={<OperationPage />} />
        <Route path="role" element={<RolePage />} />
        <Route path="rolePermission" element={<RolePermissionPage />} />
        <Route path="user" element={<UserPage />} />
        <Route path="userRole" element={<UserRolePage />} />
        {/* Organizational Structure (HRMS §3.1) */}
        <Route path="organizationUnit" element={<OrganizationUnitPage />} />
        <Route path="position" element={<PositionPage />} />
        <Route path="positionClass" element={<PositionClassPage />} />
        <Route path="jobGrade" element={<JobGradePage />} />
        <Route path="salaryScale" element={<SalaryScalePage />} />
        <Route path="leaveType" element={<LeaveTypePage />} />
        <Route path="holiday" element={<HolidayPage />} />
        <Route path="leaveRequest" element={<LeaveRequestPage />} />
        <Route path="leaveBalance" element={<LeaveBalancePage />} />
        <Route path="fiscalYear" element={<FiscalYearPage />} />
        <Route path="annualLeaveSetting" element={<AnnualLeaveSettingPage />} />
        <Route path="workWeekConfiguration" element={<WorkWeekConfigurationPage />} />
        <Route path="annualLeave" element={<AnnualLeavePage />} />
        <Route path="reports" element={<ReportViewerPage />} />
        <Route path="reportDefinition" element={<ReportDefinitionPage />} />
        <Route path="annualLeaveLedger" element={<AnnualLeaveLedgerPage />} />
        <Route path="jobCategory" element={<JobCategoryPage />} />
        <Route path="workLocation" element={<WorkLocationPage />} />
        <Route path="branch" element={<BranchPage />} />
        <Route path="auditLog" element={<AuditLogPage />} />
        <Route path="employee" element={<EmployeePage />} />
        <Route path="employeeField" element={<EmployeeFieldPage />} />
        <Route path="formBuilder" element={<FormBuilderPage />} />
        <Route path="documentTemplate" element={<DocumentTemplatePage />} />
        <Route path="workflow" element={<WorkflowPage />} />
        <Route path="workflowDefinition" element={<WorkflowDefinitionPage />} />
        <Route path="terminationList" element={<TerminationListPage />} />
        <Route path="clearanceDepartment" element={<ClearanceDepartmentPage />} />
        <Route path="workforcePlan" element={<WorkforcePlanPage />} />
        <Route path="establishmentOverview" element={<EstablishmentOverviewPage />} />
        <Route path="hiringRequest" element={<HiringRequestPage />} />
        <Route path="jobRequisition" element={<JobRequisitionPage />} />
        <Route path="candidate" element={<CandidatePage />} />
        <Route path="jobApplication" element={<JobApplicationPage />} />
        <Route path="talentPool" element={<TalentPoolPage />} />
        <Route path="hireEmployee" element={<HireEmployeePage />} />
        <Route path="offerLetterTemplate" element={<OfferLetterTemplatePage />} />
        {/* Performance Management (HRMS §3.6) — Phase A configuration */}
        <Route path="ratingScale" element={<RatingScalePage />} />
        <Route path="competencyCategory" element={<CompetencyCategoryPage />} />
        <Route path="criticalPosition" element={<CriticalPositionPage />} />
        <Route path="talentReview" element={<TalentReviewPage />} />
        <Route path="successionPlan" element={<SuccessionPlanPage />} />
        <Route path="careerPath" element={<CareerPathPage />} />
        <Route path="employeeCareerPath" element={<EmployeeCareerPathPage />} />
        <Route path="mentorship" element={<MentorshipPage />} />
        <Route path="careerPathChangeRequest" element={<CareerPathChangeRequestPage />} />
        <Route path="competency" element={<CompetencyPage />} />
        <Route path="positionCompetency" element={<PositionCompetencyPage />} />
        <Route path="reviewCycle" element={<ReviewCyclePage />} />
        <Route path="appraisalTemplate" element={<AppraisalTemplatePage />} />
        <Route path="organizationalObjective" element={<OrganizationalObjectivePage />} />
        <Route path="employeeGoal" element={<EmployeeGoalPage />} />
        <Route path="appraisal" element={<AppraisalPage />} />
        <Route path="calibration" element={<CalibrationPage />} />
        <Route path="developmentPlan" element={<DevelopmentPlanPage />} />
        <Route path="improvementPlan" element={<ImprovementPlanPage />} />
        <Route path="achievement" element={<AchievementPage />} />
        <Route path="recognitionBadge" element={<RecognitionBadgePage />} />
        <Route path="recognition" element={<RecognitionPage />} />
        <Route path="appraisalAppeal" element={<AppraisalAppealPage />} />
        <Route path="performanceDashboard" element={<PerformanceDashboardPage />} />
       </Route>
    </Routes>
  );
}
