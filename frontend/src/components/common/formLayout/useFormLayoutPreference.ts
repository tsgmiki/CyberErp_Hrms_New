import { useCallback, useEffect, useState } from "react";
import { UserData } from "@/store/user";

/** The three record-form layouts an HR admin can choose between. */
export type FormLayout = "cards" | "tabs" | "leftnav";

const STORAGE_PREFIX = "form-layout:";
const VALID: FormLayout[] = ["cards", "tabs", "leftnav"];

/** Per-user, per-form storage key (so each administrator keeps their own preference on a shared machine). */
function keyFor(storageKey: string): string {
  const user = UserData.peek() as { id?: string; userName?: string };
  const uid = user?.id ?? user?.userName ?? "anon";
  return `${STORAGE_PREFIX}${storageKey}:${uid}`;
}

function read(storageKey: string): FormLayout | null {
  try {
    const raw = localStorage.getItem(keyFor(storageKey));
    return raw && VALID.includes(raw as FormLayout) ? (raw as FormLayout) : null;
  } catch {
    return null;
  }
}

/**
 * Persists an HR admin's preferred layout for a given form (keyed per user + `storageKey`).
 * Mirrors the app's other localStorage-backed UI preferences (list columns, theme, sidebar groups).
 */
export function useFormLayoutPreference(
  storageKey: string,
  defaultLayout: FormLayout = "leftnav",
): readonly [FormLayout, (next: FormLayout) => void] {
  const [layout, setLayoutState] = useState<FormLayout>(() => read(storageKey) ?? defaultLayout);

  // Re-hydrate when the form (storageKey) changes.
  useEffect(() => {
    setLayoutState(read(storageKey) ?? defaultLayout);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [storageKey]);

  const setLayout = useCallback(
    (next: FormLayout) => {
      setLayoutState(next);
      try {
        localStorage.setItem(keyFor(storageKey), next);
      } catch {
        /* storage unavailable — keep the in-memory choice */
      }
    },
    [storageKey],
  );

  return [layout, setLayout] as const;
}
