import { AlertCircle, CheckCircle2, Info, X, XCircle } from "lucide-react";
import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import type { ToastRecord, ToastVariant } from "./toastStore";
import { dismissToast, getToasts, subscribe } from "./toastStore";

const VARIANT_STYLES: Record<
  ToastVariant,
  { icon: typeof CheckCircle2; iconWrap: string; border: string }
> = {
  default: {
    icon: Info,
    iconWrap: "bg-secondary text-foreground",
    border: "border-border",
  },
  success: {
    icon: CheckCircle2,
    iconWrap: "bg-success/15 text-success",
    border: "border-success/30",
  },
  error: {
    icon: XCircle,
    iconWrap: "bg-error/15 text-error",
    border: "border-error/30",
  },
  info: {
    icon: Info,
    iconWrap: "bg-info/15 text-info",
    border: "border-info/30",
  },
  warning: {
    icon: AlertCircle,
    iconWrap: "bg-warning/15 text-warning",
    border: "border-warning/30",
  },
};

function ToastItem({ item }: { item: ToastRecord }) {
  const { icon: Icon, iconWrap, border } = VARIANT_STYLES[item.variant];

  return (
    <li
      role="status"
      className={`relative pointer-events-auto flex w-full max-w-[420px] animate-in items-start gap-3 rounded-lg border bg-card p-4 pr-10 shadow-lg duration-200 fade-in slide-in-from-bottom-2 ${border}`}
    >
      <span
        className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-full ${iconWrap}`}
        aria-hidden
      >
        <Icon className="h-4 w-4" strokeWidth={2.25} />
      </span>
      <div className="min-w-0 flex-1 pt-0.5">
        <p className="text-sm font-semibold text-foreground">{item.title}</p>
        {item.description ? (
          <p className="mt-0.5 text-sm text-muted">{item.description}</p>
        ) : null}
      </div>
      <button
        type="button"
        onClick={() => dismissToast(item.id)}
        className="absolute right-2 top-2 rounded-md p-1 text-muted transition-colors hover:bg-secondary hover:text-foreground"
        aria-label="Dismiss"
      >
        <X className="h-4 w-4" />
      </button>
    </li>
  );
}

/** Bottom-right toast stack (cyber-erp-suite placement). */
export function Toaster() {
  const [items, setItems] = useState<ToastRecord[]>(getToasts);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    return subscribe(() => setItems(getToasts()));
  }, []);

  if (!mounted || items.length === 0) return null;

  return createPortal(
    <ol
      aria-live="polite"
      aria-relevant="additions"
      className="pointer-events-none fixed bottom-4 right-4 z-[9999] flex w-[min(100vw-2rem,420px)] flex-col gap-2 p-0 sm:bottom-6 sm:right-6"
    >
      {items.map((item) => (
        <ToastItem key={item.id} item={item} />
      ))}
    </ol>,
    document.body,
  );
}

export default Toaster;
