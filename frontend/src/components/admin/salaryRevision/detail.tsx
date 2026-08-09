"use client";
import { memo, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Send, CheckCircle2, Play, Trash2, FileQuestion } from "lucide-react";
import ButtonField from "@/components/ui/buttonField";
import Loading from "@/components/common/loader/loader";
import { EntityListShell } from "@/template";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { SalaryRevisionLineModel } from "@/models";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";
import {
  getSalaryRevision, submitSalaryRevision, approveSalaryRevision,
  applySalaryRevision, deleteSalaryRevision,
} from "@/services/admin/compensation";
import { money, revisionStatusBadge } from "./shared";

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
          <span className={`tabular-nums ${(l.increase ?? 0) > 0 ? "text-primary" : "text-muted"}`}>
            {(l.increase ?? 0) > 0 ? "+" : ""}{money(l.increase)} ({l.increasePercent}%)
          </span>
        ),
      },
    ];

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
  }, [isStep, isPerformance, t]);

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

      {error && (
        <p className="rounded-md border border-error/30 bg-error/10 px-3 py-2 text-sm text-error">{t(error)}</p>
      )}

      <div className="flex flex-wrap items-center justify-end gap-2">
        {detail.status === "Draft" && (
          <ButtonField value="Submit" variant="outline" icon={<Send size={14} />} disabled={busy}
            onClick={() => run(() => submitSalaryRevision(id))} />
        )}
        {detail.status === "PendingApproval" && (
          <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy}
            onClick={() => run(() => approveSalaryRevision(id))} />
        )}
        {detail.status === "Approved" && (
          <ButtonField value="Apply" variant="primary" icon={<Play size={14} />} disabled={busy}
            onClick={() => run(() => applySalaryRevision(id), true)} />
        )}
        {detail.status !== "Applied" && (
          <ButtonField value="Delete" variant="danger" icon={<Trash2 size={14} />} disabled={busy}
            onClick={() => run(() => deleteSalaryRevision(id), true)} />
        )}
      </div>
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
