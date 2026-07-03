import { X } from "lucide-react";
import { type ReactNode, useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";

interface Props {
  isOpen?: boolean;
  hasCloseBtn?: boolean;
  onClose?: () => void;
  children: ReactNode;
  title?: string;
  description?: string;
  size?: "sm" | "md" | "lg";
}

const SIZE_CLASS = {
  sm: "max-w-md",
  md: "max-w-lg",
  lg: "max-w-2xl",
};

const Modal = ({
  children,
  isOpen = false,
  hasCloseBtn = true,
  onClose,
  title,
  description,
  size = "md",
}: Props) => {
  const { t } = useTranslation();
  const dialogRef = useRef<HTMLDialogElement>(null);
  const showHeader = Boolean(title || description || hasCloseBtn);

  useEffect(() => {
    const el = dialogRef.current;
    if (!el) return;
    if (isOpen) {
      if (!el.open) el.showModal();
    } else if (el.open) {
      el.close();
    }
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose?.();
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <dialog
      ref={dialogRef}
      className="fixed inset-0 z-[100] m-0 h-full w-full max-h-none max-w-none border-0 bg-transparent p-4 backdrop:bg-black/30 backdrop:backdrop-blur-[1px] open:flex open:items-center open:justify-center"
      onClose={() => onClose?.()}
      onClick={(e) => {
        if (e.target === dialogRef.current) onClose?.();
      }}
    >
      <div
        role="document"
        className={`flex max-h-[min(90vh,900px)] w-full flex-col overflow-hidden rounded-xl border border-border/80 bg-card shadow-xl ${SIZE_CLASS[size]}`}
        onClick={(e) => e.stopPropagation()}
      >
        {showHeader ? (
          <header className="relative flex shrink-0 items-start justify-between gap-3 border-b border-modal-divider bg-[color-mix(in_srgb,var(--secondary)_32%,var(--card))] px-5 py-3">
            <span
              className="absolute inset-y-2 left-0 w-0.5 rounded-r-full bg-[color-mix(in_srgb,var(--primary)_45%,transparent)]"
              aria-hidden
            />
            <div className="min-w-0 flex-1 pl-2">
              {title ? (
                <h2
                  id="modal-title"
                  className="text-base font-medium tracking-tight text-foreground"
                >
                  {title}
                </h2>
              ) : (
                <span />
              )}
              {description ? (
                <p
                  id="modal-description"
                  className="mt-0.5 text-sm leading-relaxed text-muted"
                >
                  {description}
                </p>
              ) : null}
            </div>
            {hasCloseBtn ? (
              <button
                type="button"
                className="shrink-0 rounded-md p-1.5 text-muted transition-colors hover:bg-[color-mix(in_srgb,var(--secondary)_65%,var(--card))] hover:text-foreground focus:outline-none focus:ring-2 focus:ring-primary/15"
                onClick={() => onClose?.()}
                aria-label={t("Close")}
              >
                <X size={17} strokeWidth={1.75} />
              </button>
            ) : null}
          </header>
        ) : null}
        <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">{children}</div>
      </div>
    </dialog>
  );
};

export default Modal;
