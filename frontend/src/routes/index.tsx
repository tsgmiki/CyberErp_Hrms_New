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
       </Route>
    </Routes>
  );
}
