"use client";
import { memo, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { EntityFormTabs, type EntityFormTab } from "@/components/common/tabs/entityFormTabs";
import { useFormLayoutPreference, type FormLayout } from "./useFormLayoutPreference";

/** A form section — the same shape as an `EntityFormTab`, reused across all three layouts. */
export type FormLayoutSection = EntityFormTab;

const LAYOUT_OPTIONS: { id: FormLayout; label: string }[] = [
  { id: "leftnav", label: "Sections" },
  { id: "tabs", label: "Tabs" },
  { id: "cards", label: "Cards" },
];

/** Polished vertical section cards (icon + title + hint header per group). */
const SectionCards = memo(function SectionCards({
  sections,
  hasId,
  disabledHint,
}: {
  sections: FormLayoutSection[];
  hasId: boolean;
  disabledHint: string;
}) {
  const { t } = useTranslation();
  return (
    <div className="space-y-4">
      {sections.map(({ key, label, Icon, description, needsId, content }) => (
        <section key={key} className="rounded-xl border border-border bg-card shadow-sm">
          <header className="flex items-start gap-3 border-b border-border px-5 py-3.5">
            {Icon ? (
              <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                <Icon size={16} />
              </span>
            ) : null}
            <div className="min-w-0">
              <h3 className="text-sm font-semibold text-foreground">{t(label)}</h3>
              {description ? <p className="mt-0.5 text-xs text-muted">{t(description)}</p> : null}
            </div>
          </header>
          <div className="p-5">
            {needsId && !hasId ? (
              <p className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-center text-xs text-muted">{t(disabledHint)}</p>
            ) : (
              content
            )}
          </div>
        </section>
      ))}
    </div>
  );
});

/** The layout-picker combobox — place it standalone (e.g. in a form header) or via `FormLayoutSwitcher`. */
export function LayoutSwitcherControl({
  layout,
  onChange,
  className = "",
}: {
  layout: FormLayout;
  onChange: (next: FormLayout) => void;
  className?: string;
}) {
  const { t } = useTranslation();
  return (
    <select
      aria-label={t("Form layout")}
      value={layout}
      onChange={(e) => onChange(e.target.value as FormLayout)}
      className={`h-9 cursor-pointer rounded-lg border border-border bg-card px-2.5 text-xs font-medium text-foreground transition-colors hover:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 ${className}`}
    >
      {LAYOUT_OPTIONS.map((o) => (
        <option key={o.id} value={o.id}>
          {t(o.label)}
        </option>
      ))}
    </select>
  );
}

/** Renders `sections` in the given layout (Cards / Tabs / Left-nav) — the visual half of the switcher. */
export const FormLayoutRenderer = memo(function FormLayoutRenderer({
  sections,
  hasId,
  layout,
  disabledHint = "Save first",
}: {
  sections: FormLayoutSection[];
  hasId: boolean;
  layout: FormLayout;
  disabledHint?: string;
}) {
  return layout === "cards" ? (
    <SectionCards sections={sections} hasId={hasId} disabledHint={disabledHint} />
  ) : (
    <EntityFormTabs tabs={sections} hasId={hasId} dir={layout === "leftnav" ? "left" : "top"} disabledHint={disabledHint} />
  );
});

/**
 * A reusable record-form layout switcher. Renders the same `sections` as **Cards**, **Top Tabs**, or a
 * **Left Section Nav**, and remembers each HR admin's choice (per user, per `storageKey`) in localStorage.
 * Drop it into any module: pass the section list and a stable `storageKey`. For a custom header, compose
 * `useFormLayoutPreference` + `LayoutSwitcherControl` + `FormLayoutRenderer` yourself instead.
 */
function FormLayoutSwitcherBase({
  sections,
  hasId,
  storageKey,
  defaultLayout = "leftnav",
  disabledHint = "Save first",
  toolbar,
}: {
  sections: FormLayoutSection[];
  hasId: boolean;
  /** Stable key for persisting the choice (e.g. "employee-master"). */
  storageKey: string;
  defaultLayout?: FormLayout;
  disabledHint?: string;
  /** Optional content rendered on the left of the switcher row. */
  toolbar?: ReactNode;
}) {
  const [layout, setLayout] = useFormLayoutPreference(storageKey, defaultLayout);
  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-2">
        <div className="min-w-0">{toolbar}</div>
        <LayoutSwitcherControl layout={layout} onChange={setLayout} />
      </div>
      <FormLayoutRenderer sections={sections} hasId={hasId} layout={layout} disabledHint={disabledHint} />
    </div>
  );
}

export const FormLayoutSwitcher = memo(FormLayoutSwitcherBase);
