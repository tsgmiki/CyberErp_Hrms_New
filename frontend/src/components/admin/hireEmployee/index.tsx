"use client";
import { useEffect, useMemo, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { UserCheck, Award, RefreshCw } from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "@/components/common/loader/loader";
import { getHireQueue, hireCandidate, getJobOffers } from "@/services/admin/recruitment";
import getAllPosition from "@/services/admin/position/getAll";
import type { HireQueueRowModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import { employmentNatureOptions } from "@/constants/orgStructure";

const inputCls = "h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground";
const ordinal = (n: number) => `${n}${n === 1 ? "st" : n === 2 ? "nd" : n === 3 ? "rd" : "th"}`;

/** The hire conversion modal — creates the employee on the candidate's person record. */
function HireModal({
  row,
  onClose,
  onDone,
}: {
  row: HireQueueRowModel;
  onClose: () => void;
  onDone: () => void;
}) {
  const { t } = useTranslation();
  const [hire, setHire] = useState({
    employeeNumber: "",
    hireDate: "",
    positionId: "",
    employmentNature: "Permanent",
    contractPeriod: "",
    isProbation: false,
    probationEndDate: "",
    salary: "",
  });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { data: vacantPositions } = useQuery({
    queryKey: ["positions", "vacant-hire"],
    queryFn: () => getAllPosition({ ...parameterInitialData, take: 200, isVacant: true } as never),
  });

  // Auto-populate the salary from the candidate's offer (the agreed figure) — HR may override.
  // Position + salary are also resolved server-side from the offer/requisition when left blank.
  const { data: offers } = useQuery({
    queryKey: ["jobOffers", row.applicationId],
    queryFn: () => getJobOffers(row.applicationId),
  });
  const offerSalary = useMemo(() => {
    const list = offers ?? [];
    const accepted = list.find((o) => o.status === "Accepted");
    return (accepted ?? list[0])?.salary;
  }, [offers]);
  useEffect(() => {
    if (offerSalary != null) setHire((p) => (p.salary === "" ? { ...p, salary: String(offerSalary) } : p));
  }, [offerSalary]);

  const confirm = async () => {
    setError(null);
    if (!hire.employeeNumber.trim()) return setError(t("An employee number is required."));
    setBusy(true);
    const res = await hireCandidate(row.candidateId, {
      employeeNumber: hire.employeeNumber.trim(),
      hireDate: hire.hireDate || undefined,
      positionId: hire.positionId || undefined,
      salary: hire.salary === "" ? undefined : Number(String(hire.salary).replace(/[,\s]/g, "")),
      employmentNature: hire.employmentNature,
      contractPeriod: hire.contractPeriod === "" ? undefined : Number(hire.contractPeriod),
      isProbation: hire.isProbation,
      probationEndDate: hire.probationEndDate || undefined,
    });
    setBusy(false);
    if (!res.ok) return setError(res.message);
    onDone();
  };

  return (
    <Modal
      visible
      size="md"
      title={t("Hire as Employee")}
      description={`${row.candidateName} — ${row.requisitionTitle}`}
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
            className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
          >
            <UserCheck size={15} /> {t("Confirm Hire")}
          </button>
        </>
      }
    >
      <div className="space-y-2 text-sm">
        <p className="rounded-md border border-info/30 bg-info/10 px-3 py-2 text-xs text-foreground">
          {t("The employee is created on the candidate's existing person record — no re-entry. All attached documents (and the resume) migrate to the employee history automatically.")}
        </p>
        <p className="rounded-md border border-border bg-secondary/40 px-3 py-2 text-xs text-muted">
          {t("Position and salary auto-populate from the offer and job requisition — leave them as-is unless you need to override.")}
        </p>
        <div className="grid grid-cols-2 gap-2">
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
              {t("Employee Number")} <span className="text-error">*</span>
            </label>
            <input
              type="text"
              value={hire.employeeNumber}
              onChange={(e) => setHire((p) => ({ ...p, employeeNumber: e.target.value }))}
              className={inputCls}
            />
          </div>
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Hire Date")}</label>
            <input
              type="date"
              value={hire.hireDate}
              onChange={(e) => setHire((p) => ({ ...p, hireDate: e.target.value }))}
              className={inputCls}
            />
          </div>
        </div>
        <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Position (vacant)")}</label>
        <select
          value={hire.positionId}
          onChange={(e) => setHire((p) => ({ ...p, positionId: e.target.value }))}
          className={inputCls}
        >
          <option value="">{t("Auto — from the vacancy's role")}</option>
          {(vacantPositions?.data ?? []).map((p) => (
            <option key={p.id} value={p.id}>
              {p.code} — {p.positionClassTitle ?? ""}
            </option>
          ))}
        </select>
        <div className="grid grid-cols-2 gap-2">
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Nature")}</label>
            <select
              value={hire.employmentNature}
              onChange={(e) => setHire((p) => ({ ...p, employmentNature: e.target.value }))}
              className={inputCls}
            >
              {employmentNatureOptions.map((o) => (
                <option key={o.id} value={o.id}>{o.name}</option>
              ))}
            </select>
          </div>
          {hire.employmentNature === "Contract" ? (
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                {t("Contract (Months)")} <span className="text-error">*</span>
              </label>
              <input
                type="text"
                value={hire.contractPeriod}
                onChange={(e) => setHire((p) => ({ ...p, contractPeriod: e.target.value }))}
                className={inputCls}
              />
            </div>
          ) : (
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Salary")}</label>
              <input
                type="text"
                value={hire.salary}
                onChange={(e) => setHire((p) => ({ ...p, salary: e.target.value }))}
                className={inputCls}
              />
            </div>
          )}
        </div>
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={hire.isProbation}
            onChange={(e) => setHire((p) => ({ ...p, isProbation: e.target.checked }))}
          />
          {t("Start probation tracking")}
        </label>
        {hire.isProbation && (
          <div>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
              {t("Probation End Date")} <span className="text-error">*</span>
            </label>
            <input
              type="date"
              value={hire.probationEndDate}
              onChange={(e) => setHire((p) => ({ ...p, probationEndDate: e.target.value }))}
              className={inputCls}
            />
          </div>
        )}
        {error && <p className="text-xs text-error">{error}</p>}
      </div>
    </Modal>
  );
}

/**
 * "Hire Employee" — strictly the fully qualified, ranked applicants of open vacancies:
 * the top-N eligible per vacancy first, then the waitlist (which slides up automatically
 * when a higher-ranked candidate declines the offer).
 */
function HireEmployee() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [hireFor, setHireFor] = useState<HireQueueRowModel | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const { data: rows, isLoading, refetch, isFetching } = useQuery({
    queryKey: ["hireQueue"],
    queryFn: getHireQueue,
  });

  const byRequisition = useMemo(() => {
    const groups = new Map<string, HireQueueRowModel[]>();
    for (const r of rows ?? []) {
      const list = groups.get(r.requisitionId) ?? [];
      list.push(r);
      groups.set(r.requisitionId, list);
    }
    return [...groups.values()];
  }, [rows]);

  const refresh = () => {
    refetch();
    queryClient.invalidateQueries({ queryKey: ["jobApplications"] });
    queryClient.invalidateQueries({ queryKey: ["candidates"] });
    queryClient.invalidateQueries({ queryKey: ["employees"] });
  };

  return (
    <div className="m-1 flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="flex flex-wrap items-center gap-2 border-b border-border px-3 py-2">
        <h1 className="flex items-center gap-2 text-sm font-semibold text-foreground">
          <Award size={16} className="text-primary" />
          {t("Hire Employee")}
          <span className="text-xs font-normal text-muted">
            — {t("fully qualified, ranked applicants only")}
          </span>
        </h1>
        <button
          type="button"
          onClick={() => refetch()}
          className="ml-auto inline-flex items-center gap-1.5 rounded-md border border-border px-2.5 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary"
        >
          <RefreshCw size={13} className={isFetching ? "animate-spin" : ""} /> {t("Refresh")}
        </button>
      </div>

      <div className="min-h-0 flex-1 space-y-3 overflow-auto p-3">
        {isLoading && <Loading />}
        {message && (
          <p className="rounded-md border border-success/30 bg-success/10 px-3 py-2 text-sm text-success">{message}</p>
        )}
        {!isLoading && byRequisition.length === 0 && (
          <p className="py-10 text-center text-sm text-muted">
            {t("No hire-ready applicants — rank candidates on their vacancies first (score against the weighted criteria and move them to Selected).")}
          </p>
        )}

        {byRequisition.map((group) => {
          const head = group[0];
          return (
            <div key={head.requisitionId} className="rounded-lg border border-border">
              <div className="flex flex-wrap items-center gap-2 border-b border-border bg-secondary/40 px-3 py-2">
                <span className="text-sm font-semibold text-foreground">{head.requisitionTitle}</span>
                <span className="text-xs text-muted">{head.requisitionNumber}</span>
                <span className="ml-auto rounded bg-primary/10 px-2 py-0.5 text-xs font-semibold text-primary">
                  {head.hiredCount}/{head.numberOfPositions} {t("hired")}
                </span>
              </div>
              <table className="w-full text-[13px]">
                <thead>
                  <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                    <th className="px-3 py-2 font-semibold">{t("Rank")}</th>
                    <th className="px-3 py-2 font-semibold">{t("Candidate")}</th>
                    <th className="px-3 py-2 text-right font-semibold">{t("Score")}</th>
                    <th className="px-3 py-2 font-semibold">{t("Status")}</th>
                    <th className="px-3 py-2 font-semibold">{t("Compliance")}</th>
                    <th className="px-3 py-2 text-right font-semibold">{t("Action")}</th>
                  </tr>
                </thead>
                <tbody>
                  {group.map((r) => (
                    <tr key={r.applicationId} className="border-b border-border/60">
                      <td className="px-3 py-2">
                        {r.rank ? (
                          <span
                            className={`inline-flex h-7 min-w-7 items-center justify-center rounded-full px-1.5 text-xs font-bold tabular-nums ${
                              r.rank === 1
                                ? "bg-warning/20 text-warning"
                                : r.rank <= 3
                                  ? "bg-primary/10 text-primary"
                                  : "bg-secondary text-muted"
                            }`}
                          >
                            {ordinal(r.rank)}
                          </span>
                        ) : (
                          <span className="text-xs text-muted">{t("unranked")}</span>
                        )}
                      </td>
                      <td className="px-3 py-2">
                        <span className="block font-medium text-foreground">{r.candidateName}</span>
                        <span className="block text-xs text-muted">
                          {r.candidateNumber} · {t(r.stage)}
                          {r.latestOfferStatus && ` · ${t("offer")} ${t(r.latestOfferStatus)}`}
                        </span>
                      </td>
                      <td className="px-3 py-2 text-right font-bold tabular-nums text-primary">
                        {r.totalScore ?? "—"}
                      </td>
                      <td className="px-3 py-2">
                        <span
                          className={`rounded px-2 py-0.5 text-xs font-semibold ${
                            r.hireEligibility === "Eligible"
                              ? "bg-success/15 text-success"
                              : "bg-warning/15 text-warning"
                          }`}
                        >
                          {r.hireEligibility === "Eligible" ? t("Eligible to Hire") : t("Waitlisted")}
                        </span>
                      </td>
                      <td className="px-3 py-2">
                        <span
                          className={`text-xs font-medium ${r.complianceComplete ? "text-success" : "text-warning"}`}
                          title={r.missingComplianceDocuments.join(", ") || undefined}
                        >
                          {r.complianceComplete ? t("Complete") : t("Incomplete")}
                        </span>
                      </td>
                      <td className="px-3 py-2 text-right">
                        <button
                          type="button"
                          disabled={!r.canHire}
                          title={r.blockedReason ? t(r.blockedReason) : t("Hire as Employee")}
                          onClick={() => setHireFor(r)}
                          className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-40"
                        >
                          <UserCheck size={13} /> {t("Hire")}
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          );
        })}
      </div>

      {hireFor && (
        <HireModal
          row={hireFor}
          onClose={() => setHireFor(null)}
          onDone={() => {
            setMessage(
              t("Hired {{name}} — employee record created with the candidate's person and documents.", {
                name: hireFor.candidateName,
              }),
            );
            setHireFor(null);
            refresh();
          }}
        />
      )}
    </div>
  );
}

export default HireEmployee;
