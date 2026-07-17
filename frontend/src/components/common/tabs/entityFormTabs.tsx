"use client";
import { memo, useState, type ReactNode } from "react";
import type { LucideIcon } from "lucide-react";
import { useTranslation } from "react-i18next";

export interface EntityFormTab {
  key: string;
  label: string;
  Icon?: LucideIcon;
  /** Optional one-line hint shown as a section subheader above the panel content. */
  description?: string;
  /** Tab is disabled until the record is saved (needs an id). */
  needsId?: boolean;
  /** Keep the panel mounted (hidden when inactive) so its form state survives tab switches. */
  keepMounted?: boolean;
  content: ReactNode;
}

/**
 * ERP-standard tabbed record editor (same underline-tab look as the Employee profile). The master tab
 * (keepMounted) stays in the DOM so its form state is preserved; child tabs mount lazily and can be
 * gated behind a saved record (needsId).
 *
 * `dir="left"` renders a vertical section navigation (enterprise master-detail layout) instead of the
 * default top tab bar.
 */
function EntityFormTabsBase({
  tabs,
  hasId,
  dir = "top",
  disabledHint = "Save first",
}: {
  tabs: EntityFormTab[];
  hasId: boolean;
  dir?: "top" | "left";
  disabledHint?: string;
}) {
  const { t } = useTranslation();
  const [active, setActive] = useState(tabs[0]?.key ?? "");
  const left = dir === "left";

  const tabButtons = tabs.map(({ key, label, Icon, needsId }) => {
    const disabled = !!needsId && !hasId;
    const isActive = active === key;
    if (left) {
      return (
        <button
          key={key}
          type="button"
          role="tab"
          disabled={disabled}
          title={disabled ? t(disabledHint) : undefined}
          onClick={() => setActive(key)}
          className={`flex w-full items-center gap-2.5 rounded-lg border-l-2 px-3 py-2 text-left text-[13px] font-medium transition-colors ${
            isActive
              ? "border-primary bg-primary/10 text-primary"
              : "border-transparent text-muted hover:bg-secondary/50 hover:text-foreground"
          } ${disabled ? "cursor-not-allowed opacity-40" : ""}`}
        >
          {Icon ? <Icon className="h-4 w-4 shrink-0" /> : null}
          <span className="truncate">{t(label)}</span>
        </button>
      );
    }
    return (
      <button
        key={key}
        type="button"
        role="tab"
        disabled={disabled}
        title={disabled ? t(disabledHint) : undefined}
        onClick={() => setActive(key)}
        className={`-mb-px flex items-center gap-1.5 rounded-t-lg border-x border-t px-3.5 py-2 text-[13px] font-medium transition-colors ${
          isActive
            ? "border-border bg-card text-primary"
            : "border-transparent text-muted hover:text-foreground"
        } ${disabled ? "cursor-not-allowed opacity-40" : ""}`}
      >
        {Icon ? <Icon className="h-4 w-4" /> : null}
        {t(label)}
      </button>
    );
  });

  const panels = tabs.map(({ key, label, Icon, description, needsId, keepMounted, content }) => {
    const isActive = active === key;
    const header = description ? (
      <div className="mb-4 flex items-center gap-2.5 border-b border-border pb-3">
        {Icon ? (
          <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
            <Icon className="h-4 w-4" />
          </span>
        ) : null}
        <div className="min-w-0">
          <h3 className="text-sm font-semibold text-foreground">{t(label)}</h3>
          <p className="text-xs text-muted">{t(description)}</p>
        </div>
      </div>
    ) : null;

    if (keepMounted) {
      return (
        <div key={key} role="tabpanel" hidden={!isActive} className={isActive ? "block" : "hidden"}>
          {header}
          {content}
        </div>
      );
    }
    if (!isActive) return null;
    if (needsId && !hasId) {
      return (
        <p key={key} className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-center text-xs text-muted">
          {t(disabledHint)}
        </p>
      );
    }
    return (
      <div key={key} role="tabpanel">
        {header}
        {content}
      </div>
    );
  });

  if (left) {
    return (
      <div className="flex flex-col gap-3 lg:flex-row lg:gap-5">
        <div
          className="flex gap-1 overflow-x-auto border-b border-border pb-2 lg:w-52 lg:shrink-0 lg:flex-col lg:overflow-visible lg:border-b-0 lg:border-r lg:pb-0 lg:pr-4"
          role="tablist"
        >
          {tabButtons}
        </div>
        <div className="min-w-0 flex-1">{panels}</div>
      </div>
    );
  }

  return (
    <div className="flex flex-col">
      <div className="mx-1 flex flex-wrap gap-1 border-b border-border pb-0" role="tablist">
        {tabButtons}
      </div>
      <div className="pt-3">{panels}</div>
    </div>
  );
}

export const EntityFormTabs = memo(EntityFormTabsBase);
