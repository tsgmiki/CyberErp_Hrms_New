"use client";
import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  Plus,
  Trash2,
  Scale,
  UserRound,
  Building2,
  UserPlus,
  X,
  AlertTriangle,
  CheckCircle2,
  SlidersHorizontal,
} from "lucide-react";
import Modal from "@/components/common/modal";
import getAllEmployee from "@/services/admin/employee/getAll";
import type { ScreeningCriterionModel, CriterionEvaluatorModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import { criterionStageOptions } from "@/constants/orgStructure";

const lookupParam = { ...parameterInitialData, take: 200 };
const fieldCls =
  "h-9 w-full rounded-md border border-border bg-background px-2.5 text-sm text-foreground " +
  "focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/40 disabled:opacity-60";

const EVALUATOR_KIND_ICON: Record<string, typeof UserRound> = {
  Employee: UserRound,
  ExternalPerson: UserPlus,
  Organization: Building2,
};

/** Inline editor for one criterion's evaluator panel (any number, mixed kinds). */
function EvaluatorEditor({
  evaluators,
  readOnly,
  employees,
  onChange,
}: {
  evaluators: CriterionEvaluatorModel[];
  readOnly: boolean;
  employees: { id?: string; label: string }[];
  onChange: (next: CriterionEvaluatorModel[]) => void;
}) {
  const { t } = useTranslation();
  const [draftType, setDraftType] = useState("Employee");
  const [draftEmployee, setDraftEmployee] = useState("");
  const [draftName, setDraftName] = useState("");

  const add = () => {
    if (draftType === "Employee") {
      if (!draftEmployee) return;
      if (evaluators.some((e) => e.employeeId === draftEmployee)) return; // no duplicates
      const label = employees.find((e) => e.id === draftEmployee)?.label;
      onChange([...evaluators, { evaluatorType: "Employee", employeeId: draftEmployee, name: label }]);
      setDraftEmployee("");
    } else {
      if (!draftName.trim()) return;
      onChange([...evaluators, { evaluatorType: draftType, name: draftName.trim() }]);
      setDraftName("");
    }
  };

  return (
    <div className="space-y-1.5">
      {/* Assigned evaluators as removable chips */}
      <div className="flex min-h-7 flex-wrap items-center gap-1">
        {evaluators.length === 0 && (
          <span className="text-[11px] italic text-muted">{t("Scored by HR — no evaluators assigned")}</span>
        )}
        {evaluators.map((e, i) => {
          const Icon = EVALUATOR_KIND_ICON[e.evaluatorType] ?? UserRound;
          return (
            <span
              key={i}
              title={t(e.evaluatorType)}
              className="inline-flex items-center gap-1 rounded-full border border-border bg-secondary/60 py-0.5 pl-2 pr-1 text-[11px] font-medium text-foreground"
            >
              <Icon size={11} className="shrink-0 text-primary" />
              <span className="max-w-36 truncate">{e.name}</span>
              {!readOnly && (
                <button
                  type="button"
                  aria-label={t("Remove evaluator")}
                  onClick={() => onChange(evaluators.filter((_, idx) => idx !== i))}
                  className="rounded-full p-0.5 text-muted hover:bg-error/15 hover:text-error"
                >
                  <X size={11} />
                </button>
              )}
            </span>
          );
        })}
      </div>

      {/* Inline add row */}
      {!readOnly && (
        <div className="flex items-center gap-1">
          <select
            value={draftType}
            onChange={(e) => setDraftType(e.target.value)}
            className="h-7 shrink-0 rounded-md border border-border bg-background px-1.5 text-[11px] text-foreground"
          >
            <option value="Employee">{t("Employee")}</option>
            <option value="ExternalPerson">{t("External Person")}</option>
            <option value="Organization">{t("Organization")}</option>
          </select>
          {draftType === "Employee" ? (
            <select
              value={draftEmployee}
              onChange={(e) => setDraftEmployee(e.target.value)}
              className="h-7 w-full min-w-0 rounded-md border border-border bg-background px-1.5 text-[11px] text-foreground"
            >
              <option value="">{t("Select employee…")}</option>
              {employees.map((e) => (
                <option key={e.id} value={e.id}>{e.label}</option>
              ))}
            </select>
          ) : (
            <input
              type="text"
              value={draftName}
              onChange={(e) => setDraftName(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), add())}
              placeholder={draftType === "Organization" ? t("Organization name…") : t("Person's name…")}
              className="h-7 w-full min-w-0 rounded-md border border-border bg-background px-1.5 text-[11px] text-foreground"
            />
          )}
          <button
            type="button"
            onClick={add}
            title={t("Add evaluator")}
            className="inline-flex h-7 shrink-0 items-center gap-1 rounded-md border border-primary/40 bg-primary/10 px-2 text-[11px] font-semibold text-primary hover:bg-primary/20"
          >
            <Plus size={11} /> {t("Add")}
          </button>
        </div>
      )}
    </div>
  );
}

/**
 * Screening-criteria designer. Weights are PERCENTAGES of the final ranking score — the grid
 * enforces a total of exactly 100% before it can be applied. Each criterion may be scoped to one
 * recruitment level (or apply globally) and may carry ANY number of evaluators — internal
 * employees, external persons, or organizations.
 */
function CriteriaModal({
  initial,
  readOnly,
  onClose,
  onApply,
}: {
  initial: ScreeningCriterionModel[];
  readOnly: boolean;
  onClose: () => void;
  onApply: (criteria: ScreeningCriterionModel[]) => void;
}) {
  const { t } = useTranslation();
  const [rows, setRows] = useState<ScreeningCriterionModel[]>(
    initial.map((c) => ({ ...c, evaluators: (c.evaluators ?? []).map((e) => ({ ...e })) })),
  );

  const { data: employeesRaw } = useQuery({
    queryKey: ["employees", lookupParam],
    queryFn: () => getAllEmployee(lookupParam),
  });
  const employees = useMemo(
    () =>
      (employeesRaw?.data ?? []).map((e) => ({
        id: e.id,
        label: e.fullName ?? e.employeeNumber ?? "",
      })),
    [employeesRaw],
  );

  const set = (i: number, patch: Partial<ScreeningCriterionModel>) =>
    setRows((p) => p.map((c, idx) => (idx === i ? { ...c, ...patch } : c)));

  const total = rows.reduce((s, c) => s + (Number(c.weight) || 0), 0);
  const totalOk = rows.length === 0 || total === 100;
  const namesOk = rows.every((c) => (c.name ?? "").trim() !== "");
  const canApply = totalOk && namesOk;

  /** Splits 100% evenly across the rows (remainder to the first). */
  const distributeEvenly = () => {
    if (rows.length === 0) return;
    const base = Math.floor(100 / rows.length);
    setRows((p) => p.map((c, i) => ({ ...c, weight: base + (i === 0 ? 100 - base * p.length : 0) })));
  };

  return (
    <Modal
      visible
      size="xl"
      title={t("Screening Criteria")}
      description={t("Define what applicants are evaluated on, at which recruitment level, by whom — and how much each criterion weighs in the final ranking.")}
      onClose={onClose}
      footer={
        <>
          {/* Weight budget — the 100% gate */}
          <div className="mr-auto flex items-center gap-3">
            <div className="w-40">
              <div className="mb-0.5 flex items-center justify-between text-[11px] font-semibold">
                <span className="text-muted">{t("Weight total")}</span>
                <span className={totalOk ? "text-success" : "text-error"}>{total}%</span>
              </div>
              <div className="h-1.5 overflow-hidden rounded-full bg-border">
                <div
                  className={`h-full transition-all ${total === 100 ? "bg-success" : total < 100 ? "bg-warning" : "bg-error"}`}
                  style={{ width: `${Math.min(100, total)}%` }}
                />
              </div>
            </div>
            {rows.length > 0 && (
              totalOk ? (
                <span className="inline-flex items-center gap-1 text-[11px] font-medium text-success">
                  <CheckCircle2 size={12} /> {t("Balanced")}
                </span>
              ) : (
                <span className="inline-flex items-center gap-1 text-[11px] font-medium text-error">
                  <AlertTriangle size={12} />
                  {total < 100 ? t("{{n}}% unassigned", { n: 100 - total }) : t("{{n}}% over", { n: total - 100 })}
                </span>
              )
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-border px-3.5 py-1.5 text-sm text-foreground hover:bg-secondary"
          >
            {t("Cancel")}
          </button>
          {!readOnly && (
            <button
              type="button"
              disabled={!canApply}
              onClick={() => onApply(rows)}
              title={!totalOk ? t("Weights must total exactly 100%") : !namesOk ? t("Every criterion needs a name") : undefined}
              className="rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent disabled:cursor-not-allowed disabled:opacity-50"
            >
              {t("Apply Criteria")}
            </button>
          )}
        </>
      }
    >
      {/* Toolbar */}
      {!readOnly && (
        <div className="mb-3 flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() =>
              setRows((p) => [
                ...p,
                { name: "", isMandatory: false, weight: Math.max(0, 100 - total), evaluators: [] },
              ])
            }
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus size={13} /> {t("Add Criterion")}
          </button>
          <button
            type="button"
            disabled={rows.length === 0}
            onClick={distributeEvenly}
            title={t("Split 100% evenly across all criteria")}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary disabled:opacity-40"
          >
            <SlidersHorizontal size={13} /> {t("Distribute Evenly")}
          </button>
          <span className="ml-auto text-[11px] text-muted">
            {t("New criteria pre-fill with the unassigned weight")}
          </span>
        </div>
      )}

      {/* Empty state — with a one-click starter template (SAP-style guided setup) */}
      {rows.length === 0 && (
        <div className="flex flex-col items-center gap-2 rounded-lg border border-dashed border-border py-10 text-center">
          <Scale size={28} className="text-muted" />
          <p className="text-sm font-medium text-foreground">{t("No screening criteria defined")}</p>
          <p className="max-w-md text-xs text-muted">
            {t("Applicants are screened on the job specification only. Add weighted criteria to enable scored ranking and the hire-eligibility window.")}
          </p>
          {!readOnly && (
            <button
              type="button"
              onClick={() =>
                setRows([
                  { name: t("Written Exam"), isMandatory: true, weight: 50, appliesAtStage: "Screening", evaluators: [] },
                  { name: t("Interview"), isMandatory: false, weight: 30, appliesAtStage: "Interview", evaluators: [] },
                  { name: t("Document Review"), isMandatory: false, weight: 20, appliesAtStage: "Screening", evaluators: [] },
                ])
              }
              className="mt-1 inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-semibold text-primary hover:bg-primary/20"
            >
              <SlidersHorizontal size={13} /> {t("Load Standard Template (50 / 30 / 20)")}
            </button>
          )}
        </div>
      )}

      {/* Criteria cards */}
      <div className="space-y-2">
        {rows.map((c, i) => (
          <div
            key={i}
            className="rounded-lg border border-border bg-card p-3 transition-colors hover:border-primary/40"
          >
            {/* Row 1: name + weight + level + mandatory + delete */}
            <div className="flex flex-wrap items-end gap-2">
              <div className="min-w-52 flex-1">
                <label className="mb-0.5 block text-[10px] font-semibold uppercase tracking-wider text-muted">
                  {t("Criterion")} <span className="text-error">*</span>
                </label>
                <input
                  type="text"
                  disabled={readOnly}
                  value={c.name}
                  onChange={(e) => set(i, { name: e.target.value })}
                  placeholder={t("e.g. Written Exam, Interview, Document Review…")}
                  className={fieldCls}
                />
              </div>
              <div className="w-24">
                <label className="mb-0.5 block text-[10px] font-semibold uppercase tracking-wider text-muted">
                  {t("Weight")}
                </label>
                <div className="relative">
                  <input
                    type="text"
                    inputMode="numeric"
                    disabled={readOnly}
                    value={c.weight}
                    onChange={(e) => set(i, { weight: Number(e.target.value.replace(/[^0-9]/g, "")) || 0 })}
                    className={`${fieldCls} pr-6 text-right tabular-nums`}
                  />
                  <span className="pointer-events-none absolute inset-y-0 right-2 flex items-center text-xs text-muted">%</span>
                </div>
              </div>
              <div className="w-36">
                <label className="mb-0.5 block text-[10px] font-semibold uppercase tracking-wider text-muted">
                  {t("Level")}
                </label>
                <select
                  disabled={readOnly}
                  value={c.appliesAtStage ?? ""}
                  onChange={(e) => set(i, { appliesAtStage: e.target.value || undefined })}
                  className={fieldCls}
                >
                  {criterionStageOptions.map((o) => (
                    <option key={o.id} value={o.id}>{t(o.name)}</option>
                  ))}
                </select>
              </div>
              <label className="mb-2 flex shrink-0 cursor-pointer items-center gap-1.5 text-xs text-foreground">
                <input
                  type="checkbox"
                  disabled={readOnly}
                  checked={c.isMandatory === true}
                  onChange={(e) => set(i, { isMandatory: e.target.checked })}
                />
                {t("Mandatory")}
                <span className="text-[10px] text-muted">({t("<50 screens out")})</span>
              </label>
              {!readOnly && (
                <button
                  type="button"
                  title={t("Remove criterion")}
                  onClick={() => setRows((p) => p.filter((_, idx) => idx !== i))}
                  className="mb-1 shrink-0 rounded-md p-1.5 text-muted hover:bg-error/10 hover:text-error"
                >
                  <Trash2 size={15} />
                </button>
              )}
            </div>

            {/* Row 2: evaluator panel */}
            <div className="mt-2 border-t border-border/60 pt-2">
              <label className="mb-1 block text-[10px] font-semibold uppercase tracking-wider text-muted">
                {t("Evaluators")}
              </label>
              <EvaluatorEditor
                evaluators={c.evaluators ?? []}
                readOnly={readOnly}
                employees={employees}
                onChange={(next) => set(i, { evaluators: next })}
              />
            </div>
          </div>
        ))}
      </div>
    </Modal>
  );
}

export default CriteriaModal;
