import { cn } from "../utils/classNames";
import SidebarHeader from "./sidebarHeader";
import SidebarFooter from "./sidebarFooter";
import SidebarNav from "./sidebarNav";
import BackToModules from "./backToModules";

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

function Sidebar({ collapsed, onToggle }: SidebarProps) {
  return (
    <aside
      className={cn(
        "hidden md:flex h-screen flex-col border-r border-sidebar-border bg-sidebar transition-all duration-300 ease-in-out sticky top-0 select-none shrink-0",
        collapsed ? "w-[60px]" : "w-[260px]",
      )}
    >
      <SidebarHeader collapsed={collapsed} onToggle={onToggle} />
      {/* Subsystem switcher — back to the /landing picker (deep-links to Home, Finance, …). */}
      <BackToModules collapsed={collapsed} show />
      <SidebarNav collapsed={collapsed} />
      <SidebarFooter collapsed={collapsed} />
    </aside>
  );
}

export default Sidebar;
