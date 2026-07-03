import { ArrowLeft, LayoutGrid } from "lucide-react";
import { useNavigate } from "react-router-dom";
import store from "@/store";

interface BackToModulesProps {
  collapsed: boolean;
  show: boolean;
}

function BackToModules({ collapsed, show }: BackToModulesProps) {
  const navigate = useNavigate();

  if (!show) return null;

  const handleClick = () => {
    store.ModuleData.value = { name: "" };
    navigate("/landing");
  };

  if (collapsed) {
    return (
      <div className="px-2 pt-3 pb-1">
        <button
          type="button"
          onClick={handleClick}
          className="flex items-center justify-center w-full p-2 rounded-lg text-muted-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors focus-ring"
          title="All Modules"
        >
          <LayoutGrid className="w-4 h-4" />
        </button>
      </div>
    );
  }

  return (
    <div className="px-3 pt-3 pb-1">
      <button
        type="button"
        onClick={handleClick}
        className="flex items-center gap-2 w-full px-3 py-2 rounded-lg text-xs font-medium text-muted-foreground hover:text-foreground hover:bg-sidebar-accent transition-colors focus-ring"
      >
        <ArrowLeft className="w-3.5 h-3.5 shrink-0" />
        <span>All Modules</span>
      </button>
    </div>
  );
}

export default BackToModules;
