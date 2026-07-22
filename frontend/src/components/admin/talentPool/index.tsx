"use client";

import { useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Star, History, Send, BadgeCheck } from "lucide-react";
import { EntityListShell, useEntityList } from "@/template";
import {
  getAllCandidates,
  getAllJobApplications,
  getAllJobRequisitions,
  createJobApplication,
} from "@/services/admin/recruitment";
import type { CandidateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type ParameterModel from "@/models/ParameterModel";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const lookupParam = { ...parameterInitialData, take: 200 };
const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

const STAGE_TONE: Record<string, string> = {
  Received: "bg-muted/30 text-muted",
  Screening: "bg-info/15 text-info",
  Shortlisted: "bg-primary/10 text-primary",
  Interview: "bg-warning/15 text-warning",
  Selected: "bg-success/15 text-success",
  OfferPending: "bg-warning/15 text-warning",
  Hired: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Withdrawn: "bg-muted/30 text-muted",
};

/** A past applicant's full application history (requirement #4). */
function HistoryModal({ candidate, onClose }: { candidate: CandidateModel; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["candidateApplications", candidate.id],
    queryFn: () =>
      getAllJobApplications({
        ...parameterInitialData,
        take: 100,
        categoryId: candidate.id,
      } as unknown as ParameterModel),
  });

  return (
    <Modal
      visible
      size="lg"
      title={t("Application History")}
      description={`${candidate.fullName} (${candidate.candidateNumber})`}
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
      {!isLoading && (data?.data ?? []).length === 0 && (
        <p className="py-6 text-center text-sm text-muted">{t("No applications on record.")}</p>
      )}
      {!isLoading && (data?.data ?? []).length > 0 && (
        <table className="w-full text-[13px]">
          <thead>
            <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
              <th className="px-3 py-2 font-semibold">{t("Vacancy")}</th>
              <th className="px-3 py-2 font-semibold">{t("Applied")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Score")}</th>
              <th className="px-3 py-2 font-semibold">{t("Stage")}</th>
            </tr>
          </thead>
          <tbody>
            {(data?.data ?? []).map((a) => (
              <tr key={a.id} className="border-b border-border/60">
                <td className="px-3 py-2">
                  <span className="block font-medium text-foreground">{a.requisitionTitle}</span>
                  <span className="block text-xs text-muted">{a.requisitionNumber}</span>
                </td>
                <td className="px-3 py-2 text-muted">{fmtDate(a.appliedAt)}</td>
                <td className="px-3 py-2 text-right tabular-nums">{a.screeningScore ?? "—"}</td>
                <td className="px-3 py-2">
                  <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STAGE_TONE[a.stage ?? ""] ?? ""}`}>
                    {t(a.stage ?? "")}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Modal>
  );
}

/** Reference a past applicant into a new opportunity (requirement #4). */
function ApplyModal({
  candidate,
  onClose,
  onDone,
}: {
  candidate: CandidateModel;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [requisitionId, setRequisitionId] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: requisitions } = useQuery({
    queryKey: ["jobRequisitions", "open-lookup"],
    queryFn: () => getAllJobRequisitions(lookupParam),
  });
  const open = (requisitions?.data ?? []).filter(
    (r) => r.status === "Posted" || r.status === "Approved",
  );

  const confirm = async () => {
    if (!requisitionId) {
      setError(t("Select an open requisition."));
      return;
    }
    setBusy(true);
    const res = await createJobApplication({ candidateId: candidate.id!, requisitionId });
    setBusy(false);
    if (!res.ok) {
      setError(res.message);
      return;
    }
    onDone();
  };

  return (
    <Modal
      visible
      size="md"
      title={t("Apply to Vacancy")}
      description={`${candidate.fullName} (${candidate.candidateNumber})`}
      onClose={onClose}
      footer={
        <>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Cancel")}
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={confirm}
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            {t("Register Application")}
          </button>
        </>
      }
    >
      <label className="mb-1 block text-xs font-semibold uppercase tracking-wide text-muted">
        {t("Open Requisition")} <span className="text-error">*</span>
      </label>
      <select
        value={requisitionId}
        onChange={(e) => setRequisitionId(e.target.value)}
        className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
      >
        <option value="">{t("Select…")}</option>
        {open.map((r) => (
          <option key={r.id} value={r.id}>
            {r.requisitionNumber} — {r.title}
          </option>
        ))}
      </select>
      {error && <p className="mt-2 text-xs text-error">{error}</p>}
    </Modal>
  );
}

const FILTERS = [
  { id: "", label: "All Past Applicants" },
  { id: "TalentPool", label: "Talent Pool" },
  { id: "Archived", label: "Archived" },
];

/**
 * Talent Pool — the searchable past-applicant interface (requirement #4): every candidate on
 * record, their application history, and one-click referencing into new opportunities (HC089/HC096).
 */
function TalentPool() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [historyFor, setHistoryFor] = useState<CandidateModel | null>(null);
  const [applyFor, setApplyFor] = useState<CandidateModel | null>(null);

  const list = useEntityList({
    queryKey: "talentPool",
    fetchPage: getAllCandidates,
  });

  const activeFilter = (list.param.status as string) || "";
  const setFilter = (status: string) =>
    list.setParam((p) => ({ ...p, status: status || undefined, skip: 0 }) as never);

  const columns = useMemo(
    () =>
      [
        {
          name: "fullName",
          label: "Candidate",
          sort: true,
          render: (text: string, r: CandidateModel) => (
            <span className="block">
              <span className="flex items-center gap-1.5 font-semibold">
                {text}
                {r.isInTalentPool && <Star size={13} className="text-warning" />}
                {r.hiredEmployeeId && <BadgeCheck size={13} className="text-success" />}
              </span>
              <span className="block text-xs text-muted">
                {r.candidateNumber}
                {r.email ? ` · ${r.email}` : ""}
              </span>
            </span>
          ),
        },
        {
          name: "skillsSummary",
          label: "Skills",
          render: (v: string) => (
            <span className="block max-w-[260px] truncate text-xs text-muted" title={v}>
              {v || "—"}
            </span>
          ),
        },
        {
          name: "yearsOfExperience",
          label: "Exp (Yrs)",
          render: (v: number) => <span className="tabular-nums">{v ?? "—"}</span>,
        },
        {
          name: "applicationCount",
          label: "Past Applications",
          render: (v: number) => <span className="tabular-nums">{v ?? 0}</span>,
        },
        {
          name: "talentPoolNotes",
          label: "Pool Notes",
          render: (v: string) => (
            <span className="block max-w-[200px] truncate text-xs text-muted" title={v}>
              {v || "—"}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: CandidateModel) => (
            <span className="inline-flex items-center gap-1">
              <button
                type="button"
                onClick={() => setHistoryFor(record)}
                title={t("Application History")}
                className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
              >
                <History size={14} />
              </button>
              <button
                type="button"
                disabled={!!record.hiredEmployeeId || !!record.anonymizedAt}
                onClick={() => setApplyFor(record)}
                title={t("Apply to a new vacancy")}
                className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary disabled:cursor-not-allowed disabled:opacity-40"
              >
                <Send size={14} />
              </button>
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <div className="m-1 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="flex flex-wrap items-center gap-2 border-b border-border px-3 py-2">
        <h1 className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <Star size={16} className="text-primary" />
          {t("Talent Pool")}
          <span className="text-xs font-normal text-muted">
            — {t("past applicants, searchable for new opportunities (HC089/HC096)")}
          </span>
        </h1>
        <div className="ml-auto flex items-center gap-1">
          {FILTERS.map((f) => (
            <button
              key={f.id}
              type="button"
              onClick={() => setFilter(f.id)}
              className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                activeFilter === f.id ? "bg-primary/10 text-primary" : "text-muted hover:text-foreground"
              }`}
            >
              {t(f.label)}
            </button>
          ))}
        </div>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell listKey="talentPool" listLabel="Talent Pool" columns={columns} {...list} />
      </div>

      {historyFor && <HistoryModal candidate={historyFor} onClose={() => setHistoryFor(null)} />}
      {applyFor && (
        <ApplyModal
          candidate={applyFor}
          onClose={() => setApplyFor(null)}
          onDone={() => {
            setApplyFor(null);
            queryClient.invalidateQueries({ queryKey: ["talentPool"] });
            queryClient.invalidateQueries({ queryKey: ["jobApplications"] });
          }}
        />
      )}
    </div>
  );
}

export default TalentPool;
