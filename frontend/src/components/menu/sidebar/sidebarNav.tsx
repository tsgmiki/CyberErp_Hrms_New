import { LayoutDashboard } from "lucide-react";
import { useTranslation } from "react-i18next";
import store from "@/store";
import type { SidebarNavigationModel } from "../utils/menuTypes";
import MenuNavLink from "../navLink";
import CategoryGroup from "./categoryGroup";
import ModuleListItem from "./moduleListItem";
import NavItem from "./navItem";

interface SidebarNavProps {
  collapsed: boolean;
  navigation: SidebarNavigationModel;
  isCategoryExpanded: (key: string) => boolean;
  toggleCategory: (key: string) => void;
}

function SidebarNav({
  collapsed,
  navigation,
  isCategoryExpanded,
  toggleCategory,
}: SidebarNavProps) {
  const { t } = useTranslation();
  const { isSubsystemSelected, entries } = navigation;

  const collapsedItems = entries.flatMap((entry) => entry.flatItems);

  const handleSelectSubsystem = (subsystem: string) => {
    store.ModuleData.value = { name: subsystem };
  };

  return (
    <nav className="flex-1 overflow-y-auto sidebar-scroll px-3 py-2 space-y-0.5">
      {!collapsed && (
        <MenuNavLink
          to="/"
          end
          className="flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm text-sidebar-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors"
          activeClassName="bg-sidebar-operation-active font-semibold text-primary"
        >
          <LayoutDashboard className="w-4 h-4 shrink-0" />
          <span>{t("Dashboard")}</span>
        </MenuNavLink>
      )}

      {collapsed && isSubsystemSelected && (
        <div className="space-y-0.5">
          {collapsedItems.map((item) => (
            <NavItem key={item.id} item={item} collapsed />
          ))}
        </div>
      )}

      {entries.map((entry) => {
        const showCategories = isSubsystemSelected;

        return (
          <div key={entry.id}>
            {!showCategories && (
              <ModuleListItem
                entry={entry}
                collapsed={collapsed}
                onSelectSubsystem={!isSubsystemSelected ? handleSelectSubsystem : undefined}
              />
            )}

            {!collapsed && showCategories && (
              <div className="space-y-0.5">
                {entry.categories.map((category) => (
                  <CategoryGroup
                    key={category.key}
                    category={category}
                    expanded={isCategoryExpanded(category.key)}
                    onToggle={() => toggleCategory(category.key)}
                  />
                ))}
              </div>
            )}
          </div>
        );
      })}
    </nav>
  );
}

export default SidebarNav;
