import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { format as formatWithTokens } from "date-fns";
import { toZonedTime } from "date-fns-tz";
import i18n from "@/i18n";
import { useAuth } from "@/context/AuthContext";
import { useTheme } from "@/context/ThemeContext";
import { api } from "@/utils/apiClient";

/**
 * The signed-in user's display preferences, applied in HRMS.
 *
 * ⚠️ ONE ROW, TWO SUBSYSTEMS. Preferences live in the shared `Core.UserPreference` table and are
 * EDITED in the Home portal's Edit Profile dialog. HRMS only reads them (GET UserPreference/mine),
 * so a user who picks a theme or language in Home sees the same choice here. There is deliberately
 * no editor in HRMS — a second editor for one shared row is how validation drifts apart.
 *
 * Contract, matching the Home portal's provider:
 * - **Dynamic read**: loaded once the session is known, keyed to the signed-in user.
 * - **Default fallback**: no row (or an unreachable API) leaves the system defaults in place. An
 *   absent row is the NORMAL state for anyone who has never opened the dialog, not an error.
 * - **Applied**: `theme` and `language`, the two with a single control point.
 *
 * `dateFormat` / `numberFormat` / `timeZone` are exposed as `formatDate` / `formatNumber`. Screens
 * that format dates or numbers themselves are NOT retrofitted — use these helpers, or the
 * preference cannot reach that screen.
 */
export interface UserPreferences {
  language: string;
  timeZone: string;
  dateFormat: string;
  numberFormat: string;
  landingPage: string;
  theme: "light" | "dark" | "system";
  emailNotifications: boolean;
  inAppNotifications: boolean;
  approvalNotifications: boolean;
  /** False when these are the system defaults because the user has no saved row. */
  isUserDefined?: boolean;
}

export const DEFAULT_PREFERENCES: UserPreferences = {
  language: "en",
  timeZone: "Africa/Nairobi",
  dateFormat: "dd/MM/yyyy",
  numberFormat: "1,234.56",
  landingPage: "/",
  theme: "system",
  emailNotifications: true,
  inAppNotifications: true,
  approvalNotifications: true,
  isUserDefined: false,
};

interface PreferencesContextType {
  preferences: UserPreferences;
  /** True once the server has answered — false means the values are still the defaults. */
  loaded: boolean;
  formatDate: (value: Date | string | null | undefined) => string;
  formatNumber: (value: number | null | undefined, decimals?: number) => string;
}

const PreferencesContext = createContext<PreferencesContextType | undefined>(undefined);

/** `numberFormat` is stored as a SAMPLE ("1,234.56") — the separators are derived from it. */
const SEPARATORS: Record<string, { group: string; decimal: string }> = {
  "1,234.56": { group: ",", decimal: "." },
  "1.234,56": { group: ".", decimal: "," },
  "1 234,56": { group: " ", decimal: "," },
};

export function PreferencesProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const { setTheme } = useTheme();
  const [preferences, setPreferences] = useState<UserPreferences>(DEFAULT_PREFERENCES);
  const [loaded, setLoaded] = useState(false);
  /**
   * The account the CURRENT load belongs to — used to drop a late response after the user changed,
   * NOT to skip repeat effect runs. Short-circuiting the effect on an unchanged id deadlocks under
   * StrictMode: pass 1 arms the guard and starts the fetch, cleanup cancels it, pass 2 returns early,
   * and `loaded` is never set.
   */
  const loadingFor = useRef<string | null>(null);

  useEffect(() => {
    if (!user?.id) {
      loadingFor.current = null;
      setPreferences(DEFAULT_PREFERENCES);
      setLoaded(false);
      return;
    }
    loadingFor.current = user.id;

    let cancelled = false;
    void (async () => {
      try {
        // skipAuthRedirect: a preference read must never bounce the user to /login — the session
        // probe owns that decision, and losing a theme is not a reason to end a session.
        const saved = await api.get<UserPreferences>("UserPreference/mine", {
          skipAuthRedirect: true,
        });
        if (cancelled || loadingFor.current !== user.id) return;
        const merged = { ...DEFAULT_PREFERENCES, ...(saved ?? {}) };
        setPreferences(merged);
        setTheme(merged.theme);
        if (merged.language && i18n.language !== merged.language) {
          void i18n.changeLanguage(merged.language);
        }
        // Keep <html lang> in step: screen readers and the browser's translation prompt read it,
        // and it makes the applied language observable rather than internal to i18next.
        if (merged.language) document.documentElement.lang = merged.language;
      } catch {
        // Unreachable API — the defaults already in state ARE the fallback.
        if (!cancelled) setPreferences(DEFAULT_PREFERENCES);
      } finally {
        if (!cancelled) setLoaded(true);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [user?.id, setTheme]);

  const formatDate = useCallback(
    (value: Date | string | null | undefined) => {
      if (value === null || value === undefined || value === "") return "—";
      const date = value instanceof Date ? value : new Date(value);
      if (Number.isNaN(date.getTime())) return "—";
      try {
        const zoned = preferences.timeZone ? toZonedTime(date, preferences.timeZone) : date;
        return formatWithTokens(zoned, preferences.dateFormat || "dd/MM/yyyy");
      } catch {
        // An unknown zone or a bad token must not blank the screen.
        return date.toLocaleDateString();
      }
    },
    [preferences.dateFormat, preferences.timeZone],
  );

  const formatNumber = useCallback(
    (value: number | null | undefined, decimals = 2) => {
      if (value === null || value === undefined || Number.isNaN(value)) return "—";
      const { group, decimal } = SEPARATORS[preferences.numberFormat] ?? SEPARATORS["1,234.56"];
      const [whole, fraction] = Math.abs(value).toFixed(decimals).split(".");
      const grouped = whole.replace(/\B(?=(\d{3})+(?!\d))/g, group);
      const sign = value < 0 ? "-" : "";
      return fraction ? `${sign}${grouped}${decimal}${fraction}` : `${sign}${grouped}`;
    },
    [preferences.numberFormat],
  );

  const value = useMemo(
    () => ({ preferences, loaded, formatDate, formatNumber }),
    [preferences, loaded, formatDate, formatNumber],
  );

  return <PreferencesContext.Provider value={value}>{children}</PreferencesContext.Provider>;
}

export function usePreferences() {
  const context = useContext(PreferencesContext);
  if (!context) throw new Error("usePreferences must be used within a PreferencesProvider");
  return context;
}

export default PreferencesContext;
