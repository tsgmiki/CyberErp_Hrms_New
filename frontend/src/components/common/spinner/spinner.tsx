import { Loader2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import "./spinner.css";

export type SpinnerSize = "xs" | "sm" | "md" | "lg";
export type SpinnerVariant = "ring" | "icon";

export interface SpinnerProps {
  size?: SpinnerSize;
  /** `ring` = dual-ring spinner (page/section); `icon` = Loader2 (inline, dropdowns). */
  variant?: SpinnerVariant;
  label?: string;
  /** Show "Loading..." under the indicator. Default true for `block` and `fullPage`. */
  showLabel?: boolean;
  /** Fixed overlay (e.g. auth gate). */
  fullPage?: boolean;
  /** Centered in a tall area (tables, forms, Suspense). */
  block?: boolean;
  className?: string;
}

const RING_SIZE_CLASS: Record<SpinnerSize, string> = {
  xs: "h-5 w-5 border-2",
  sm: "h-7 w-7 border-2",
  md: "h-10 w-10 border-[2.5px]",
  lg: "h-14 w-14 border-[3px]",
};

const ICON_SIZE_CLASS: Record<SpinnerSize, string> = {
  xs: "h-3.5 w-3.5",
  sm: "h-4 w-4",
  md: "h-6 w-6",
  lg: "h-8 w-8",
};

const CENTER_DOT_CLASS: Record<SpinnerSize, string> = {
  xs: "h-1 w-1",
  sm: "h-1.5 w-1.5",
  md: "h-2 w-2",
  lg: "h-2.5 w-2.5",
};

const ICON_RING_CLASS: Record<SpinnerSize, string> = {
  xs: "h-6 w-6 border",
  sm: "h-7 w-7 border",
  md: "h-9 w-9 border-2",
  lg: "h-11 w-11 border-2",
};

function RingIndicator({ size }: { size: SpinnerSize }) {
  const ring = RING_SIZE_CLASS[size];
  const dot = CENTER_DOT_CLASS[size];

  return (
    <div
      role="status"
      aria-label="Loading"
      className="relative flex shrink-0 items-center justify-center"
    >
      <span
        className={`loader-pulse-glow absolute inset-0 rounded-full ${ring}`}
        aria-hidden
      />
      <span
        className={`loader-spin absolute inset-0 rounded-full border-primary border-t-transparent ${ring}`}
        aria-hidden
      />
      <span
        className={`loader-spin-reverse absolute inset-0 rounded-full border-transparent border-b-primary ${ring}`}
        aria-hidden
      />
      <span
        className={`loader-center-pulse absolute rounded-full bg-primary ${dot}`}
        aria-hidden
      />
    </div>
  );
}

function IconIndicator({ size }: { size: SpinnerSize }) {
  const icon = ICON_SIZE_CLASS[size];
  const ring = ICON_RING_CLASS[size];

  return (
    <span
      role="status"
      aria-label="Loading"
      className="relative flex shrink-0 items-center justify-center"
    >
      <span
        className={`loader-spin-reverse absolute rounded-full border-primary/25 border-t-primary/60 ${ring}`}
        aria-hidden
      />
      <span className="loader-icon-pulse relative flex items-center justify-center">
        <Loader2 className={`loader-spin text-primary ${icon}`} aria-hidden />
      </span>
    </span>
  );
}

function LoadingLabel({ text }: { text: string }) {
  const base = text.replace(/\.{2,}$|…$/u, "").trimEnd();

  return (
    <p className="loader-fade-up flex items-center gap-0.5 text-sm text-muted">
      <span>{base}</span>
      <span className="inline-flex min-w-[1.25rem]" aria-hidden>
        {[0, 1, 2].map((i) => (
          <span
            key={i}
            className="loader-dot"
            style={{ animationDelay: `${i * 0.14}s` }}
          >
            .
          </span>
        ))}
      </span>
    </p>
  );
}

function Spinner({
  size = "md",
  variant = "ring",
  label,
  showLabel,
  fullPage = false,
  block = false,
  className = "",
}: SpinnerProps) {
  const { t } = useTranslation();
  const shouldShowLabel = showLabel ?? (fullPage || block);
  const labelText = shouldShowLabel ? (label ?? t("Loading...")) : null;

  const indicator =
    variant === "icon" ? (
      <IconIndicator size={size} />
    ) : (
      <RingIndicator size={size} />
    );

  const content = (
    <div
      className={`loader-fade-up flex flex-col items-center gap-3 ${className}`}
    >
      {indicator}
      {labelText ? <LoadingLabel text={labelText} /> : null}
    </div>
  );

  if (fullPage) {
    return (
      <div className="loader-fade-up loader-backdrop-pulse fixed inset-0 z-50 flex items-center justify-center bg-background/60 backdrop-blur-[2px]">
        {content}
      </div>
    );
  }

  if (block) {
    return (
      <div className="flex h-full min-h-[12rem] w-full items-center justify-center p-8">
        {content}
      </div>
    );
  }

  return content;
}

export default Spinner;
