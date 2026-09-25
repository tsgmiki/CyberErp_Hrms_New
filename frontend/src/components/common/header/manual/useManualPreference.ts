import { useCallback, useEffect, useState } from "react";
import { UserData } from "@/store/user";

/** How the system manual opens when the header's Manual button is pressed. */
export type ManualOpenMode = "embedded" | "tab";

const STORAGE_PREFIX = "manual-open:";
const VALID: ManualOpenMode[] = ["embedded", "tab"];

/** Embedded by default — it keeps the reader inside the app, and a new tab stays one click away. */
export const DEFAULT_MANUAL_MODE: ManualOpenMode = "embedded";

/**
 * Per-user key, so two people sharing a workstation keep their own choice. Same shape
 * `useFormLayoutPreference` uses for record-form layouts and `useListColumnSelection` for columns.
 */
function storageKey(): string {
  const user = UserData.peek() as { id?: string; userName?: string };
  return `${STORAGE_PREFIX}${user?.id ?? user?.userName ?? "anon"}`;
}

function read(): ManualOpenMode | null {
  try {
    const raw = localStorage.getItem(storageKey());
    return raw && VALID.includes(raw as ManualOpenMode) ? (raw as ManualOpenMode) : null;
  } catch {
    // Private browsing, or storage disabled by policy. A preference is not worth an exception —
    // fall back to the default and carry on.
    return null;
  }
}

/**
 * The signed-in user's manual-open preference, persisted in localStorage.
 *
 * <p>Read through `UserData.peek()` rather than `.value` so the hook never subscribes to the user
 * signal — this sits in the header, which must not re-render on every unrelated user-store touch.
 * The mount effect re-reads once, because the key depends on WHO is signed in: the header renders
 * inside the protected layout, so the user is normally present already, but a re-read costs
 * nothing and stops a first-paint "anon" value from sticking to a real account.</p>
 */
export function useManualPreference(): readonly [ManualOpenMode, (next: ManualOpenMode) => void] {
  const [mode, setModeState] = useState<ManualOpenMode>(() => read() ?? DEFAULT_MANUAL_MODE);

  useEffect(() => {
    setModeState(read() ?? DEFAULT_MANUAL_MODE);
  }, []);

  const setMode = useCallback((next: ManualOpenMode) => {
    setModeState(next);
    try {
      localStorage.setItem(storageKey(), next);
    } catch {
      /* storage unavailable — keep the in-memory choice for this session */
    }
  }, []);

  return [mode, setMode] as const;
}
