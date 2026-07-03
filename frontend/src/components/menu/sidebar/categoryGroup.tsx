import { ChevronRight } from "lucide-react";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { cn } from "../utils/classNames";
import type { SidebarCategory } from "../utils/menuTypes";
import NavItem from "./navItem";

interface CategoryGroupProps {
  category: SidebarCategory;
  expanded: boolean;
  onToggle: () => void;
}

function CategoryGroup({ category, expanded, onToggle }: CategoryGroupProps) {
  const location = useLocation();
  const { t } = useTranslation();
  const hasActive = category.items.some((item) => location.pathname === item.path);

  return (
    <div>
      <button
        type="button"
        onClick={onToggle}
        className={cn(
          "flex items-center gap-2 w-full px-3 py-2 rounded-lg text-xs transition-all duration-200",
          hasActive
            ? "bg-sidebar-module-active font-semibold text-foreground"
            : "text-muted-foreground hover:text-foreground hover:bg-sidebar-accent",
        )}
      >
        <div className="flex w-4 items-center justify-center">
          <ChevronRight
            className={cn(
              "h-3 w-3 shrink-0 transition-transform duration-200",
              hasActive ? "text-primary" : "text-muted-foreground",
              expanded && "rotate-90",
            )}
          />
        </div>
        <span className="uppercase tracking-wider text-[10px] font-semibold">
          {t(category.category)}
        </span>
      </button>

      <div
        className={cn(
          "overflow-hidden transition-all duration-200",
          expanded ? "max-h-[500px] opacity-100" : "max-h-0 opacity-0",
        )}
      >
        <div className="ml-4 pl-3 border-l border-sidebar-border/60 space-y-0.5 py-0.5">
          {category.items.map((item) => (
            <NavItem key={item.id} item={item} />
          ))}
        </div>
      </div>
    </div>
  );
}

export default CategoryGroup;
