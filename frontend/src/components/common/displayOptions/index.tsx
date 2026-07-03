import { memo } from "react";
import { Grid, List } from "lucide-react";
import { useTranslation } from "react-i18next";

interface DisplayOptionsProps {
  displayMode: "list" | "grid";
  onDisplayModeChange: (mode: "list" | "grid") => void;
}

const DisplayOptions = memo(function DisplayOptions({
  displayMode,
  onDisplayModeChange,
}: DisplayOptionsProps) {
  const { t } = useTranslation();

  return (
    <div
      className={`flex items-center border rounded-lg overflow-hidden transition-colors duration-200 divide-x border-border bg-background`}
    >
      <button
        onClick={() => onDisplayModeChange?.("list")}
        className={`p-2 transition-colors duration-150 ${
          displayMode === "list"
            ? "bg-primary text-on-accent"
            : "text-primary hover:text-foreground hover:bg-primary/20"
        }`}
        title={t("List view")}
      >
        <List size={16} />
      </button>
      <button
        onClick={() => onDisplayModeChange?.("grid")}
        className={`p-2 transition-colors duration-150 ${
          displayMode === "grid"
            ? "bg-primary text-on-accent"
            : "text-primary hover:text-foreground hover:bg-primary/20"
        }`}
        title={t("Grid view")}
      >
        <Grid size={16} />
      </button>
    </div>
  );
});

DisplayOptions.displayName = "DisplayOptions";
export default DisplayOptions;
