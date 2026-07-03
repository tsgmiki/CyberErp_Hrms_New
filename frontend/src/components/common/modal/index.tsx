import { X } from "lucide-react";
import type { ReactNode } from "react";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";

export type ModalSize = "sm" | "md" | "lg" | "xl" | "fullscreen";

const MODAL_SIZE_CLASS: Record<ModalSize, string> = {
  sm: "max-w-md",
  md: "max-w-lg",
  lg: "max-w-2xl",
  xl: "max-w-4xl",
  fullscreen: "max-w-[min(96vw,1200px)]",
};

interface ModalProps {
  title: string;
  children: ReactNode;
  visible?: boolean;
  onClose?: (visible: boolean) => void;
  /** @deprecated Prefer `size="fullscreen"` */
  showFullscreen?: boolean;
  size?: ModalSize;
  description?: string;
  footer?: ReactNode;
}

function Modal({
  children,
  title,
  visible = false,
  onClose,
  showFullscreen,
  size,
  description,
  footer,
}: ModalProps) {
  const { t } = useTranslation();

  useEffect(() => {
    if (!visible) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose?.(false);
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [visible, onClose]);

  if (!visible) return null;

  const resolvedSize: ModalSize =
    size ?? (showFullscreen ? "fullscreen" : "md");
  const widthClass = MODAL_SIZE_CLASS[resolvedSize];

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/30 p-4 backdrop-blur-[1px]"
      role="presentation"
      onClick={() => onClose?.(false)}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
        aria-describedby={description ? "modal-description" : undefined}
        className={`flex max-h-[min(90vh,900px)] w-full flex-col overflow-hidden rounded-xl border border-border/80 bg-card shadow-xl ${widthClass}`}
        onClick={(e) => e.stopPropagation()}
      >
        <header className="relative flex shrink-0 items-start justify-between gap-3 border-b border-modal-divider bg-[color-mix(in_srgb,var(--secondary)_32%,var(--card))] px-5 py-3">
          <span
            className="absolute inset-y-2 left-0 w-0.5 rounded-r-full bg-[color-mix(in_srgb,var(--primary)_45%,transparent)]"
            aria-hidden
          />
          <div className="min-w-0 flex-1 pl-2 pr-2">
            <h2
              id="modal-title"
              className="text-base font-medium tracking-tight text-foreground"
            >
              {title}
            </h2>
            {description ? (
              <p id="modal-description" className="mt-0.5 text-sm leading-relaxed text-muted">
                {description}
              </p>
            ) : null}
          </div>
          <button
            type="button"
            className="shrink-0 rounded-md p-1.5 text-muted transition-colors hover:bg-[color-mix(in_srgb,var(--secondary)_65%,var(--card))] hover:text-foreground focus:outline-none"
            onClick={() => onClose?.(false)}
            aria-label={t("Close")}
          >
            <X size={17} strokeWidth={1.75} />
          </button>
        </header>

        <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">{children}</div>

        {footer ? (
          <footer className="flex shrink-0 flex-wrap items-center justify-end gap-2 border-t border-modal-divider bg-[color-mix(in_srgb,var(--secondary)_18%,var(--card))] px-5 py-3">
            {footer}
          </footer>
        ) : null}
      </div>
    </div>
  );
}

export default Modal;
