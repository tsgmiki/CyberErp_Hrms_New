import { createContext, useContext, useEffect, useState, type ReactNode } from "react";

type Theme = "light" | "dark" | "system";
type ResolvedTheme = "light" | "dark";

interface ThemeContextType {
  theme: Theme;
  resolvedTheme: ResolvedTheme;
  toggleTheme: () => void;
  setTheme: (theme: Theme) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);
export { ThemeContext };

const STORAGE_KEY = "theme";

function getResolvedTheme(theme: Theme): ResolvedTheme {
  if (theme === "system") {
    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }
  return theme;
}

function applyThemeColors(resolved: ResolvedTheme) {
  const colors =
    resolved === "dark"
      ? {
          "--color-bg-primary": "#183B4E",
          "--color-bg-secondary": "#1e4a63",
          "--color-text-primary": "#F3F3E0",
          "--color-text-secondary": "#DDA853",
          "--color-accent": "#27548A",
          "--color-highlight": "#DDA853",
          "--color-border": "#27548A",
          "--color-focus": "#DDA853",
        }
      : {
          "--color-bg-primary": "#F3F3E0",
          "--color-bg-secondary": "#ffffff",
          "--color-text-primary": "#183B4E",
          "--color-text-secondary": "#27548A",
          "--color-accent": "#27548A",
          "--color-highlight": "#DDA853",
          "--color-border": "#DDA853",
          "--color-focus": "#27548A",
        };

  Object.entries(colors).forEach(([key, value]) => {
    document.documentElement.style.setProperty(key, value);
  });

  document.documentElement.setAttribute("data-theme", resolved);
}

interface ThemeProviderProps {
  children: ReactNode;
}

export function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setThemeState] = useState<Theme>(() => {
    const savedTheme = localStorage.getItem(STORAGE_KEY) as Theme | null;
    if (savedTheme === "light" || savedTheme === "dark" || savedTheme === "system") {
      return savedTheme;
    }
    if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
      return "system";
    }
    return "light";
  });

  const [resolvedTheme, setResolvedTheme] = useState<ResolvedTheme>(() => getResolvedTheme(theme));

  useEffect(() => {
    const resolved = getResolvedTheme(theme);
    setResolvedTheme(resolved);
    applyThemeColors(resolved);
    localStorage.setItem(STORAGE_KEY, theme);
  }, [theme]);

  useEffect(() => {
    if (theme !== "system") return;

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleChange = () => {
      const resolved = mediaQuery.matches ? "dark" : "light";
      setResolvedTheme(resolved);
      applyThemeColors(resolved);
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => mediaQuery.removeEventListener("change", handleChange);
  }, [theme]);

  const toggleTheme = () => {
    setThemeState((prev) => {
      const resolved = getResolvedTheme(prev);
      return resolved === "light" ? "dark" : "light";
    });
  };

  const setTheme = (newTheme: Theme) => {
    setThemeState(newTheme);
  };

  return (
    <ThemeContext.Provider value={{ theme, resolvedTheme, toggleTheme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }
  return context;
}
