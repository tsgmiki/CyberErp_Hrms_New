"use client";
import { memo, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Send, CheckCircle2, Play, Trash2 } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import DetailSection from "@/components/common/detailSection";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import ButtonField from "@/components/ui/buttonField";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import Loading from "../../common/loader/loader";
import { getSalaryRevision, submitSalaryRevision, approveSalaryRevision, applySalaryRevision, deleteSalaryRevision } from "@/services/admin/compensation";
import { money, revisionStatusBadge } from "./shared";

/** HC228 — salary-revision detail (per-employee lines) + lifecycle actions. */
function SalaryRevisionDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);

  const { data: detail, isLoading } = useQuery({ queryKey: ["salaryRevision", id], queryFn: () => getSalaryRevision(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["salaryRevisions"] });
    queryClient.invalidateQueries({ queryKey: ["salaryRevision", id] });
  };
  const run = async (fn: () => Promise<{ ok: boolean; message: string }>, close = false) => {
    setBusy(true);
    const r = await fn();
    setBusy(false);
    if (r.ok) { invalidate(); if (close) onClose(); }
  };

  const columns = useMemo(
    () =>
      [
        { name: "employeeName", label: "Employee", render: (_t: unknown, l: any) => <>{l.employeeName}<span className="ml-1 text-xs text-muted">{l.employeeNumber}</span></> },
        { name: "currentSalary", label: "Current", render: (_t: unknown, l: any) => <span className="tabular-nums">{money(l.currentSalary)}</span> },
        { name: "proposedSalary", label: "Proposed", render: (_t: unknown, l: any) => <span className="font-medium tabular-nums">{money(l.proposedSalary)}</span> },
        { name: "increase", label: "Change", render: (_t: unknown, l: any) => <span className="tabular-nums text-primary">+{money(l.increase)} ({l.increasePercent}%)</span> },
      ] as DataTableColumnModel[],
    [],
  );

  return (
    <DialogModal title={detail?.name ?? t("Salary Revision")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !detail ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{detail.name}</p>
              <p className="truncate text-xs text-muted">{detail.employeeCount} {t("employees")} · +{money(detail.totalIncrease)} ({detail.averagePercent}%)</p>
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${revisionStatusBadge(detail.status)}`}>{t(detail.status ?? "")}</span>
          </div>

          <DetailSection title="Lines">
            <DataTableProvider dataTable={{ columns, data: detail.lines ?? [], count: detail.lines?.length ?? 0, pagination: "None", search: "None", key: "id" }} />
          </DetailSection>

          <div className="flex flex-wrap items-center justify-end gap-2 border-t border-border pt-3">
            {detail.status === "Draft" && <ButtonField value="Submit" variant="outline" icon={<Send size={14} />} disabled={busy} onClick={() => run(() => submitSalaryRevision(id))} />}
            {detail.status === "PendingApproval" && <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy} onClick={() => run(() => approveSalaryRevision(id))} />}
            {detail.status === "Approved" && <ButtonField value="Apply" variant="primary" icon={<Play size={14} />} disabled={busy} onClick={() => run(() => applySalaryRevision(id), true)} />}
            {detail.status !== "Applied" && <ButtonField value="Delete" variant="danger" icon={<Trash2 size={14} />} disabled={busy} onClick={() => run(() => deleteSalaryRevision(id), true)} />}
          </div>
        </div>
      )}
    </DialogModal>
  );
}

export default memo(SalaryRevisionDetailModal);
