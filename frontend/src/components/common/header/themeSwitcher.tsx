import { Monitor, Moon, Sun } from "lucide-react";
import { useTheme } from "@/context/ThemeContext";
import {
  HeaderDropdown,
  HeaderDropdownContent,
  HeaderDropdownItem,
  HeaderDropdownTrigger,
} from "./dropdown";

function ThemeSwitcher() {
  const { theme, setTheme, resolvedTheme } = useTheme();

  return (
    <HeaderDropdown>
      <HeaderDropdownTrigger>
        <button
          type="button"
          className="w-8 h-8 flex items-center justify-center rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors"
          aria-label="Change theme"
        >
          {resolvedTheme === "dark" ? <Moon className="w-4 h-4" /> : <Sun className="w-4 h-4" />}
        </button>
      </HeaderDropdownTrigger>
      <HeaderDropdownContent align="end" className="w-32">
        <HeaderDropdownItem
          onClick={() => setTheme("light")}
          className={`gap-2 text-xs ${theme === "light" ? "bg-accent" : ""}`}
        >
          <Sun className="w-3.5 h-3.5" /> Light
        </HeaderDropdownItem>
        <HeaderDropdownItem
          onClick={() => setTheme("dark")}
          className={`gap-2 text-xs ${theme === "dark" ? "bg-accent" : ""}`}
        >
          <Moon className="w-3.5 h-3.5" /> Dark
        </HeaderDropdownItem>
        <HeaderDropdownItem
          onClick={() => setTheme("system")}
          className={`gap-2 text-xs ${theme === "system" ? "bg-accent" : ""}`}
        >
          <Monitor className="w-3.5 h-3.5" /> System
        </HeaderDropdownItem>
      </HeaderDropdownContent>
    </HeaderDropdown>
  );
}

export default ThemeSwitcher;
