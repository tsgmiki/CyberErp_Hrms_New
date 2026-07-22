"use client";

import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Wallet } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import {
  getAllHiringRequests,
  deleteHiringRequest,
  getRecruitmentBudgetMonitor,
} from "@/services/admin/recruitment";
import type { HiringRequestModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  Submitted: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Closed: "bg-info/15 text-info",
};

const fmtMoney = (v?: number) => (v ?? 0).toLocaleString(undefined, { maximumFractionDigits: 0 });

/** Per-unit recruitment budget/headcount monitor (HC083). */
function BudgetMonitorModal({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["recruitmentBudgetMonitor"],
    queryFn: getRecruitmentBudgetMonitor,
  });

  return (
    <Modal
      visible
      size="lg"
      title={t("Recruitment Budget Monitor")}
      description={t("Approved + submitted hiring needs per directorate (HC083).")}
      onClose={onClose}
      footer={
        <button
          type="button"
          onClick={onClose}
          className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
        >
          {t("Close")}
        </button>
      }
    >
      {isLoading && <Loading />}
      {!isLoading && (data ?? []).length === 0 && (
        <p className="py-6 text-center text-sm text-muted">{t("No active hiring needs.")}</p>
      )}
      {!isLoading && (data ?? []).length > 0 && (
        <table className="w-full text-[13px]">
          <thead>
            <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
              <th className="px-3 py-2 font-semibold">{t("Unit")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Approved Requests")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Positions")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Estimated Budget")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Open Requisitions")}</th>
            </tr>
          </thead>
          <tbody>
            {(data ?? []).map((r) => (
              <tr key={r.organizationUnitId} className="border-b border-border/60">
                <td className="px-3 py-2 font-medium text-foreground">{r.organizationUnitName}</td>
                <td className="px-3 py-2 text-right tabular-nums">{r.approvedRequests}</td>
                <td className="px-3 py-2 text-right tabular-nums">{r.requestedPositions}</td>
                <td className="px-3 py-2 text-right tabular-nums">{fmtMoney(r.estimatedBudget)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{r.openRequisitions}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Modal>
  );
}

interface Props {
  editHandler: (id: string) => void;
}

function HiringRequestList({ editHandler }: Props) {
  const { t } = useTranslation();
  const [showBudget, setShowBudget] = useState(false);

  const list = useEntityList({
    queryKey: "hiringRequests",
    fetchPage: getAllHiringRequests,
    deleteById: deleteHiringRequest,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "requestNumber",
          label: "Request",
          sort: true,
          render: (text: string, r: HiringRequestModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{text}</span>
              <span className="block text-xs text-muted">
                {r.organizationUnitName} · {r.positionClassTitle}
              </span>
            </button>
          ),
        },
        {
          name: "numberOfPositions",
          label: "Positions",
          render: (v: number, r: HiringRequestModel) => (
            <span className="tabular-nums">
              {v} <span className="text-xs text-muted">({t(r.employmentType ?? "")})</span>
            </span>
          ),
        },
        {
          name: "estimatedBudget",
          label: "Est. Budget",
          render: (v: number) => <span className="tabular-nums">{fmtMoney(v)}</span>,
        },
        {
          name: "workforcePlanName",
          label: "Workforce Plan",
          render: (v: string) => v || <span className="text-xs text-muted">—</span>,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text] ?? ""}`}>
              {t(text)}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: HiringRequestModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete={record.status === "Draft" || record.status === "Rejected"}
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="mb-2 flex items-center justify-end px-1">
        <button
          type="button"
          onClick={() => setShowBudget(true)}
          className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary"
        >
          <Wallet size={14} /> {t("Budget Monitor")}
        </button>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="hiringRequests"
          listLabel="Hiring Requests"
          columns={columns}
          {...list}
        />
      </div>
      {showBudget && <BudgetMonitorModal onClose={() => setShowBudget(false)} />}
    </div>
  );
}

export default HiringRequestList;
