"use client";
import { useMemo, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import type { LoanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";

export const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Titled card section — the standard grouping used inside the loan detail popups. */
export function DetailSection({ title, children }: { title: string; children: ReactNode }) {
  const { t } = useTranslation();
  return (
    <section className="rounded-lg border border-border bg-card/60 p-3">
      <h4 className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-muted">{t(title)}</h4>
      {children}
    </section>
  );
}

export const loanStatusBadge = (s?: string) => ({
  Requested: "bg-warning/15 text-warning", Approved: "bg-primary/15 text-primary", Active: "bg-success/15 text-success",
  Settled: "bg-secondary/40 text-foreground", Rejected: "bg-error/15 text-error", Cancelled: "bg-muted/30 text-muted", Disbursed: "bg-primary/15 text-primary",
}[s ?? ""] ?? "bg-muted/30 text-muted");

export function LoanStat({ label, v, strong }: { label: string; v: string; strong?: boolean }) {
  return <div><p className="text-[11px] uppercase tracking-wide text-muted">{label}</p><p className={`tabular-nums ${strong ? "text-base font-bold text-primary" : "font-semibold text-foreground"}`}>{v}</p></div>;
}

/** Summary grid + repayment progress bar shared by the HR and self-service loan modals. */
export function LoanSummary({ loan }: { loan: LoanModel }) {
  const { t } = useTranslation();
  const paid = loan.paidInstallmentCount ?? 0;
  const total = loan.totalInstallmentCount ?? 0;
  const pct = total > 0 ? Math.round((paid / total) * 100) : 0;
  return (
    <div className="space-y-3">
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <LoanStat label={t("Principal")} v={money(loan.principalAmount)} />
        <LoanStat label={t("Interest")} v={loan.interestRatePct ? `${money(loan.totalInterest)} (${loan.interestRatePct}%)` : money(0)} />
        <LoanStat label={t("Total repayable")} v={money(loan.totalRepayable)} />
        <LoanStat label={t("Monthly")} v={money(loan.monthlyInstallment)} strong />
        <LoanStat label={t("Term")} v={`${loan.termMonths} ${t("mo")}`} />
        <LoanStat label={t("Outstanding")} v={money(loan.outstandingBalance)} strong />
        <LoanStat label={t("Service commit.")} v={loan.serviceCommitmentMonths ? `${loan.serviceCommitmentMonths} ${t("mo")}` : "—"} />
        <LoanStat label={t("Consent")} v={loan.serviceCommitmentConsentAt ? loan.serviceCommitmentConsentAt.slice(0, 10) : t("Pending")} />
      </div>
      {total > 0 && (
        <div>
          <div className="mb-1 flex justify-between text-xs text-muted"><span>{t("Repayment progress")}</span><span>{paid}/{total} ({pct}%)</span></div>
          <div className="h-2 w-full overflow-hidden rounded-full bg-secondary/40"><div className="h-full rounded-full bg-primary" style={{ width: `${pct}%` }} /></div>
        </div>
      )}
      {loan.disbursementReference && <p className="text-xs text-muted">{t("Disbursement ref")}: {loan.disbursementReference} · {loan.disbursedAt?.slice(0, 10)}</p>}
      {loan.resolution && <p className="text-xs text-muted">{t("Note")}: {loan.resolution}</p>}
    </div>
  );
}

export function GuarantorList({ loan }: { loan: LoanModel }) {
  if ((loan.guarantors?.length ?? 0) === 0) return null;
  return (
    <div className="flex flex-wrap gap-2">
      {loan.guarantors!.map((g) => (
        <span key={g.id} className="rounded-md border border-border bg-card px-2 py-1 text-xs">{g.fullName}{g.relationship ? ` · ${g.relationship}` : ""}{g.guaranteedAmount ? ` · ${money(g.guaranteedAmount)}` : ""}</span>
      ))}
    </div>
  );
}

/** Repayment schedule rendered via the standard data grid. */
export function ScheduleTable({ loan }: { loan: LoanModel }) {
  const rows = loan.schedule ?? [];
  const columns = useMemo(
    () =>
      [
        { name: "installmentNo", label: "#" },
        { name: "dueDate", label: "Due", render: (v: string) => v?.slice(0, 10) },
        { name: "principalPortion", label: "Principal", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.principalPortion)}</span> },
        { name: "interestPortion", label: "Interest", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.interestPortion)}</span> },
        { name: "amount", label: "Amount", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.amount)}</span> },
        {
          name: "status", label: "Status",
          render: (v: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${v === "Paid" ? "bg-success/15 text-success" : "bg-warning/15 text-warning"}`}>{v}</span>,
        },
      ] as DataTableColumnModel[],
    [],
  );
  if (rows.length === 0) return null;
  return (
    <div className="max-h-56 overflow-auto">
      <DataTableProvider dataTable={{ columns, data: rows, count: rows.length, pagination: "None", search: "None", key: "id" }} />
    </div>
  );
}
