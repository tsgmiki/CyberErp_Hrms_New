import type { ReactNode } from "react";

/**
 * Imperative confirm dialog — a drop-in replacement for the native `window.confirm`
 * that renders through our standard {@link DialogModal} instead of the browser popup.
 * Usage: `if (!(await confirm({ message: "Delete this?" }))) return;`
 * A single <ConfirmHost/> (mounted once in App) subscribes to this store and shows the modal.
 */
export interface ConfirmOptions {
  /** Body text / node shown in the dialog. */
  message: ReactNode;
  /** Header title (defaults to "Confirm" in the host). */
  title?: string;
  /** Primary button label (defaults to "OK"). */
  confirmLabel?: string;
  /** Cancel button label (defaults to "Cancel"). */
  cancelLabel?: string;
  /** Visual tone of the primary button — destructive (red) for delete/cancel/reject. */
  variant?: "default" | "destructive";
}

interface ConfirmState extends ConfirmOptions {
  id: string;
  resolve: (ok: boolean) => void;
}

let current: ConfirmState | null = null;
const listeners = new Set<() => void>();

function emit() {
  listeners.forEach((listener) => listener());
}

export function getConfirm(): ConfirmState | null {
  return current;
}

export function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

/** Opens the confirm dialog and resolves true (confirmed) or false (cancelled/dismissed). */
export function confirm(options: ConfirmOptions): Promise<boolean> {
  return new Promise<boolean>((resolve) => {
    // A second confirm supersedes any still-open one (resolve the old as cancelled).
    if (current) current.resolve(false);
    const id =
      typeof crypto !== "undefined" && crypto.randomUUID
        ? crypto.randomUUID()
        : `${Date.now()}-${Math.random()}`;
    current = { ...options, id, resolve };
    emit();
  });
}

/** Settles the active confirm and closes it. No-op if none is open. */
export function resolveConfirm(ok: boolean) {
  if (!current) return;
  current.resolve(ok);
  current = null;
  emit();
}
