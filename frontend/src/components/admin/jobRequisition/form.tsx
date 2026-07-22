"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { JobRequisitionModel, ScreeningCriterionModel, CandidateMatchModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  Send,
  Hourglass,
  Megaphone,
  XCircle,
  Plus,
  Wand2,
  Users,
  Trophy,
} from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "../../common/loader/loader";
import {
  getJobRequisition,
  saveJobRequisition,
  submitJobRequisition,
  postJobRequisition,
  closeJobRequisition,
  cancelJobRequisition,
  generateRequisitionPosting,
  setRequisitionPosting,
  getAllHiringRequests,
  matchCandidates,
} from "@/services/admin/recruitment";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
import CriteriaModal from "./criteriaModal";
import RankingModal from "./rankingModal";
import { parameterInitialData } from "@/constants/initialization";
import {
  plannedEmploymentTypeOptions,
  postingChannelOptions,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 200 };

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  PendingApproval: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Posted: "bg-info/15 text-info",
  Closed: "bg-secondary text-foreground",
  Cancelled: "bg-muted/30 text-muted",
  Rejected: "bg-error/15 text-error",
};


/** Ranked internal/talent-pool matches for this vacancy (HC090). */
function MatchModal({ requisitionId, onClose }: { requisitionId: string; onClose: () => void }) {
  const { t } = useTranslation();
  const { data, isLoading } = useQuery({
    queryKey: ["candidateMatches", requisitionId],
    queryFn: () => matchCandidates(requisitionId),
  });

  return (
    <Modal
      visible
      size="lg"
      title={t("Matched Candidates")}
      description={t("Ranked by skills overlap, experience and talent-pool membership (HC090).")}
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
        <p className="py-6 text-center text-sm text-muted">
          {t("No matching candidates — capture skills on candidate profiles to enable matching.")}
        </p>
      )}
      {!isLoading && (data ?? []).length > 0 && (
        <table className="w-full text-[13px]">
          <thead>
            <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
              <th className="px-3 py-2 font-semibold">{t("Candidate")}</th>
              <th className="px-3 py-2 font-semibold">{t("Source")}</th>
              <th className="px-3 py-2 font-semibold">{t("Matched Skills")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Experience")}</th>
              <th className="px-3 py-2 text-right font-semibold">{t("Score")}</th>
            </tr>
          </thead>
          <tbody>
            {(data ?? []).map((m: CandidateMatchModel) => (
              <tr key={m.candidateId} className="border-b border-border/60">
                <td className="px-3 py-2 font-medium text-foreground">
                  {m.fullName}
                  {m.isInTalentPool && (
                    <span className="ml-1.5 rounded bg-info/15 px-1.5 py-0.5 text-[10px] font-semibold text-info">
                      {t("TALENT POOL")}
                    </span>
                  )}
                  <span className="block text-xs font-normal text-muted">{m.candidateNumber}</span>
                </td>
                <td className="px-3 py-2 text-muted">{t(m.source)}</td>
                <td className="px-3 py-2 text-xs text-muted">{m.matchedSkills.join(", ") || "—"}</td>
                <td className={`px-3 py-2 text-right tabular-nums ${m.meetsExperience ? "text-success" : "text-warning"}`}>
                  {m.yearsOfExperience ?? "—"}
                </td>
                <td className="px-3 py-2 text-right font-bold tabular-nums text-primary">{m.matchScore}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Modal>
  );
}

function JobRequisitionForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<JobRequisitionModel>({
    numberOfPositions: 1,
    employmentType: "Permanent",
    postingChannel: "Internal",
  });
  const [criteria, setCriteria] = useState<ScreeningCriterionModel[]>([]);
  const [showCriteria, setShowCriteria] = useState(false);
  // Apply (in the popup) stages the criteria locally — they persist with Save Requisition.
  // The dirty flag keeps that visible so applied-but-unsaved criteria are never silently lost.
  const [criteriaDirty, setCriteriaDirty] = useState(false);
  const [busy, setBusy] = useState(false);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [showMatches, setShowMatches] = useState(false);
  const [showRanking, setShowRanking] = useState(false);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["jobRequisition", id],
    queryFn: () => getJobRequisition(id),
    enabled: typeof id != "undefined" && id != "",
  });
  // Only APPROVED hiring requests may source a requisition (HC080).
  const { data: approvedRequests } = useQuery({
    queryKey: ["hiringRequests", "approved-lookup"],
    queryFn: () => getAllHiringRequests({ ...lookupParam, status: "Approved" } as never),
  });
  const { data: locations } = useQuery({
    queryKey: ["workLocations", lookupParam],
    queryFn: () => getAllWorkLocation(lookupParam),
  });
  const readOnly = !!record && record.status !== "Draft" && record.status !== "Rejected";
  const postingEditable = !!record && record.status !== "Closed" && record.status !== "Cancelled";

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData(record);
      setCriteria(record.screeningCriteria ?? []);
      setCriteriaDirty(false);
    }
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setCriteriaDirty(false);
      queryClient.invalidateQueries({ queryKey: ["jobRequisitions"] });
      if (!formData.id && formState.id) {
        setId(formState.id);
        queryClient.invalidateQueries({ queryKey: ["jobRequisition", formState.id] });
      } else if (formData.id) {
        queryClient.invalidateQueries({ queryKey: ["jobRequisition", formData.id] });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id, [`${name.replace(/Id$/, "")}Name`]: r.name }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const result = await saveJobRequisition({ ...formData, screeningCriteria: criteria });
    setFormState(result);
    setIsLoading(false);
  };

  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["jobRequisition", id] });
    queryClient.invalidateQueries({ queryKey: ["jobRequisitions"] });
    queryClient.invalidateQueries({ queryKey: ["workflows"] });
    queryClient.invalidateQueries({ queryKey: ["myApprovals"] });
  }, [queryClient, id]);

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    setActionMessage(res.message);
    refresh();
  };

  /** Regenerates the advertisement from the requisition details (HC091). */
  const generatePosting = async () => {
    setBusy(true);
    try {
      const text = await generateRequisitionPosting(id);
      setFormData((p) => ({ ...p, postingText: text }));
      setActionMessage(t("Posting text generated — review, then Save Posting."));
    } finally {
      setBusy(false);
    }
  };

  const savePosting = () =>
    run(() =>
      setRequisitionPosting({
        id,
        postingChannel: formData.postingChannel || "Internal",
        postingText: formData.postingText,
        openFrom: formData.openFrom || undefined,
        openUntil: formData.openUntil || undefined,
      }),
    );


  return (
    <div className="text-foreground">
      {pending && <Loading />}

      {record && (
        <div className="mb-2 flex flex-wrap items-center gap-2 text-sm">
          <span className="font-semibold">{record.requisitionNumber}</span>
          <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[record.status ?? ""] ?? ""}`}>
            {t(record.status ?? "")}
          </span>
          {record.awaitingWorkflow && (
            <span className="flex items-center gap-1 rounded border border-info/30 bg-info/10 px-2 py-0.5 text-xs text-info">
              <Hourglass size={12} /> {t("Awaiting workflow approval")}
            </span>
          )}
          <span className="rounded bg-secondary px-2 py-0.5 text-xs">
            {t("Applications")}: {record.applicationCount ?? 0}
          </span>
        </div>
      )}

      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: (readOnly ? "none" : "top") as "top",
          submitBtnTitle: "Save Requisition",
          components: [
            {
              name: "hiringRequestId", label: "Approved Hiring Request", required: true, type: "dropDown",
              onSelect: selectHandler, value: formData.hiringRequestId,
              displayValue: formData.hiringRequestNumber ?? (formData as any).hiringRequestName,
              disabled: readOnly || !!formData.id,
              placeholder: "Requisitions start from an approved need (HC080)",
              error: formState?.zodErrors?.hiringRequestId,
              data: (approvedRequests?.data ?? []).map((r) => ({
                id: r.id,
                name: `${r.requestNumber} — ${r.positionClassTitle} (${r.numberOfPositions} @ ${r.organizationUnitName})`,
              })) as never,
            },
            {
              name: "title", label: "Title", type: "text",
              placeholder: "Defaults from the role's position class",
              value: formData.title, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "numberOfPositions", label: "Openings", required: true, type: "text",
              value: formData.numberOfPositions, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "employmentType", label: "Employment Type", type: "dropDown", onSelect: selectHandler,
              value: formData.employmentType, displayValue: formData.employmentType, disabled: readOnly,
              data: plannedEmploymentTypeOptions as never,
            },
            {
              name: "workLocationId", label: "Work Location", type: "dropDown", onSelect: selectHandler,
              value: formData.workLocationId, displayValue: formData.workLocationName, disabled: readOnly,
              data: (locations?.data ?? []).map((l) => ({ id: l.id, name: l.name })) as never,
            },
            {
              name: "minExperienceYears", label: "Min Experience (Years)", type: "text",
              value: formData.minExperienceYears, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "description", label: "Description", type: "textarea", colSpan: "full",
              value: formData.description, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "minQualifications", label: "Min Qualifications", type: "textarea", colSpan: "full",
              value: formData.minQualifications, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "skills", label: "Skills (comma-separated)", type: "text", colSpan: "full",
              value: formData.skills, onChange: changeHandler, disabled: readOnly,
            },
          ],
        }}
      />

      {/* Screening criteria (HC095) — summary card; editing happens in the popup grid */}
      <div className="mt-3 rounded-lg border border-border bg-card p-3">
        <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
          <h4 className="text-sm font-semibold">
            {t("Screening Criteria")} <span className="text-xs font-normal text-muted">(HC095)</span>
            {criteria.length > 0 && (
              <span
                className={`ml-2 rounded px-2 py-0.5 text-xs font-bold tabular-nums ${
                  criteria.reduce((s, c) => s + (Number(c.weight) || 0), 0) === 100
                    ? "bg-success/15 text-success"
                    : "bg-error/15 text-error"
                }`}
              >
                Σ {criteria.reduce((s, c) => s + (Number(c.weight) || 0), 0)}%
              </span>
            )}
            {/* Apply ≠ Save: applied criteria stay local until the requisition is saved. */}
            {criteriaDirty && (
              <span className="ml-2 rounded bg-warning/15 px-2 py-0.5 text-xs font-semibold text-warning">
                {t("Not saved yet — Save Requisition to persist")}
              </span>
            )}
          </h4>
          <button
            type="button"
            onClick={() => setShowCriteria(true)}
            className="inline-flex items-center gap-1 rounded border border-border px-2.5 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
          >
            <Plus size={13} />{" "}
            {readOnly ? t("View Criteria") : criteria.length === 0 ? t("Define Criteria") : t("Edit Criteria")}
          </button>
        </div>
        {criteria.length === 0 ? (
          <p className="text-xs text-muted">{t("No criteria — applicants are screened on the job specification only.")}</p>
        ) : (
          <div className="flex flex-wrap gap-1.5">
            {criteria.map((c, i) => (
              <span
                key={i}
                className="inline-flex items-center gap-1.5 rounded-full border border-border bg-secondary/50 px-2.5 py-1 text-xs text-foreground"
              >
                <span className="font-medium">{c.name}</span>
                <span className="font-bold tabular-nums text-primary">{c.weight}%</span>
                {c.appliesAtStage && (
                  <span className="rounded bg-info/15 px-1.5 py-0.5 text-[10px] font-semibold text-info">
                    {t(c.appliesAtStage)}
                  </span>
                )}
                {c.isMandatory && <span className="text-error" title={t("Mandatory")}>*</span>}
                {(c.evaluators?.length ?? 0) > 0 && (
                  <span
                    className="text-[10px] text-muted"
                    title={(c.evaluators ?? []).map((e) => e.name).join(", ")}
                  >
                    · {c.evaluators!.length === 1
                      ? c.evaluators![0].name
                      : t("{{n}} evaluators", { n: c.evaluators!.length })}
                  </span>
                )}
              </span>
            ))}
          </div>
        )}
      </div>

      {showCriteria && (
        <CriteriaModal
          initial={criteria}
          readOnly={readOnly}
          onClose={() => setShowCriteria(false)}
          onApply={(rows) => {
            setCriteria(rows);
            setCriteriaDirty(true);
            setShowCriteria(false);
          }}
        />
      )}

      {/* Posting designer (HC088/HC091) — editable until closed */}
      {id && (
        <div className="mt-3 rounded-lg border border-border bg-card p-3">
          <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
            <h4 className="text-sm font-semibold">
              {t("Job Posting")}{" "}
              <span className="text-xs font-normal text-muted">
                ({t("internal / external market — HC088")})
              </span>
            </h4>
            {postingEditable && (
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  disabled={busy}
                  onClick={generatePosting}
                  className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
                >
                  <Wand2 size={13} /> {t("Generate from Requisition")}
                </button>
                <button
                  type="button"
                  disabled={busy}
                  onClick={savePosting}
                  className="inline-flex items-center gap-1 rounded border border-primary/40 bg-primary/10 px-2 py-1 text-xs font-medium text-primary hover:bg-primary/20 disabled:opacity-50"
                >
                  {t("Save Posting")}
                </button>
              </div>
            )}
          </div>
          <div className="mb-2 grid grid-cols-1 gap-2 md:grid-cols-3">
            <select
              disabled={!postingEditable || busy}
              value={formData.postingChannel || "Internal"}
              onChange={(e) => setFormData((p) => ({ ...p, postingChannel: e.target.value }))}
              className="h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground disabled:opacity-60"
            >
              {postingChannelOptions.map((o) => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>
            <input
              type="date"
              disabled={!postingEditable || busy}
              value={(formData.openFrom ?? "").slice(0, 10)}
              onChange={(e) => setFormData((p) => ({ ...p, openFrom: e.target.value }))}
              title={t("Open from")}
              className="h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground disabled:opacity-60"
            />
            <input
              type="date"
              disabled={!postingEditable || busy}
              value={(formData.openUntil ?? "").slice(0, 10)}
              onChange={(e) => setFormData((p) => ({ ...p, openUntil: e.target.value }))}
              title={t("Open until")}
              className="h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground disabled:opacity-60"
            />
          </div>
          <textarea
            rows={8}
            disabled={!postingEditable || busy}
            value={formData.postingText ?? ""}
            onChange={(e) => setFormData((p) => ({ ...p, postingText: e.target.value }))}
            placeholder={t("Generate a standard advertisement from the requisition, then customize (HC091)…")}
            className="w-full resize-y rounded-md border border-border bg-background px-3 py-2 font-mono text-xs text-foreground disabled:opacity-60"
          />
        </div>
      )}

      {/* Lifecycle actions */}
      <div className="mt-3 flex flex-wrap items-center gap-2">
        {id && !readOnly && (
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => submitJobRequisition(id))}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Send size={14} /> {t("Submit for Approval")}
          </button>
        )}
        {record?.status === "Approved" && (
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => postJobRequisition(id))}
            title={t("Publishes to the selected channel(s); requires posting text")}
            className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Megaphone size={14} /> {t("Publish Posting")}
          </button>
        )}
        {(record?.status === "Approved" || record?.status === "Posted") && (
          <>
            <button
              type="button"
              onClick={() => setShowMatches(true)}
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary"
            >
              <Users size={14} /> {t("Match Candidates")}
            </button>
            <button
              type="button"
              onClick={() => setShowRanking(true)}
              title={t("Weighted evaluator scores per criterion — totals auto-calculated")}
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary"
            >
              <Trophy size={14} /> {t("Ranking")}
            </button>
            <button
              type="button"
              disabled={busy}
              onClick={() => run(() => closeJobRequisition(id))}
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-error hover:text-error disabled:opacity-50"
            >
              <XCircle size={14} /> {t("Close Requisition")}
            </button>
          </>
        )}
        {record && (record.status === "Draft" || record.status === "Rejected") && (
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => cancelJobRequisition(id))}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-muted hover:border-error hover:text-error disabled:opacity-50"
          >
            <XCircle size={14} /> {t("Cancel")}
          </button>
        )}
        {actionMessage && <span className="text-xs text-muted">{actionMessage}</span>}
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {showMatches && id && <MatchModal requisitionId={id} onClose={() => setShowMatches(false)} />}
      {showRanking && id && <RankingModal requisitionId={id} onClose={() => setShowRanking(false)} />}
    </div>
  );
}

export default JobRequisitionForm;
