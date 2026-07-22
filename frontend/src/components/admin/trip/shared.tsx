"use client";
import { useMemo, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { TripRequestModel } from "@/models";

export const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Titled card section — the standard grouping used inside the trip detail popups. */
export function DetailSection({ title, children }: { title: string; children: ReactNode }) {
  const { t } = useTranslation();
  return (
    <section className="rounded-lg border border-border bg-card/60 p-3">
      <h4 className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-muted">{t(title)}</h4>
      {children}
    </section>
  );
}

export const tripStatusBadge = (s?: string) => ({
  Requested: "bg-warning/15 text-warning", Approved: "bg-primary/15 text-primary", InProgress: "bg-primary/15 text-primary",
  Completed: "bg-primary/15 text-primary", Settled: "bg-success/15 text-success", Rejected: "bg-error/15 text-error", Cancelled: "bg-muted/30 text-muted",
}[s ?? ""] ?? "bg-muted/30 text-muted");

function Stat({ label, v, strong }: { label: string; v: string; strong?: boolean }) {
  return <div><p className="text-[11px] uppercase tracking-wide text-muted">{label}</p><p className={`tabular-nums ${strong ? "text-base font-bold text-primary" : "font-semibold text-foreground"}`}>{v}</p></div>;
}

export function TripSummary({ trip }: { trip: TripRequestModel }) {
  const { t } = useTranslation();
  const c = (n?: number | null) => `${money(n)} ${trip.currency ?? ""}`.trim();
  return (
    <div className="space-y-2">
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <Stat label={t("Destination")} v={trip.destination ?? "—"} />
        <Stat label={t("Dates")} v={`${trip.startDate?.slice(0, 10)} → ${trip.endDate?.slice(0, 10)}`} />
        <Stat label={t("Days")} v={String(trip.days ?? 0)} />
        <Stat label={t("Per-diem/day")} v={c(trip.dailyPerDiemRate)} />
        <Stat label={t("Per-diem")} v={c(trip.perDiemAmount)} />
        <Stat label={t("Advance")} v={c(trip.advanceAmount)} strong />
        <Stat label={t("Expenses")} v={c(trip.totalExpenses)} />
        <Stat label={t("Settlement net")} v={trip.settlementNet == null ? "—" : c(trip.settlementNet)} strong />
      </div>
      {trip.advanceReference && <p className="text-xs text-muted">{t("Advance ref")}: {trip.advanceReference} · {trip.advanceDisbursedAt?.slice(0, 10)}</p>}
      {trip.settledAt && <p className="text-xs text-muted">{t("Settled")}: {trip.settledAt.slice(0, 10)}{trip.settlementReference ? ` · ${trip.settlementReference}` : ""}{(trip.settlementNet ?? 0) > 0 ? ` — ${t("refund from employee")}` : (trip.settlementNet ?? 0) < 0 ? ` — ${t("reimburse employee")}` : ""}</p>}
      {trip.purpose && <p className="text-xs text-muted">{t("Purpose")}: {trip.purpose}</p>}
      {trip.resolution && <p className="text-xs text-muted">{t("Note")}: {trip.resolution}</p>}
    </div>
  );
}

export function ExpenseTable({ trip }: { trip: TripRequestModel }) {
  const { t } = useTranslation();
  const rows = trip.expenses ?? [];
  const columns = useMemo(
    () =>
      [
        { name: "category", label: "Category" },
        { name: "expenseDate", label: "Date", render: (v: string) => v?.slice(0, 10) },
        { name: "description", label: "Description", render: (v: string) => v || "—" },
        { name: "amount", label: "Amount", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.amount)}</span> },
      ] as DataTableColumnModel[],
    [],
  );
  if (rows.length === 0) return <p className="text-xs text-muted">{t("No expenses recorded.")}</p>;
  return (
    <div>
      <DataTableProvider dataTable={{ columns, data: rows, count: rows.length, pagination: "None", search: "None", key: "id" }} />
      <p className="mt-1 text-right text-sm font-semibold tabular-nums">{t("Total")}: {money(trip.totalExpenses)}</p>
    </div>
  );
}
