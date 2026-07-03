import { cn } from "../utils/classNames";
import { useMenuModules } from "../hooks/useMenuModules";
import { useExpandedCategories } from "../hooks/useExpandedCategories";
import SidebarHeader from "./sidebarHeader";
import SidebarFooter from "./sidebarFooter";
import BackToModules from "./backToModules";
import SubsystemTitle from "./subsystemTitle";
import SidebarNav from "./sidebarNav";

interface SidebarProps {
  collapsed: boolean;
  onToggle: () => void;
}

function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const { navigation } = useMenuModules();
  const { isCategoryExpanded, toggleCategory } = useExpandedCategories(navigation);

  return (
    <aside
      className={cn(
        "hidden md:flex h-screen flex-col border-r border-sidebar-border bg-sidebar transition-all duration-300 ease-in-out sticky top-0 select-none shrink-0",
        collapsed ? "w-[60px]" : "w-[260px]",
      )}
    >
      <SidebarHeader collapsed={collapsed} onToggle={onToggle} />
      <BackToModules collapsed={collapsed} show={navigation.isSubsystemSelected} />
      <SubsystemTitle
        collapsed={collapsed}
        title={navigation.selectedSubsystem}
        icon={navigation.selectedSubsystemIcon}
      />
      <SidebarNav
        collapsed={collapsed}
        navigation={navigation}
        isCategoryExpanded={isCategoryExpanded}
        toggleCategory={toggleCategory}
      />
      <SidebarFooter collapsed={collapsed} />
    </aside>
  );
}

export default Sidebar;
