"use client";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

/** Titled card section — the standard grouping used inside detail popups (loan / trip / claim …). */
function DetailSection({ title, children }: { title: string; children: ReactNode }) {
  const { t } = useTranslation();
  return (
    <section className="rounded-lg border border-border bg-card/60 p-3">
      <h4 className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-muted">{t(title)}</h4>
      {children}
    </section>
  );
}

export default DetailSection;
