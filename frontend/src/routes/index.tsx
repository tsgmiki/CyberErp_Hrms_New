import { Routes, Route } from "react-router-dom";
import { lazy, memo } from "react";
import ProtectedRoute from "@/components/common/protectedRoute";
import PermissionGate from "@/components/common/permissionGate";
const UnauthorizedPage = memo(lazy(() => import("@/pages/home/unauthorized")));
const LoginPage = memo(lazy(() => import("@/pages/auth/login/page")));
const LoginOutPage = memo(lazy(() => import("@/pages/auth/logout/page")));
const RegisterPage = memo(lazy(() => import("@/pages/auth/register/page")));
const LandingPage = memo(lazy(() => import("@/pages/home/landingPage")));
const HomePage = memo(lazy(() => import("@/pages/home/homePage")));
const Dashboard = memo(lazy(() => import("@/pages/home/dashboard.tsx")));
const ModulePage = memo(lazy(() => import("@/pages/admin/module")));
const OperationPage = memo(lazy(() => import("@/pages/admin/operation")));
const SubsystemPage = memo(lazy(() => import("@/pages/admin/subsystem")));
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
const TransferRequestPage = memo(lazy(() => import("@/pages/admin/transferRequest")));
const DisciplinaryCasePage = memo(lazy(() => import("@/pages/admin/disciplinaryCase")));
// Compensation & Benefit (HRMS §3.10.1)
const AllowanceTypePage = memo(lazy(() => import("@/pages/admin/allowanceType")));
const BenefitPlanPage = memo(lazy(() => import("@/pages/admin/benefitPlan")));
const TaxBracketPage = memo(lazy(() => import("@/pages/admin/taxBracket")));
const SalaryRevisionPage = memo(lazy(() => import("@/pages/admin/salaryRevision")));
const EmployeeCompensationPage = memo(lazy(() => import("@/pages/admin/employeeCompensation")));
const MyCompensationPage = memo(lazy(() => import("@/pages/admin/myCompensation")));
const CompensationRequestPage = memo(lazy(() => import("@/pages/admin/compensationRequest")));
// Medical Benefit (HRMS §3.10.2)
const MedicalProviderPage = memo(lazy(() => import("@/pages/admin/medicalProvider")));
const MedicalPlanPage = memo(lazy(() => import("@/pages/admin/medicalPlan")));
const MedicalContractPage = memo(lazy(() => import("@/pages/admin/medicalContract")));
const MedicalEnrollmentPage = memo(lazy(() => import("@/pages/admin/medicalEnrollment")));
const MedicalClaimPage = memo(lazy(() => import("@/pages/admin/medicalClaim")));
const MyMedicalClaimsPage = memo(lazy(() => import("@/pages/admin/myMedicalClaims")));
// Insurance (HRMS §3.10.3)
const InsurancePolicyPage = memo(lazy(() => import("@/pages/admin/insurancePolicy")));
const InsuranceClaimPage = memo(lazy(() => import("@/pages/admin/insuranceClaim")));
const MyInsuranceClaimsPage = memo(lazy(() => import("@/pages/admin/myInsuranceClaims")));
// Employee Loan (HRMS §3.10.4)
const LoanTypePage = memo(lazy(() => import("@/pages/admin/loanType")));
const LoanPage = memo(lazy(() => import("@/pages/admin/loan")));
const MyLoansPage = memo(lazy(() => import("@/pages/admin/myLoans")));
// Trip Management (HRMS §3.10.5)
const PerDiemRatePage = memo(lazy(() => import("@/pages/admin/perDiemRate")));
const TripBudgetPage = memo(lazy(() => import("@/pages/admin/tripBudget")));
const TripPage = memo(lazy(() => import("@/pages/admin/trip")));
const MyTripsPage = memo(lazy(() => import("@/pages/admin/myTrips")));
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
const MyPeerReviewsPage = memo(lazy(() => import("@/pages/admin/myPeerReviews")));
const CalibrationPage = memo(lazy(() => import("@/pages/admin/calibration")));
const DevelopmentPlanPage = memo(lazy(() => import("@/pages/admin/developmentPlan")));
const ImprovementPlanPage = memo(lazy(() => import("@/pages/admin/improvementPlan")));
const AchievementPage = memo(lazy(() => import("@/pages/admin/achievement")));
const RecognitionBadgePage = memo(lazy(() => import("@/pages/admin/recognitionBadge")));
const RecognitionPage = memo(lazy(() => import("@/pages/admin/recognition")));
const AwardCategoryPage = memo(lazy(() => import("@/pages/admin/awardCategory")));
const RecognitionProgramPage = memo(lazy(() => import("@/pages/admin/recognitionProgram")));
const RewardNominationPage = memo(lazy(() => import("@/pages/admin/rewardNomination")));
const RecognitionWallPage = memo(lazy(() => import("@/pages/admin/recognitionWall")));
const MyPointsPage = memo(lazy(() => import("@/pages/admin/myPoints")));
const RewardDisbursementPage = memo(lazy(() => import("@/pages/admin/rewardDisbursement")));
const TrainingCategoryPage = memo(lazy(() => import("@/pages/admin/trainingCategory")));
const TrainingCoursePage = memo(lazy(() => import("@/pages/admin/trainingCourse")));
const TrainingNeedPage = memo(lazy(() => import("@/pages/admin/trainingNeed")));
const TrainingSessionPage = memo(lazy(() => import("@/pages/admin/trainingSession")));
const TrainingBudgetPage = memo(lazy(() => import("@/pages/admin/trainingBudget")));
const LearningPathPage = memo(lazy(() => import("@/pages/admin/learningPath")));
const TrainingCertificatePage = memo(lazy(() => import("@/pages/admin/trainingCertificate")));
const TrainingProviderPaymentPage = memo(lazy(() => import("@/pages/admin/trainingProviderPayment")));
const MyTrainingPage = memo(lazy(() => import("@/pages/admin/myTraining")));
const LearningCommunityPage = memo(lazy(() => import("@/pages/admin/learningCommunity")));
const CompanyAssetPage = memo(lazy(() => import("@/pages/admin/companyAsset")));
const MyExitPage = memo(lazy(() => import("@/pages/admin/myExit")));
const ExitQuestionnairePage = memo(lazy(() => import("@/pages/admin/exitQuestionnaire")));
const AppraisalAppealPage = memo(lazy(() => import("@/pages/admin/appraisalAppeal")));
// Employee Engagement (HRMS §3.9.1)
const SuggestionPage = memo(lazy(() => import("@/pages/admin/suggestion")));
const GrievancePage = memo(lazy(() => import("@/pages/admin/grievance")));
const AnnouncementPage = memo(lazy(() => import("@/pages/admin/announcement")));
const NewsFeedPage = memo(lazy(() => import("@/pages/admin/newsFeed")));
const SurveyPage = memo(lazy(() => import("@/pages/admin/survey")));
const SurveyTakePage = memo(lazy(() => import("@/pages/admin/surveyTake")));
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
        <Route path="unauthorized" element={<UnauthorizedPage />} />
        {/* Everything below is role-permission gated: a direct URL to an operation the
            user's role lacks CanView for redirects to /unauthorized (PermissionGate). */}
        <Route element={<PermissionGate />}>
        <Route path="module" element={<ModulePage />} />
        <Route path="operation" element={<OperationPage />} />
        <Route path="subsystem" element={<SubsystemPage />} />
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
        <Route path="transferRequest" element={<TransferRequestPage />} />
        <Route path="disciplinaryCase" element={<DisciplinaryCasePage />} />
        {/* Compensation & Benefit (HRMS §3.10.1) */}
        <Route path="allowanceType" element={<AllowanceTypePage />} />
        <Route path="benefitPlan" element={<BenefitPlanPage />} />
        <Route path="taxBracket" element={<TaxBracketPage />} />
        <Route path="salaryRevision" element={<SalaryRevisionPage />} />
        <Route path="employeeCompensation" element={<EmployeeCompensationPage />} />
        <Route path="myCompensation" element={<MyCompensationPage />} />
        <Route path="compensationRequest" element={<CompensationRequestPage />} />
        {/* Medical Benefit (HRMS §3.10.2) */}
        <Route path="medicalProvider" element={<MedicalProviderPage />} />
        <Route path="medicalPlan" element={<MedicalPlanPage />} />
        <Route path="medicalContract" element={<MedicalContractPage />} />
        <Route path="medicalEnrollment" element={<MedicalEnrollmentPage />} />
        <Route path="medicalClaim" element={<MedicalClaimPage />} />
        <Route path="myMedicalClaims" element={<MyMedicalClaimsPage />} />
        {/* Insurance (HRMS §3.10.3) */}
        <Route path="insurancePolicy" element={<InsurancePolicyPage />} />
        <Route path="insuranceClaim" element={<InsuranceClaimPage />} />
        <Route path="myInsuranceClaims" element={<MyInsuranceClaimsPage />} />
        {/* Employee Loan (HRMS §3.10.4) */}
        <Route path="loanType" element={<LoanTypePage />} />
        <Route path="loan" element={<LoanPage />} />
        <Route path="myLoans" element={<MyLoansPage />} />
        {/* Trip Management (HRMS §3.10.5) */}
        <Route path="perDiemRate" element={<PerDiemRatePage />} />
        <Route path="tripBudget" element={<TripBudgetPage />} />
        <Route path="trip" element={<TripPage />} />
        <Route path="myTrips" element={<MyTripsPage />} />
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
        <Route path="myPeerReviews" element={<MyPeerReviewsPage />} />
        <Route path="calibration" element={<CalibrationPage />} />
        <Route path="developmentPlan" element={<DevelopmentPlanPage />} />
        <Route path="improvementPlan" element={<ImprovementPlanPage />} />
        <Route path="achievement" element={<AchievementPage />} />
        <Route path="recognitionBadge" element={<RecognitionBadgePage />} />
        <Route path="recognition" element={<RecognitionPage />} />
        <Route path="awardCategory" element={<AwardCategoryPage />} />
        <Route path="recognitionProgram" element={<RecognitionProgramPage />} />
        <Route path="rewardNomination" element={<RewardNominationPage />} />
        <Route path="recognitionWall" element={<RecognitionWallPage />} />
        <Route path="myPoints" element={<MyPointsPage />} />
        <Route path="rewardDisbursement" element={<RewardDisbursementPage />} />
        <Route path="trainingCategory" element={<TrainingCategoryPage />} />
        <Route path="trainingCourse" element={<TrainingCoursePage />} />
        <Route path="trainingNeed" element={<TrainingNeedPage />} />
        <Route path="trainingSession" element={<TrainingSessionPage />} />
        <Route path="trainingBudget" element={<TrainingBudgetPage />} />
        <Route path="learningPath" element={<LearningPathPage />} />
        <Route path="trainingCertificate" element={<TrainingCertificatePage />} />
        <Route path="trainingProviderPayment" element={<TrainingProviderPaymentPage />} />
        <Route path="myTraining" element={<MyTrainingPage />} />
        <Route path="learningCommunity" element={<LearningCommunityPage />} />
        <Route path="companyAsset" element={<CompanyAssetPage />} />
        <Route path="myExit" element={<MyExitPage />} />
        <Route path="exitQuestionnaire" element={<ExitQuestionnairePage />} />
        <Route path="appraisalAppeal" element={<AppraisalAppealPage />} />
        {/* Employee Engagement (HRMS §3.9.1) */}
        <Route path="suggestion" element={<SuggestionPage />} />
        <Route path="grievance" element={<GrievancePage />} />
        <Route path="announcement" element={<AnnouncementPage />} />
        <Route path="newsFeed" element={<NewsFeedPage />} />
        <Route path="survey" element={<SurveyPage />} />
        <Route path="surveyTake" element={<SurveyTakePage />} />
        <Route path="performanceDashboard" element={<PerformanceDashboardPage />} />
        </Route>
       </Route>
    </Routes>
  );
}
