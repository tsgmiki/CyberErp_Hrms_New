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
const JobCategoryPage = memo(lazy(() => import("@/pages/admin/jobCategory")));
const WorkLocationPage = memo(lazy(() => import("@/pages/admin/workLocation")));
const BranchPage = memo(lazy(() => import("@/pages/admin/branch")));
const AuditLogPage = memo(lazy(() => import("@/pages/admin/auditLog")));
const EmployeePage = memo(lazy(() => import("@/pages/admin/employee")));
const EmployeeFieldPage = memo(lazy(() => import("@/pages/admin/employeeField")));
const DocumentTemplatePage = memo(lazy(() => import("@/pages/admin/documentTemplate")));
const WorkflowPage = memo(lazy(() => import("@/pages/admin/workflow")));
const WorkflowDefinitionPage = memo(lazy(() => import("@/pages/admin/workflowDefinition")));
const TerminationListPage = memo(lazy(() => import("@/pages/admin/terminationList")));
const ClearanceDepartmentPage = memo(lazy(() => import("@/pages/admin/clearanceDepartment")));
export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/logout" element={<LoginOutPage />} />
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
        <Route path="annualLeaveLedger" element={<AnnualLeaveLedgerPage />} />
        <Route path="jobCategory" element={<JobCategoryPage />} />
        <Route path="workLocation" element={<WorkLocationPage />} />
        <Route path="branch" element={<BranchPage />} />
        <Route path="auditLog" element={<AuditLogPage />} />
        <Route path="employee" element={<EmployeePage />} />
        <Route path="employeeField" element={<EmployeeFieldPage />} />
        <Route path="documentTemplate" element={<DocumentTemplatePage />} />
        <Route path="workflow" element={<WorkflowPage />} />
        <Route path="workflowDefinition" element={<WorkflowDefinitionPage />} />
        <Route path="terminationList" element={<TerminationListPage />} />
        <Route path="clearanceDepartment" element={<ClearanceDepartmentPage />} />
       </Route>
    </Routes>
  );
}
