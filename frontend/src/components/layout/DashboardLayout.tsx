import { Suspense, useState } from "react";
import { Outlet } from "react-router-dom";
import { Header } from "@/components/header";
import Spinner from "@/components/common/spinner/spinner";
import Menu from "@/components/menu";
import { RouteErrorBoundary } from "@/components/common/errorBoundary";

export default function DashboardLayout() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  return (
    // h-screen, NOT min-h-screen: a minimum is not a definite height, so `main`'s flex-1 grew with
    // its content and every `h-full` / `min-h-0 flex-1 overflow-auto` below it (the org tree and
    // EVERY data grid) grew too — the window scrolled instead of the panels. Pinning the shell to the
    // viewport gives the whole app the fixed height those panels were already written against, so
    // each one scrolls inside its own box.
    <div className="flex h-screen w-full bg-background">
      <Menu
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed((value) => !value)}
      />
      <div className="flex flex-1 flex-col min-w-0 min-h-0 relative">
        <Header />
        <main className="relative z-0 min-h-0 flex-1 overflow-auto p-2 md:p-2">
          {/* Page-level boundary: a crashing page keeps the sidebar/header, and navigating
              elsewhere (route change) clears the error automatically. */}
          <RouteErrorBoundary>
            <Suspense fallback={<Spinner block />}>
              <Outlet />
            </Suspense>
          </RouteErrorBoundary>
        </main>
      </div>
    </div>
  );
}
