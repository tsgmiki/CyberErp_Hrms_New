import { Suspense, useState } from "react";
import { Outlet } from "react-router-dom";
import { Header } from "@/components/header";
import Spinner from "@/components/common/spinner/spinner";
import Menu from "@/components/menu";

export default function DashboardLayout() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  return (
    <div className="flex min-h-screen w-full bg-background">
      <Menu
        collapsed={sidebarCollapsed}
        onToggle={() => setSidebarCollapsed((value) => !value)}
      />
      <div className="flex flex-1 flex-col min-w-0 min-h-screen relative">
        <Header />
        <main className="relative z-0 flex-1 overflow-auto p-2 md:p-2">
          <Suspense fallback={<Spinner block />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  );
}
