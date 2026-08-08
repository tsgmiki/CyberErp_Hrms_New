import { lazy, memo, type ComponentType } from "react";
import { Route } from "react-router-dom";
import EntityRecordGuard from "@/components/common/entityRecordGuard";
import { normalizeRoutePath } from "@/utils/routeMatch";

const EmployeePage = memo(lazy(() => import("@/pages/admin/employee")));
const OrganizationUnitPage = memo(lazy(() => import("@/pages/admin/organizationUnit")));
const PositionPage = memo(lazy(() => import("@/pages/admin/position")));
const BranchPage = memo(lazy(() => import("@/pages/admin/branch")));
const JobCategoryPage = memo(lazy(() => import("@/pages/admin/jobCategory")));
const ModulePage = memo(lazy(() => import("@/pages/admin/module")));
const OperationPage = memo(lazy(() => import("@/pages/admin/operation")));
const SubsystemPage = memo(lazy(() => import("@/pages/admin/subsystem")));
const RolePage = memo(lazy(() => import("@/pages/admin/role")));
const UserPage = memo(lazy(() => import("@/pages/admin/user")));
const UserRolePage = memo(lazy(() => import("@/pages/admin/userRole")));
const PositionClassPage = memo(lazy(() => import("@/pages/admin/positionClass")));
const JobGradePage = memo(lazy(() => import("@/pages/admin/jobGrade")));
const SalaryScalePage = memo(lazy(() => import("@/pages/admin/salaryScale")));
const LeaveTypePage = memo(lazy(() => import("@/pages/admin/leaveType")));
const HolidayPage = memo(lazy(() => import("@/pages/admin/holiday")));
const LeaveRequestPage = memo(lazy(() => import("@/pages/admin/leaveRequest")));
const LeaveBalancePage = memo(lazy(() => import("@/pages/admin/leaveBalance")));
const FiscalYearPage = memo(lazy(() => import("@/pages/admin/fiscalYear")));
const AnnualLeaveSettingPage = memo(lazy(() => import("@/pages/admin/annualLeaveSetting")));
const WorkWeekConfigurationPage = memo(lazy(() => import("@/pages/admin/workWeekConfiguration")));
const AnnualLeavePage = memo(lazy(() => import("@/pages/admin/annualLeave")));
const OtherLeavePage = memo(lazy(() => import("@/pages/admin/otherLeave")));
const OtherLeaveSettingPage = memo(lazy(() => import("@/pages/admin/otherLeaveSetting")));
const ReportDefinitionPage = memo(lazy(() => import("@/pages/admin/reportDefinition")));
const AnnualLeaveLedgerPage = memo(lazy(() => import("@/pages/admin/annualLeaveLedger")));
const WorkLocationPage = memo(lazy(() => import("@/pages/admin/workLocation")));
const TransferRequestPage = memo(lazy(() => import("@/pages/admin/transferRequest")));
const DisciplinaryCasePage = memo(lazy(() => import("@/pages/admin/disciplinaryCase")));
const AllowanceTypePage = memo(lazy(() => import("@/pages/admin/allowanceType")));
const BenefitPlanPage = memo(lazy(() => import("@/pages/admin/benefitPlan")));
const SalaryRevisionPage = memo(lazy(() => import("@/pages/admin/salaryRevision")));
const MedicalProviderPage = memo(lazy(() => import("@/pages/admin/medicalProvider")));
const MedicalPlanPage = memo(lazy(() => import("@/pages/admin/medicalPlan")));
const MedicalContractPage = memo(lazy(() => import("@/pages/admin/medicalContract")));
const MyMedicalClaimsPage = memo(lazy(() => import("@/pages/admin/myMedicalClaims")));
const InsurancePolicyPage = memo(lazy(() => import("@/pages/admin/insurancePolicy")));
const MyInsuranceClaimsPage = memo(lazy(() => import("@/pages/admin/myInsuranceClaims")));
const LoanTypePage = memo(lazy(() => import("@/pages/admin/loanType")));
const LoanPage = memo(lazy(() => import("@/pages/admin/loan")));
const MyLoansPage = memo(lazy(() => import("@/pages/admin/myLoans")));
const EmployeeGuaranteePage = memo(lazy(() => import("@/pages/admin/employeeGuarantee")));
const MyGuaranteesPage = memo(lazy(() => import("@/pages/admin/myGuarantees")));
const PerDiemRatePage = memo(lazy(() => import("@/pages/admin/perDiemRate")));
const TripBudgetPage = memo(lazy(() => import("@/pages/admin/tripBudget")));
const MyTripsPage = memo(lazy(() => import("@/pages/admin/myTrips")));
const EmployeeFieldPage = memo(lazy(() => import("@/pages/admin/employeeField")));
const FormBuilderPage = memo(lazy(() => import("@/pages/admin/formBuilder")));
const DocumentTemplatePage = memo(lazy(() => import("@/pages/admin/documentTemplate")));
const WorkflowDefinitionPage = memo(lazy(() => import("@/pages/admin/workflowDefinition")));
const ClearanceDepartmentPage = memo(lazy(() => import("@/pages/admin/clearanceDepartment")));
const WorkforcePlanPage = memo(lazy(() => import("@/pages/admin/workforcePlan")));
const HiringRequestPage = memo(lazy(() => import("@/pages/admin/hiringRequest")));
const JobRequisitionPage = memo(lazy(() => import("@/pages/admin/jobRequisition")));
const CandidatePage = memo(lazy(() => import("@/pages/admin/candidate")));
const RatingScalePage = memo(lazy(() => import("@/pages/admin/ratingScale")));
const CompetencyCategoryPage = memo(lazy(() => import("@/pages/admin/competencyCategory")));
const CriticalPositionPage = memo(lazy(() => import("@/pages/admin/criticalPosition")));
const TalentReviewPage = memo(lazy(() => import("@/pages/admin/talentReview")));
const SuccessionPlanPage = memo(lazy(() => import("@/pages/admin/successionPlan")));
const CareerPathPage = memo(lazy(() => import("@/pages/admin/careerPath")));
const EmployeeCareerPathPage = memo(lazy(() => import("@/pages/admin/employeeCareerPath")));
const MentorshipPage = memo(lazy(() => import("@/pages/admin/mentorship")));
const CareerPathChangeRequestPage = memo(lazy(() => import("@/pages/admin/careerPathChangeRequest")));
const CompetencyPage = memo(lazy(() => import("@/pages/admin/competency")));
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
const AwardCategoryPage = memo(lazy(() => import("@/pages/admin/awardCategory")));
const RecognitionProgramPage = memo(lazy(() => import("@/pages/admin/recognitionProgram")));
const RewardNominationPage = memo(lazy(() => import("@/pages/admin/rewardNomination")));
const TrainingCategoryPage = memo(lazy(() => import("@/pages/admin/trainingCategory")));
const TrainingCoursePage = memo(lazy(() => import("@/pages/admin/trainingCourse")));
const TrainingNeedPage = memo(lazy(() => import("@/pages/admin/trainingNeed")));
const TrainingSessionPage = memo(lazy(() => import("@/pages/admin/trainingSession")));
const TrainingBudgetPage = memo(lazy(() => import("@/pages/admin/trainingBudget")));
const LearningPathPage = memo(lazy(() => import("@/pages/admin/learningPath")));
const TrainingCertificatePage = memo(lazy(() => import("@/pages/admin/trainingCertificate")));
const CompanyAssetPage = memo(lazy(() => import("@/pages/admin/companyAsset")));
const AppraisalAppealPage = memo(lazy(() => import("@/pages/admin/appraisalAppeal")));

export interface EntityRouteDef {
  /** Route root — must equal the module's `coreOperation.Link` so the DB-driven menu still resolves. */
  path: string;
  Page: ComponentType;
}

/**
 * Modules migrated to URL-backed records (`useEntityRouteModule`). Everything NOT listed here keeps
 * its flat `<Route path="x">` in `routes/index.tsx` and its `useEntityCrudModule` state — the two
 * styles coexist in the same `PermissionGate` block.
 *
 * To migrate a module: swap its `index.tsx` to `useEntityRouteModule("/x")`, add a line here, and
 * delete its flat route from `routes/index.tsx`.
 */
export const ENTITY_ROUTES: EntityRouteDef[] = [
  // Employees + Organization Structure — form is a profile page (employee) or a modal
  // over the org tree (organizationUnit, position); the tree selection stays local state.
  { path: "employee", Page: EmployeePage },
  { path: "organizationUnit", Page: OrganizationUnitPage },
  { path: "position", Page: PositionPage },
  // Organizational Structure (HRMS §3.1)
  { path: "branch", Page: BranchPage },
  { path: "jobCategory", Page: JobCategoryPage },
  { path: "module", Page: ModulePage },
  { path: "operation", Page: OperationPage },
  { path: "subsystem", Page: SubsystemPage },
  { path: "role", Page: RolePage },
  { path: "user", Page: UserPage },
  { path: "userRole", Page: UserRolePage },
  // Organizational Structure (HRMS §3.1)
  { path: "positionClass", Page: PositionClassPage },
  { path: "jobGrade", Page: JobGradePage },
  { path: "salaryScale", Page: SalaryScalePage },
  { path: "leaveType", Page: LeaveTypePage },
  { path: "holiday", Page: HolidayPage },
  { path: "leaveRequest", Page: LeaveRequestPage },
  { path: "leaveBalance", Page: LeaveBalancePage },
  { path: "fiscalYear", Page: FiscalYearPage },
  { path: "annualLeaveSetting", Page: AnnualLeaveSettingPage },
  { path: "workWeekConfiguration", Page: WorkWeekConfigurationPage },
  { path: "annualLeave", Page: AnnualLeavePage },
  { path: "otherLeave", Page: OtherLeavePage },
  { path: "otherLeaveSetting", Page: OtherLeaveSettingPage },
  { path: "reportDefinition", Page: ReportDefinitionPage },
  { path: "annualLeaveLedger", Page: AnnualLeaveLedgerPage },
  { path: "workLocation", Page: WorkLocationPage },
  { path: "transferRequest", Page: TransferRequestPage },
  { path: "disciplinaryCase", Page: DisciplinaryCasePage },
  // Compensation & Benefit (HRMS §3.10.1)
  { path: "allowanceType", Page: AllowanceTypePage },
  { path: "benefitPlan", Page: BenefitPlanPage },
  { path: "salaryRevision", Page: SalaryRevisionPage },
  // Medical Benefit (HRMS §3.10.2)
  { path: "medicalProvider", Page: MedicalProviderPage },
  { path: "medicalPlan", Page: MedicalPlanPage },
  { path: "medicalContract", Page: MedicalContractPage },
  { path: "myMedicalClaims", Page: MyMedicalClaimsPage },
  // Insurance (HRMS §3.10.3)
  { path: "insurancePolicy", Page: InsurancePolicyPage },
  { path: "myInsuranceClaims", Page: MyInsuranceClaimsPage },
  // Employee Loan (HRMS §3.10.4)
  { path: "loanType", Page: LoanTypePage },
  { path: "loan", Page: LoanPage },
  { path: "myLoans", Page: MyLoansPage },
  // Guarantee Commitments (HRMS §3.12)
  { path: "employeeGuarantee", Page: EmployeeGuaranteePage },
  { path: "myGuarantees", Page: MyGuaranteesPage },
  // Trip Management (HRMS §3.10.5)
  { path: "perDiemRate", Page: PerDiemRatePage },
  { path: "tripBudget", Page: TripBudgetPage },
  { path: "myTrips", Page: MyTripsPage },
  { path: "employeeField", Page: EmployeeFieldPage },
  { path: "formBuilder", Page: FormBuilderPage },
  { path: "documentTemplate", Page: DocumentTemplatePage },
  { path: "workflowDefinition", Page: WorkflowDefinitionPage },
  { path: "clearanceDepartment", Page: ClearanceDepartmentPage },
  { path: "workforcePlan", Page: WorkforcePlanPage },
  { path: "hiringRequest", Page: HiringRequestPage },
  { path: "jobRequisition", Page: JobRequisitionPage },
  { path: "candidate", Page: CandidatePage },
  // Performance Management (HRMS §3.6) — Phase A configuration
  { path: "ratingScale", Page: RatingScalePage },
  { path: "competencyCategory", Page: CompetencyCategoryPage },
  { path: "criticalPosition", Page: CriticalPositionPage },
  { path: "talentReview", Page: TalentReviewPage },
  { path: "successionPlan", Page: SuccessionPlanPage },
  { path: "careerPath", Page: CareerPathPage },
  { path: "employeeCareerPath", Page: EmployeeCareerPathPage },
  { path: "mentorship", Page: MentorshipPage },
  { path: "careerPathChangeRequest", Page: CareerPathChangeRequestPage },
  { path: "competency", Page: CompetencyPage },
  { path: "reviewCycle", Page: ReviewCyclePage },
  { path: "appraisalTemplate", Page: AppraisalTemplatePage },
  { path: "organizationalObjective", Page: OrganizationalObjectivePage },
  { path: "employeeGoal", Page: EmployeeGoalPage },
  { path: "appraisal", Page: AppraisalPage },
  { path: "calibration", Page: CalibrationPage },
  { path: "developmentPlan", Page: DevelopmentPlanPage },
  { path: "improvementPlan", Page: ImprovementPlanPage },
  { path: "achievement", Page: AchievementPage },
  { path: "recognitionBadge", Page: RecognitionBadgePage },
  { path: "recognition", Page: RecognitionPage },
  { path: "awardCategory", Page: AwardCategoryPage },
  { path: "recognitionProgram", Page: RecognitionProgramPage },
  { path: "rewardNomination", Page: RewardNominationPage },
  { path: "trainingCategory", Page: TrainingCategoryPage },
  { path: "trainingCourse", Page: TrainingCoursePage },
  { path: "trainingNeed", Page: TrainingNeedPage },
  { path: "trainingSession", Page: TrainingSessionPage },
  { path: "trainingBudget", Page: TrainingBudgetPage },
  { path: "learningPath", Page: LearningPathPage },
  { path: "trainingCertificate", Page: TrainingCertificatePage },
  { path: "companyAsset", Page: CompanyAssetPage },
  { path: "appraisalAppeal", Page: AppraisalAppealPage },
];

/**
 * Expands each registry entry into the standard triple:
 *
 *   /x         index   → list (keeps the existing menu link working unchanged)
 *   /x/:id     dynamic → form. The literal "new" rides the SAME `:id` slot rather than getting its
 *                        own static route, which is what makes `useParams().id` a total
 *                        description of the screen: undefined = list, "new" = create, guid = edit.
 *                        A static `path="new"` sibling would leave `params.id` undefined on
 *                        /x/new, indistinguishable from the list. The guard admits "new".
 *
 * Both render the SAME component. React-router keys a rendered route by position rather than by
 * path, so moving between siblings at the same depth does not remount the module — no Suspense
 * flash, and the list's query cache survives the transition.
 */
export const renderEntityRoutes = (defs: readonly EntityRouteDef[]) =>
  defs.map(({ path, Page }) => (
    // The guard sits on the SHARED PARENT, not on an extra wrapper around `:id` only. Both children
    // must render at the same tree depth, otherwise React sees a different element shape when moving
    // list -> form and REMOUNTS the module, wiping any local state it holds alongside the record —
    // the salary-scale grade filter, the org-tree selection, the position "add under this unit"
    // preset. Same depth + same element type = state survives the transition.
    <Route key={path} path={path} element={<EntityRecordGuard />}>
      <Route index element={<Page />} />
      <Route path=":id" element={<Page />} />
    </Route>
  ));

/**
 * Builds the deep link for a record when its module supports one, else the plain list route.
 *
 * This is what makes "open the thing you picked" work from anywhere — global search, notifications,
 * a dashboard tile — without each caller (or each backend search provider) hard-coding URL shapes.
 * Callers hand over the module's base route plus a record id; a module still on a flat route simply
 * gets its list back, so adding it to ENTITY_ROUTES upgrades every caller at once.
 */
/** normalized path → the registry's canonical casing, e.g. "organizationunit" → "organizationUnit". */
const RECORD_ROUTES = new Map(ENTITY_ROUTES.map((e) => [normalizeRoutePath(e.path), e.path]));
const GUID = /^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$/;

export function buildRecordRoute(route: string, id?: string | null): string {
  if (!id || !GUID.test(id)) return route;
  // Only a module's OWN root takes a record id; "/employee/x/y" or a non-entity page is left alone.
  const canonical = RECORD_ROUTES.get(normalizeRoutePath(route));
  return canonical ? `/${canonical}/${id}` : route;
}
