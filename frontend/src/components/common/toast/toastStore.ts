export type ToastVariant = "default" | "success" | "error" | "info" | "warning";

export interface ToastRecord {
  id: string;
  title: string;
  description?: string;
  variant: ToastVariant;
}

type ToastOptions = {
  description?: string;
  duration?: number;
};

const DEFAULT_DURATION = 4000;
const TOAST_LIMIT = 3;

let toasts: ToastRecord[] = [];
const listeners = new Set<() => void>();
const dismissTimers = new Map<string, ReturnType<typeof setTimeout>>();

function emit() {
  listeners.forEach((listener) => listener());
}

export function getToasts(): ToastRecord[] {
  return toasts;
}

export function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}

export function dismissToast(id: string) {
  const timer = dismissTimers.get(id);
  if (timer) {
    clearTimeout(timer);
    dismissTimers.delete(id);
  }
  toasts = toasts.filter((t) => t.id !== id);
  emit();
}

function show(variant: ToastVariant, title: string, options?: ToastOptions) {
  const id =
    typeof crypto !== "undefined" && crypto.randomUUID
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random()}`;

  toasts = [{ id, title, description: options?.description, variant }, ...toasts].slice(
    0,
    TOAST_LIMIT,
  );
  emit();

  const duration = options?.duration ?? DEFAULT_DURATION;
  const timer = setTimeout(() => dismissToast(id), duration);
  dismissTimers.set(id, timer);
}

export const toast = {
  message: (title: string, options?: ToastOptions) => show("default", title, options),
  success: (title: string, options?: ToastOptions) => show("success", title, options),
  error: (title: string, options?: ToastOptions) => show("error", title, options),
  info: (title: string, options?: ToastOptions) => show("info", title, options),
  warning: (title: string, options?: ToastOptions) => show("warning", title, options),
  dismiss: dismissToast,
};
