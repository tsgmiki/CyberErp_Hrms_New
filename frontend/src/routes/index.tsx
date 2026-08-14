import { Routes, Route } from "react-router-dom";
import { lazy, memo } from "react";
import ProtectedRoute from "@/components/common/protectedRoute";
import PermissionGate from "@/components/common/permissionGate";
import { ENTITY_ROUTES, renderEntityRoutes } from "./entityRoutes";
// Static, not lazy: EntityRecordGuard imports it directly, so it lands in the main chunk
// either way and a lazy wrapper here would only add a pointless Suspense boundary.
import NotFoundPage from "@/pages/home/notFound";
const UnauthorizedPage = memo(lazy(() => import("@/pages/home/unauthorized")));
const LoginPage = memo(lazy(() => import("@/pages/auth/login/page")));
const LoginOutPage = memo(lazy(() => import("@/pages/auth/logout/page")));
const RegisterPage = memo(lazy(() => import("@/pages/auth/register/page")));
const LandingPage = memo(lazy(() => import("@/pages/home/landingPage")));
const HomePage = memo(lazy(() => import("@/pages/home/homePage")));
const Dashboard = memo(lazy(() => import("@/pages/home/dashboard.tsx")));
// Organizational Structure (HRMS §3.1)
const ReportViewerPage = memo(lazy(() => import("@/pages/admin/reportViewer")));
const ReportResultPage = memo(lazy(() => import("@/pages/admin/reportResult")));
const AuditLogPage = memo(lazy(() => import("@/pages/admin/auditLog")));
// Compensation & Benefit (HRMS §3.10.1)
const TaxBracketPage = memo(lazy(() => import("@/pages/admin/taxBracket")));
const SalaryIncrementPolicyPage = memo(lazy(() => import("@/pages/admin/salaryIncrementPolicy")));
const EmployeeCompensationPage = memo(lazy(() => import("@/pages/admin/employeeCompensation")));
const MyCompensationPage = memo(lazy(() => import("@/pages/admin/myCompensation")));
const CompensationRequestPage = memo(lazy(() => import("@/pages/admin/compensationRequest")));
// Medical Benefit (HRMS §3.10.2)
const MedicalEnrollmentPage = memo(lazy(() => import("@/pages/admin/medicalEnrollment")));
const MedicalClaimPage = memo(lazy(() => import("@/pages/admin/medicalClaim")));
// Insurance (HRMS §3.10.3)
const InsuranceClaimPage = memo(lazy(() => import("@/pages/admin/insuranceClaim")));
// Employee Loan (HRMS §3.10.4)
// Guarantee Commitments (HRMS §3.12)
// Trip Management (HRMS §3.10.5)
const TripPage = memo(lazy(() => import("@/pages/admin/trip")));
const WorkflowPage = memo(lazy(() => import("@/pages/admin/workflow")));
const TerminationListPage = memo(lazy(() => import("@/pages/admin/terminationList")));
const EstablishmentOverviewPage = memo(lazy(() => import("@/pages/admin/establishmentOverview")));
const JobApplicationPage = memo(lazy(() => import("@/pages/admin/jobApplication")));
const TalentPoolPage = memo(lazy(() => import("@/pages/admin/talentPool")));
const HireEmployeePage = memo(lazy(() => import("@/pages/admin/hireEmployee")));
const OfferLetterTemplatePage = memo(lazy(() => import("@/pages/admin/offerLetterTemplate")));
const PositionCompetencyPage = memo(lazy(() => import("@/pages/admin/positionCompetency")));
const MyPeerReviewsPage = memo(lazy(() => import("@/pages/admin/myPeerReviews")));
const RecognitionWallPage = memo(lazy(() => import("@/pages/admin/recognitionWall")));
const MyPointsPage = memo(lazy(() => import("@/pages/admin/myPoints")));
const RewardDisbursementPage = memo(lazy(() => import("@/pages/admin/rewardDisbursement")));
const TrainingProviderPaymentPage = memo(lazy(() => import("@/pages/admin/trainingProviderPayment")));
const MyTrainingPage = memo(lazy(() => import("@/pages/admin/myTraining")));
const LearningCommunityPage = memo(lazy(() => import("@/pages/admin/learningCommunity")));
const MyExitPage = memo(lazy(() => import("@/pages/admin/myExit")));
const ExitQuestionnairePage = memo(lazy(() => import("@/pages/admin/exitQuestionnaire")));
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
        {/* Modules migrated to URL-backed records (/x · /x/new · /x/{guid}), generated
            from the registry in ./entityRoutes. Any flat routes remaining below are screens
            with no single-record concept; both styles are gated identically. */}
        {renderEntityRoutes(ENTITY_ROUTES)}
        {/* Organizational Structure (HRMS §3.1) */}
        <Route path="reports" element={<ReportViewerPage />} />
        <Route path="auditLog" element={<AuditLogPage />} />
        {/* Compensation & Benefit (HRMS §3.10.1) */}
        <Route path="taxBracket" element={<TaxBracketPage />} />
        {/* A singleton config, so it stays a flat route — there is no record id to put in the URL. */}
        <Route path="salaryIncrementPolicy" element={<SalaryIncrementPolicyPage />} />
        <Route path="employeeCompensation" element={<EmployeeCompensationPage />} />
        <Route path="myCompensation" element={<MyCompensationPage />} />
        <Route path="compensationRequest" element={<CompensationRequestPage />} />
        {/* Medical Benefit (HRMS §3.10.2) */}
        <Route path="medicalEnrollment" element={<MedicalEnrollmentPage />} />
        <Route path="medicalClaim" element={<MedicalClaimPage />} />
        {/* Insurance (HRMS §3.10.3) */}
        <Route path="insuranceClaim" element={<InsuranceClaimPage />} />
        {/* Employee Loan (HRMS §3.10.4) */}
        {/* Guarantee Commitments (HRMS §3.12) */}
        {/* Trip Management (HRMS §3.10.5) */}
        <Route path="trip" element={<TripPage />} />
        <Route path="workflow" element={<WorkflowPage />} />
        <Route path="terminationList" element={<TerminationListPage />} />
        <Route path="establishmentOverview" element={<EstablishmentOverviewPage />} />
        <Route path="jobApplication" element={<JobApplicationPage />} />
        <Route path="talentPool" element={<TalentPoolPage />} />
        <Route path="hireEmployee" element={<HireEmployeePage />} />
        <Route path="offerLetterTemplate" element={<OfferLetterTemplatePage />} />
        {/* Performance Management (HRMS §3.6) — Phase A configuration */}
        <Route path="positionCompetency" element={<PositionCompetencyPage />} />
        <Route path="myPeerReviews" element={<MyPeerReviewsPage />} />
        <Route path="recognitionWall" element={<RecognitionWallPage />} />
        <Route path="myPoints" element={<MyPointsPage />} />
        <Route path="rewardDisbursement" element={<RewardDisbursementPage />} />
        <Route path="trainingProviderPayment" element={<TrainingProviderPaymentPage />} />
        <Route path="myTraining" element={<MyTrainingPage />} />
        <Route path="learningCommunity" element={<LearningCommunityPage />} />
        <Route path="myExit" element={<MyExitPage />} />
        <Route path="exitQuestionnaire" element={<ExitQuestionnairePage />} />
        {/* Employee Engagement (HRMS §3.9.1) */}
        <Route path="suggestion" element={<SuggestionPage />} />
        <Route path="grievance" element={<GrievancePage />} />
        <Route path="announcement" element={<AnnouncementPage />} />
        <Route path="newsFeed" element={<NewsFeedPage />} />
        <Route path="survey" element={<SurveyPage />} />
        <Route path="surveyTake" element={<SurveyTakePage />} />
        <Route path="performanceDashboard" element={<PerformanceDashboardPage />} />
        {/* Unmatched URL inside the shell — previously rendered an empty page. Outside
            the gate so it can't be mistaken for a permission failure. */}
        <Route path="*" element={<NotFoundPage />} />
        </Route>
       </Route>
    </Routes>
  );
}
