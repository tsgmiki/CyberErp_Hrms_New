import { ChevronRight } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cn } from "../utils/classNames";
import type { SidebarModuleEntry } from "../utils/menuTypes";

interface ModuleListItemProps {
  entry: SidebarModuleEntry;
  collapsed: boolean;
  onSelectSubsystem?: (subsystem: string) => void;
}

function ModuleListItem({ entry, collapsed, onSelectSubsystem }: ModuleListItemProps) {
  const { t } = useTranslation();

  const handleClick = () => {
    onSelectSubsystem?.(entry.title);
  };

  if (collapsed) {
    return (
      <button
        type="button"
        onClick={handleClick}
        className="flex items-center justify-center w-full p-2 rounded-lg text-sidebar-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors"
        title={t(entry.title)}
      >
        <span className="[&>svg]:w-4 [&>svg]:h-4">{entry.icon}</span>
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      className={cn(
        "flex items-center gap-2 w-full px-3 py-2 rounded-lg text-xs transition-all duration-200",
        "text-muted-foreground hover:text-foreground hover:bg-sidebar-accent",
      )}
    >
      <div className="flex w-4 items-center justify-center" aria-hidden>
        <ChevronRight className="h-3 w-3 shrink-0" />
      </div>
      <span className="uppercase tracking-wider text-[10px] font-semibold flex-1 text-left">
        {t(entry.title)}
      </span>
    </button>
  );
}

export default ModuleListItem;
