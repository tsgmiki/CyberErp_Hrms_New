"use client";
import { lazy, memo, useMemo, useRef, useState, Suspense } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Send, CheckCircle2, Play, Trash2, FileQuestion, ArrowUpRight, Hourglass, History } from "lucide-react";
import ButtonField from "@/components/ui/buttonField";
import Loading from "@/components/common/loader/loader";
import { EntityListShell } from "@/template";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { SalaryRevisionLineModel } from "@/models";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";
import {
  getSalaryRevision, sendSalaryRevisionForApproval, submitSalaryRevision, approveSalaryRevision,
  applySalaryRevision, deleteSalaryRevision,
} from "@/services/admin/compensation";
import { money, revisionStatusBadge } from "./shared";

// Lazy: the audit trail is opened deliberately, so its chunk should not sit on the path of simply
// viewing the increment grid.
const HistoryModal = memo(lazy(() => import("./historyModal")));

interface Props {
  id: string;
  onBack: () => void;
}

/**
 * HC228 — the per-employee increment grid for one salary revision.
 *
 * This replaces the former detail POPUP with a full grid view: selecting a row in the Salary
 * Revisions list swaps the page to this grid, and the shell's standard Back arrow returns. Using
 * {@link EntityListShell} means it inherits the same chrome as every other list — search, column
 * picker, export, list/grid toggle, pagination — instead of a bespoke in-dialog table.
 *
 * The revision endpoint returns ALL its lines in one payload, so paging/searching is done here in
 * memory rather than adding a second paged endpoint for data that is already loaded.
 */
function SalaryRevisionDetail({ id, onBack }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);
  const busyRef = useRef(false);
  const [error, setError] = useState<string | null>(null);
  const [param, setParam] = useState<ParameterModel>({ ...parameterInitialData });
  const [displayMode, setDisplayMode] = useState<ListDisplayMode>("list");
  const [showHistory, setShowHistory] = useState(false);

  // retry:false — a 404 means the revision is genuinely gone; retrying only delays the
  // "no longer available" state behind a spinner.
  const { data: detail, isLoading, isError } = useQuery({
    queryKey: ["salaryRevision", id],
    queryFn: () => getSalaryRevision(id),
    retry: false,
  });

  const refreshList = () => queryClient.invalidateQueries({ queryKey: ["salaryRevisions"] });

  /** Stay on the detail: refetch it so the new status shows. */
  const refreshDetail = () => queryClient.invalidateQueries({ queryKey: ["salaryRevision", id] });

  /*
   * When the record is GONE, the detail query must simply be left alone.
   *
   * This component is still mounted at that moment — the navigation happens on the next line — so its
   * `useQuery` observer is still active, and BOTH cache operations force a request for an id that no
   * longer exists:
   *   invalidateQueries -> marks stale and refetches active queries  -> GET 404
   *   removeQueries     -> drops the entry, so the active observer refetches to repopulate -> GET 404
   * That 404 is what surfaced "Resource of type 'SalaryRevision' with id ... was not found" straight
   * after a delete that had in fact succeeded.
   * Doing nothing is correct: on unmount the query goes inactive and is garbage-collected, and
   * re-opening the same id later refetches and lands on the "no longer available" panel, which is the
   * right outcome.
   */

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>, backAfter = false) => {
    // Re-entrancy guard held in a REF, not state. Both `disabled={busy}` and a `if (busy) return`
    // check read a value captured at render time, so two clicks landing in the same frame both see
    // false and both fire — which is exactly how a double-click on Delete produced a second request
    // that came back "Resource of type 'SalaryRevision' ... was not found". A ref updates
    // synchronously, so the second click sees the guard already closed.
    if (busyRef.current) return;
    busyRef.current = true;
    setBusy(true);
    setError(null);
    const r = await fn();
    busyRef.current = false;
    setBusy(false);

    if (r.ok) {
      refreshList();
      if (backAfter) onBack(); else refreshDetail();
      return;
    }
    // Already gone (deleted in another tab, or a duplicate submit that succeeded): resync and leave,
    // rather than showing a backend exception for work that is in fact done.
    if (/not found/i.test(r.message)) { refreshList(); onBack(); return; }
    setError(r.message);
  };

  const lines = useMemo(() => detail?.lines ?? [], [detail]);
  const isPerformance = detail?.revisionType === "Performance";
  const isStep = detail?.basis === "Step";

  // Employees the eligibility rules removed never get a line, so a saved revision only ever shows
  // the people who qualified. What IS worth showing here is who was paid a reduced increment and
  // the service that decided it — otherwise a prorated figure looks like an arithmetic error.
  const prorated = useMemo(
    () => lines.filter((l) => (l.prorationFactor ?? 1) < 1).length,
    [lines],
  );
  const hasService = useMemo(() => lines.some((l) => l.monthsOfService != null), [lines]);
  const promotions = useMemo(() => lines.filter((l) => l.promotedToGradeCode).length, [lines]);
  const hasPromotion = promotions > 0;

  // Client-side search + paging over the already-loaded lines.
  const filtered = useMemo(() => {
    const q = (param.searchText ?? "").trim().toLowerCase();
    if (!q) return lines;
    return lines.filter((l) =>
      `${l.employeeName ?? ""} ${l.employeeNumber ?? ""}`.toLowerCase().includes(q));
  }, [lines, param.searchText]);

  const rows = useMemo(
    () => filtered.slice(param.skip, param.skip + param.take),
    [filtered, param.skip, param.take],
  );

  const columns = useMemo(() => {
    const base: DataTableColumnModel[] = [
      {
        name: "employeeName", label: "Employee", sort: true,
        render: (_t: unknown, l: SalaryRevisionLineModel) => (
          <span className="font-medium">{l.employeeName}</span>
        ),
      },
      { name: "employeeNumber", label: "Employee No." },
      {
        name: "hireDate", label: "Hired Date",
        render: (_t: unknown, l: SalaryRevisionLineModel) =>
          l.hireDate
            ? <span className="tabular-nums">{String(l.hireDate).slice(0, 10)}</span>
            : <span className="text-muted">—</span>,
      },
      {
        name: "currentSalary", label: "Current",
        render: (_t: unknown, l: SalaryRevisionLineModel) => (
          <span className="tabular-nums">{money(l.currentSalary)}</span>
        ),
      },
      {
        name: "proposedSalary", label: "Proposed",
        render: (_t: unknown, l: SalaryRevisionLineModel) => (
          <span className="font-medium tabular-nums">{money(l.proposedSalary)}</span>
        ),
      },
      {
        name: "increase", label: "Change",
        render: (_t: unknown, l: SalaryRevisionLineModel) => (
          <span className="flex items-center gap-1.5">
            <span className={`tabular-nums ${(l.increase ?? 0) > 0 ? "text-primary" : "text-muted"}`}>
              {(l.increase ?? 0) > 0 ? "+" : ""}{money(l.increase)} ({l.increasePercent}%)
            </span>
            {/* The reduced figure needs its reason attached to it, not just in a footnote. */}
            {(l.prorationFactor ?? 1) < 1 && (
              <span
                className="shrink-0 rounded-full bg-warning/15 px-1.5 py-0.5 text-[10px] font-semibold text-warning"
                title={t("Prorated — the employee is inside their first year")}
              >
                {t("prorated")}
              </span>
            )}
          </span>
        ),
      },
    ];

    // Only when the rules actually produced service data — a revision planned before the policy
    // existed has none, and an empty column would just imply missing hire dates.
    if (hasService) {
      base.push({
        name: "monthsOfService", label: "Service",
        render: (_t: unknown, l: SalaryRevisionLineModel) =>
          l.monthsOfService == null ? <span className="text-muted">—</span> : (
            <span className="tabular-nums">
              {l.monthsOfService} {t("mo")}
              {(l.prorationFactor ?? 1) < 1 && (
                <span className="ml-1 text-xs text-muted">
                  ({l.monthsOfService}/12)
                </span>
              )}
            </span>
          ),
      } as DataTableColumnModel);
    }

    // Only surface the basis/type-specific columns when they carry meaning, so a plain percentage
    // revision is not padded with empty Step / Score columns.
    if (isStep) {
      base.push({
        name: "proposedStep", label: "Step",
        render: (_t: unknown, l: SalaryRevisionLineModel) =>
          l.proposedStep == null ? <span className="text-muted">—</span> : (
            <span className="tabular-nums">
              {l.currentStep} → {l.proposedStep}
              {l.interpolated && <span className="ml-1 text-xs text-muted">{t("interpolated")}</span>}
            </span>
          ),
      } as DataTableColumnModel);
    }
    // A grade move is a bigger change than a pay move, so it gets its own column rather than living
    // only in the note — this is the thing an approver most needs to notice before applying.
    if (hasPromotion) {
      base.push({
        name: "promotedToGradeCode", label: "New Grade",
        render: (_t: unknown, l: SalaryRevisionLineModel) =>
          l.promotedToGradeCode
            ? (
              <span className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-semibold text-primary">
                <ArrowUpRight size={12} />
                {l.promotedToGradeCode}
              </span>
            )
            : <span className="text-muted">—</span>,
      } as DataTableColumnModel);
    }
    if (isPerformance) {
      base.push({
        name: "performanceScore", label: "Score",
        render: (_t: unknown, l: SalaryRevisionLineModel) =>
          l.performanceScore == null
            ? <span className="text-muted">—</span>
            : <span className="tabular-nums">{l.performanceScore}{l.bandLabel ? ` · ${l.bandLabel}` : ""}</span>,
      } as DataTableColumnModel);
    }
    // A line that did not move carries the reason (off-scale, no appraisal, ceiling, pay held).
    base.push({
      name: "note", label: "Note",
      render: (_t: unknown, l: SalaryRevisionLineModel) =>
        l.note ? <span className="text-xs text-warning">{t(l.note)}</span> : <span className="text-muted">—</span>,
    } as DataTableColumnModel);

    return base;
  }, [isStep, isPerformance, hasService, hasPromotion, t]);

  if (isLoading) return <Loading />;

  if (isError || !detail) {
    return (
      <div className="flex min-h-[40vh] flex-col items-center justify-center gap-3 p-6 text-center">
        <FileQuestion className="h-10 w-10 text-muted" />
        <h2 className="text-base font-semibold text-foreground">{t("This revision is no longer available")}</h2>
        <p className="max-w-md text-sm text-muted">
          {t("It has been deleted, or you no longer have access to it.")}
        </p>
        {/* The record is already gone here, so drop its query rather than invalidating it. */}
        <ButtonField value="Back to list" variant="outline" onClick={() => { refreshList(); onBack(); }} />
      </div>
    );
  }

  const header = (
    <div className="space-y-2">
      <div className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border bg-secondary/20 px-3 py-2">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <p className="truncate text-sm font-semibold text-foreground">{detail.name}</p>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${revisionStatusBadge(detail.status)}`}>
              {t(detail.status ?? "")}
            </span>
            {/* Says WHY there is no Approve button, so its absence reads as "someone else's turn"
                rather than a missing permission. */}
            {detail.awaitingWorkflow && (
              <span className="flex items-center gap-1 rounded border border-info/30 bg-info/10 px-2 py-0.5 text-xs text-info">
                <Hourglass size={12} /> {t("Awaiting workflow approval")}
              </span>
            )}
          </div>
          <p className="mt-0.5 truncate text-xs text-muted">
            {t(detail.revisionType ?? "")} · {t(detail.basis ?? "")}
            {detail.effectiveDate ? ` · ${t("effective")} ${String(detail.effectiveDate).slice(0, 10)}` : ""}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-4 text-sm">
          <span className="text-muted">{t("Employees")}: <span className="font-semibold tabular-nums text-foreground">{detail.employeeCount}</span></span>
          <span className="text-muted">{t("Current")}: <span className="font-semibold tabular-nums text-foreground">{money(detail.totalCurrent)}</span></span>
          <span className="text-muted">{t("Proposed")}: <span className="font-semibold tabular-nums text-foreground">{money(detail.totalProposed)}</span></span>
          <span className="text-muted">{t("Increase")}: <span className="font-semibold tabular-nums text-primary">+{money(detail.totalIncrease)} ({detail.averagePercent}%)</span></span>
        </div>
      </div>

      {/* Employees the rules excluded were never written as lines, so say so rather than leaving the
          headcount looking unexpectedly short. */}
      {(prorated > 0 || promotions > 0) && (
        <div className="flex flex-wrap gap-4 rounded-md border border-border bg-secondary/20 px-3 py-2 text-xs">
          {prorated > 0 && (
            <span className="text-muted">
              {t("Prorated (first year)")}:{" "}
              <span className="font-semibold tabular-nums text-warning">{prorated}</span>
            </span>
          )}
          {promotions > 0 && (
            <span className="text-muted">
              {t("Promoted a grade")}:{" "}
              <span className="font-semibold tabular-nums text-primary">{promotions}</span>
            </span>
          )}
          <span className="text-muted">
            {t("Employees excluded by the increment rules are not listed here.")}
          </span>
        </div>
      )}

      {/* Applying this revision will change job grades, not just pay. That is worth saying ON the
          screen where Apply lives, not only in the policy the approver may never have opened. */}
      {promotions > 0 && detail.status !== "Applied" && (
        <p className="rounded-md border border-primary/30 bg-primary/5 px-3 py-2 text-xs text-foreground">
          {t("Applying this revision moves")}{" "}
          <span className="font-semibold tabular-nums">{promotions}</span>{" "}
          {t("employee(s) onto a new job grade, shown in the New Grade column.")}
        </p>
      )}

      {error && (
        <p className="rounded-md border border-error/30 bg-error/10 px-3 py-2 text-sm text-error">{t(error)}</p>
      )}

      <div className="flex flex-wrap items-center justify-end gap-2">
        {/* Available in every state, including Applied — the audit question ("who approved this?") is
            asked most often about revisions that have already been paid. */}
        <ButtonField value="History" variant="outline" icon={<History size={14} />}
          onClick={() => setShowHistory(true)} />
        {/* A draft's author may only SEND IT FOR APPROVAL — never submit it. Submission is a separate
            act that appears once the approver has approved, so nobody can commit their own pay
            decision. The backend enforces the same order; this only keeps the UI honest about it. */}
        {detail.status === "Draft" && (
          <ButtonField value="Send for Approval" variant="outline" icon={<Send size={14} />} disabled={busy}
            onClick={() => run(() => sendSalaryRevisionForApproval(id))} />
        )}
        {/* Sending for approval starts the workflow, and the backend then REFUSES a direct approve
            (EnsureNoRunningAsync). Offering the button anyway gives an action that can only fail —
            approval belongs to the workflow from that point on. */}
        {detail.status === "PendingApproval" && !detail.awaitingWorkflow && (
          <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy}
            onClick={() => run(() => approveSalaryRevision(id))} />
        )}
        {detail.status === "Approved" && (
          <ButtonField value="Submit" variant="primary" icon={<Send size={14} />} disabled={busy}
            onClick={() => run(() => submitSalaryRevision(id))} />
        )}
        {/* Stay on the grid after applying: the status becomes Applied, which is what removes Apply and
            Delete, and the refreshed lines are the record of what was just paid. */}
        {detail.status === "Submitted" && (
          <ButtonField value="Apply" variant="primary" icon={<Play size={14} />} disabled={busy}
            onClick={() => run(() => applySalaryRevision(id))} />
        )}
        {detail.status !== "Applied" && (
          <ButtonField value="Delete" variant="danger" icon={<Trash2 size={14} />} disabled={busy}
            onClick={() => run(() => deleteSalaryRevision(id), true)} />
        )}
      </div>

      {showHistory && (
        <Suspense fallback={null}>
          <HistoryModal revisionId={id} revisionName={detail.name} onClose={() => setShowHistory(false)} />
        </Suspense>
      )}
    </div>
  );

  return (
    <EntityListShell
      listKey="salaryRevisionLines"
      listLabel="Salary Increment"
      columns={columns}
      isLoading={false}
      rows={rows}
      total={filtered.length}
      param={param}
      setParam={setParam}
      displayMode={displayMode}
      setDisplayMode={setDisplayMode}
      fetchAllData={async () => filtered as unknown as Record<string, unknown>[]}
      header={header}
    />
  );
}

export default memo(SalaryRevisionDetail);
