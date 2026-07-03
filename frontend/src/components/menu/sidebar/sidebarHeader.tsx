import { Building2 } from "lucide-react";
import BrandTitle from "@/components/common/brand/brandTitle";

interface SidebarHeaderProps {
  collapsed: boolean;
  onToggle: () => void;
}

function SidebarHeader({ collapsed, onToggle }: SidebarHeaderProps) {
  return (
    <div className="h-14 flex items-center px-3 border-b border-sidebar-border shrink-0">
      <button
        type="button"
        onClick={onToggle}
        className="flex items-center gap-2.5 hover:opacity-80 transition-opacity focus-ring rounded-lg px-1 py-1"
        aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center shrink-0 shadow-sm">
          <Building2 className="w-4 h-4 text-primary-foreground" />
        </div>
        {!collapsed && <BrandTitle size="sm" />}
      </button>
    </div>
  );
}

export default SidebarHeader;
