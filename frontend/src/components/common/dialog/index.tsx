import { X } from "lucide-react";
import type { ReactNode } from "react";
import { useEffect } from "react";
import { createPortal } from "react-dom";
import { useTranslation } from "react-i18next";

export interface DialogModalProps {
  title: string;
  children: ReactNode;
  visible?: boolean;
  onClose?: (visible: boolean) => void;
  onOk?: () => void;
  okLabel?: string;
  cancelLabel?: string;
  hideOk?: boolean;
  /** Destructive confirm (delete, reject, etc.) */
  variant?: "default" | "destructive";
}

function DialogModal({
  children,
  title,
  visible = false,
  onClose,
  onOk,
  okLabel,
  cancelLabel,
  hideOk = false,
  variant = "default",
}: DialogModalProps) {
  const { t } = useTranslation();

  useEffect(() => {
    if (!visible) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose?.(false);
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [visible, onClose]);

  useEffect(() => {
    if (!visible) return;
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = prev;
    };
  }, [visible]);

  if (!visible) return null;

  const okClass =
    variant === "destructive"
      ? "bg-error text-on-accent hover:opacity-90"
      : "bg-primary text-on-accent hover:opacity-90";

  return createPortal(
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 p-4 backdrop-blur-[1px]"
      role="presentation"
      onClick={() => onClose?.(false)}
    >
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="dialog-title"
        className="flex max-h-[90vh] w-full max-w-md flex-col overflow-hidden rounded-xl border border-border/80 bg-card shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <header className="relative flex shrink-0 items-start justify-between gap-3 border-b border-modal-divider bg-[color-mix(in_srgb,var(--secondary)_32%,var(--card))] px-5 py-3">
          <span
            className="absolute inset-y-2 left-0 w-0.5 rounded-r-full bg-[color-mix(in_srgb,var(--primary)_45%,transparent)]"
            aria-hidden
          />
          <h2
            id="dialog-title"
            className="min-w-0 flex-1 pl-2 pr-2 text-base font-medium tracking-tight text-foreground"
          >
            {title}
          </h2>
          <button
            type="button"
            onClick={() => onClose?.(false)}
            className="shrink-0 rounded-md p-1.5 text-muted transition-colors hover:bg-[color-mix(in_srgb,var(--secondary)_65%,var(--card))] hover:text-foreground focus:outline-none"
            aria-label={t("Close")}
          >
            <X size={17} strokeWidth={1.75} />
          </button>
        </header>

        <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4 text-sm leading-relaxed text-muted">{children}</div>

        <footer className="flex shrink-0 flex-wrap items-center justify-end gap-2 border-t border-modal-divider bg-[color-mix(in_srgb,var(--secondary)_18%,var(--card))] px-5 py-3">
          <button
            type="button"
            className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary"
            onClick={() => onClose?.(false)}
          >
            {cancelLabel ?? t("Cancel")}
          </button>
          {!hideOk && (
            <button
              type="button"
              className={`rounded-lg px-4 py-2 text-sm font-semibold transition-opacity ${okClass}`}
              onClick={() => {
                onOk?.();
                onClose?.(false);
              }}
            >
              {okLabel ?? t("OK")}
            </button>
          )}
        </footer>
      </div>
    </div>,
    document.body,
  );
}

export default DialogModal;
