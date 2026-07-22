"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, ArrowLeftRight, ClipboardCheck, CheckCircle2, AlertTriangle } from "lucide-react";
import type { EmployeeMovementModel } from "@/models";
import { getTransferRequest, saveTransferRequest, assessTransfer } from "@/services/admin/transferRequest";
import getAllPosition from "@/services/admin/position/getAll";
import EmployeePicker from "@/components/common/employeePicker";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { transferKindOptions } from "@/constants/orgStructure";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";
const LABEL = "block text-xs font-medium text-muted mb-1";

const NEW_DEFAULTS: EmployeeMovementModel = { movementType: "Transfer", transferKind: "Department" };

function TransferRequestForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<EmployeeMovementModel>({ ...NEW_DEFAULTS });
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["transferRequest", id],
    queryFn: () => getTransferRequest(id),
    enabled: id !== "",
  });

  // Open (vacant) positions only — the movement targets an unoccupied seat.
  const [posParam] = useState({ ...parameterInitialData, take: 300, isVacant: true });
  const { data: positions } = useQuery({ queryKey: ["positions", posParam], queryFn: () => getAllPosition(posParam) });

  useEffect(() => {
    if (record) setMeta({ ...record, effectiveDate: record.effectiveDate?.slice(0, 10) });
  }, [record]);

  const set = (name: keyof EmployeeMovementModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));
  const editable = !id || meta.status === "Pending";

  // HC173 — the assessment runs live once the employee + target are chosen (read-only, advisory).
  const canAssess = !!meta.employeeId && !!meta.toPositionId;
  const { data: assess } = useQuery({
    queryKey: ["transferAssess", meta.employeeId, meta.toPositionId],
    queryFn: () => assessTransfer(meta.employeeId!, meta.toPositionId!),
    enabled: canAssess,
    staleTime: 30_000,
  });

  const submit = async () => {
    setIsSaving(true);
    const result = await saveTransferRequest({
      id: meta.id,
      employeeId: meta.employeeId,
      movementType: "Transfer",
      transferKind: meta.transferKind || undefined,
      effectiveDate: meta.effectiveDate,
      toPositionId: meta.toPositionId,
      reason: meta.reason,
      remark: meta.remark,
      relocationExpense: meta.relocationExpense != null && String(meta.relocationExpense) !== "" ? Number(meta.relocationExpense) : undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["transferRequests"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = editable && !!meta.employeeId && !!meta.toPositionId && !!meta.effectiveDate;

  return (
    <div className="space-y-4 text-foreground">
      {id && meta.status && meta.status !== "Pending" && (
        <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
          {t("This request is")} <span className="font-semibold text-foreground">{meta.status}</span> — {t("it can no longer be edited here.")}
        </p>
      )}
      <EntityFormTabs
        hasId={!!id}
        tabs={[
          {
            key: "request",
            label: "Request",
            Icon: ArrowLeftRight,
            description: "Who moves, where to, when and why (HC170/171)",
            keepMounted: true,
            content: (
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div>
                  <label className={LABEL}>{t("Employee")} *</label>
                  {/* Role-scoped: HR = all, manager = unit subtree, employee = locked to self (HC170). */}
                  <EmployeePicker
                    value={meta.employeeId}
                    displayValue={meta.employeeName}
                    disabled={!editable || !!id}
                    onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
                  />
                </div>
                <div>
                  <label className={LABEL}>{t("Transfer Kind")} *</label>
                  <select className={INPUT} disabled={!editable} value={meta.transferKind ?? ""} onChange={(e) => set("transferKind", e.target.value)}>
                    {transferKindOptions.map((o) => (
                      <option key={o.id} value={o.id}>{t(o.name)}</option>
                    ))}
                  </select>
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Target Position (vacant)")} *</label>
                  <select className={INPUT} disabled={!editable} value={meta.toPositionId ?? ""} onChange={(e) => set("toPositionId", e.target.value)}>
                    <option value="">{t("Select a vacant position")}</option>
                    {meta.toPositionId && !(positions?.data ?? []).some((p) => p.id === meta.toPositionId) && (
                      <option value={meta.toPositionId}>{meta.toPositionName ?? t("Current target")}</option>
                    )}
                    {(positions?.data ?? []).map((p) => (
                      <option key={p.id} value={p.id}>{p.code} — {p.positionClassTitle ?? ""} {p.organizationUnitName ? `(${p.organizationUnitName})` : ""}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={LABEL}>{t("Preferred Start Date")} *</label>
                  <input type="date" className={INPUT} disabled={!editable} value={meta.effectiveDate ?? ""} onChange={(e) => set("effectiveDate", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("Relocation Expense (est.)")}</label>
                  <input type="number" step="any" min="0" className={INPUT} disabled={!editable} value={meta.relocationExpense ?? ""} onChange={(e) => set("relocationExpense", e.target.value)} />
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Justification")} *</label>
                  <textarea rows={2} className={INPUT} disabled={!editable} value={meta.reason ?? ""} onChange={(e) => set("reason", e.target.value)} placeholder={t("Why this transfer is requested") ?? ""} />
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Remark")}</label>
                  <input className={INPUT} disabled={!editable} value={meta.remark ?? ""} onChange={(e) => set("remark", e.target.value)} />
                </div>
              </div>
            ),
          },
          {
            key: "assessment",
            label: "Assessment",
            Icon: ClipboardCheck,
            description: "Eligibility & impact — advisory, computed live (HC173)",
            keepMounted: true,
            content: !canAssess ? (
              <p className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-center text-xs text-muted">
                {t("Choose the employee and the target position to assess eligibility and impact.")}
              </p>
            ) : !assess ? (
              <Loading />
            ) : (
              <div className="space-y-3 text-sm">
                {/* Eligibility snapshot */}
                <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
                  <div className="rounded-lg border border-border bg-card p-2.5">
                    <p className="text-[11px] uppercase tracking-wide text-muted">{t("Performance")}</p>
                    <p className="text-base font-semibold tabular-nums">{assess.performanceScorePercent != null ? `${assess.performanceScorePercent}%` : "—"}</p>
                    <p className="truncate text-[11px] text-muted">{assess.performanceCycleName ?? t("no appraisal")}</p>
                  </div>
                  <div className="rounded-lg border border-border bg-card p-2.5">
                    <p className="text-[11px] uppercase tracking-wide text-muted">{t("In current role")}</p>
                    <p className="text-base font-semibold tabular-nums">{assess.tenureMonthsInCurrentRole} {t("mo")}</p>
                    <p className="text-[11px] text-muted">{t("total")} {assess.tenureMonthsTotal} {t("mo")}</p>
                  </div>
                  <div className="rounded-lg border border-border bg-card p-2.5">
                    <p className="text-[11px] uppercase tracking-wide text-muted">{t("Skill gap")}</p>
                    <p className="text-base font-semibold tabular-nums">{assess.skillGapCount}/{assess.targetCompetencies.length}</p>
                    <p className="text-[11px] text-muted">{t("target competencies uncovered")}</p>
                  </div>
                  <div className="rounded-lg border border-border bg-card p-2.5">
                    <p className="text-[11px] uppercase tracking-wide text-muted">{t("Budget")}</p>
                    <p className="text-base font-semibold">{assess.salaryUnchanged ? t("Pay unchanged") : "—"}</p>
                    <p className="text-[11px] text-muted">{t("relocation")}: {meta.relocationExpense || 0}</p>
                  </div>
                </div>

                {/* Skill gap detail */}
                {assess.targetCompetencies.length > 0 && (
                  <div>
                    <p className="mb-1.5 text-xs font-semibold uppercase tracking-wide text-muted">{t("Target role requirements")}</p>
                    <div className="flex flex-wrap gap-1.5">
                      {assess.targetCompetencies.map((c) => (
                        <span key={c.competencyId}
                          className={`inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs ${c.coveredByCurrentRole ? "border-success/40 bg-success/10 text-success" : "border-warning/40 bg-warning/10 text-warning"}`}>
                          {c.coveredByCurrentRole ? <CheckCircle2 size={11} /> : <AlertTriangle size={11} />} {c.name}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

                {/* Department impact */}
                <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
                  {[{ label: t("Current department"), i: assess.currentUnitImpact }, { label: t("Target department"), i: assess.targetUnitImpact }].map((x) => (
                    <div key={x.label} className="rounded-lg border border-border bg-card p-2.5 text-xs">
                      <p className="font-semibold">{x.label}: <span className="font-normal">{x.i.unitName ?? "—"}</span></p>
                      <p className="text-muted">{t("Positions")}: {x.i.totalPositions} · {t("vacant")}: {x.i.vacantPositions}</p>
                    </div>
                  ))}
                </div>

                {/* Qualifications */}
                {assess.qualifications.length > 0 && (
                  <div className="text-xs">
                    <p className="mb-1 font-semibold uppercase tracking-wide text-muted">{t("Qualifications")}</p>
                    <ul className="list-inside list-disc text-muted">
                      {assess.qualifications.map((q, i) => <li key={i}>{q}</li>)}
                    </ul>
                  </div>
                )}

                {/* Advisory flags */}
                {assess.flags.length > 0 && (
                  <div className="space-y-1">
                    {assess.flags.map((f, i) => (
                      <p key={i} className="flex items-start gap-1.5 rounded-md border border-warning/30 bg-warning/10 px-2.5 py-1.5 text-xs text-warning">
                        <AlertTriangle size={13} className="mt-0.5 shrink-0" /> {f}
                      </p>
                    ))}
                  </div>
                )}
              </div>
            ),
          },
        ]}
      />

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {editable && (
        <div className="flex items-center justify-end gap-2 border-t border-border pt-3">
          <button type="button" disabled={isSaving || !canSave} onClick={submit}
            className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">
            <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : id ? t("Save Changes") : t("Submit Transfer Request")}
          </button>
        </div>
      )}
    </div>
  );
}

export default memo(TransferRequestForm);
