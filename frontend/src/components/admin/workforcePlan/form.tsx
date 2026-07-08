"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import type { WorkforcePlanModel, WorkforcePlanLineModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  Plus,
  Trash2,
  Database,
  Lightbulb,
  Send,
  GitBranch,
  Hourglass,
} from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "../../common/loader/loader";
import {
  getWorkforcePlan,
  saveWorkforcePlan,
  populateWorkforcePlan,
  suggestSeparations,
  submitWorkforcePlan,
  createWorkforcePlanVersion,
} from "@/services/admin/workforcePlan";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import { parameterInitialData } from "@/constants/initialization";
import {
  planHorizonOptions,
  planHorizonLabel,
  planScenarioOptions,
  plannedEmploymentTypeOptions,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 200 };

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  Submitted: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Archived: "bg-info/15 text-info",
};

const emptyLine = (): WorkforcePlanLineModel => ({
  organizationUnitId: "",
  positionClassId: "",
  employmentType: "Permanent",
  periodIndex: 0,
  authorizedHeadcount: 0,
  filledCount: 0,
  vacantCount: 0,
  newHires: 0,
  replacements: 0,
  temporaryStaff: 0,
  mobilityIn: 0,
  promotions: 0,
  actingAssignments: 0,
  retirements: 0,
  resignations: 0,
  contractExpiries: 0,
  isCriticalRole: false,
  annualSalaryCost: 0,
  annualAllowances: 0,
  annualBenefits: 0,
});

/** Client-side mirror of the domain projections so totals track the grid live. */
function computeLine(l: WorkforcePlanLineModel) {
  const n = (v: unknown) => Number(v) || 0;
  const endHeadcount = Math.max(
    0,
    n(l.filledCount) -
      (n(l.retirements) + n(l.resignations) + n(l.contractExpiries)) +
      (n(l.newHires) + n(l.replacements) + n(l.temporaryStaff)) +
      (n(l.mobilityIn) + n(l.promotions) + n(l.actingAssignments)),
  );
  const gap = Math.max(0, endHeadcount - n(l.authorizedHeadcount));
  const cost = endHeadcount * (n(l.annualSalaryCost) + n(l.annualAllowances) + n(l.annualBenefits));
  return { endHeadcount, gap, cost };
}

const fmtMoney = (v: number) => v.toLocaleString(undefined, { maximumFractionDigits: 0 });

function WorkforcePlanForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<WorkforcePlanModel>({
    horizon: "Annual",
    scenario: "Baseline",
    periodCount: 1,
    totalBudget: 0,
    budgetThresholdPercent: 0,
  });
  const [lines, setLines] = useState<WorkforcePlanLineModel[]>([]);
  const [busy, setBusy] = useState(false);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [showSubmit, setShowSubmit] = useState(false);
  const [justification, setJustification] = useState("");
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["workforcePlan", id],
    queryFn: () => getWorkforcePlan(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: units } = useQuery({
    queryKey: ["organizationUnits", lookupParam],
    queryFn: () => getAllOrganizationUnit(lookupParam),
  });
  const { data: classes } = useQuery({
    queryKey: ["positionClasses", lookupParam],
    queryFn: () => getAllPositionClass(lookupParam),
  });
  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscalYears", lookupParam],
    queryFn: () => getAllFiscalYear(lookupParam),
  });

  // Approved / submitted / archived plans are immutable — create a new version to revise (HC071).
  const readOnly =
    !!record && record.status !== "Draft" && record.status !== "Rejected";

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData(record);
      setLines(record.lines ?? []);
    }
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["workforcePlans"] });
      // Stay on the plan after a create so Populate / Submit are immediately usable.
      if (!formData.id && formState.id) {
        setId(formState.id);
        queryClient.invalidateQueries({ queryKey: ["workforcePlan", formState.id] });
      } else if (formData.id) {
        queryClient.invalidateQueries({ queryKey: ["workforcePlan", formData.id] });
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
    const result = await saveWorkforcePlan({ ...formData, lines });
    setFormState(result);
    setIsLoading(false);
  };

  const setLine = (index: number, patch: Partial<WorkforcePlanLineModel>) =>
    setLines((p) => p.map((l, i) => (i === index ? { ...l, ...patch } : l)));
  const removeLine = (index: number) => setLines((p) => p.filter((_, i) => i !== index));

  const totals = useMemo(() => {
    const computed = lines.map(computeLine);
    const budget = Number(formData.totalBudget) || 0;
    const threshold = Number(formData.budgetThresholdPercent) || 0;
    const cost = computed.reduce((s, c) => s + c.cost, 0);
    const ceiling = budget * (1 + threshold / 100);
    return {
      endHeadcount: computed.reduce((s, c) => s + c.endHeadcount, 0),
      gap: computed.reduce((s, c) => s + c.gap, 0),
      cost,
      variance: budget - cost,
      excess: budget > 0 && cost > ceiling ? cost - ceiling : 0,
    };
  }, [lines, formData.totalBudget, formData.budgetThresholdPercent]);

  // Time-phased projections (HC069/HC073): per-period headcount, hiring demand, internal mobility
  // (supply), attrition (separations) and cost trend — live from the grid, mirroring the domain.
  const periodProjections = useMemo(() => {
    const n = (v: unknown) => Number(v) || 0;
    const count = Math.max(1, Number(formData.periodCount) || 1);
    return Array.from({ length: count }, (_, p) => {
      const periodLines = lines.filter((l) => Number(l.periodIndex) === p);
      const computed = periodLines.map(computeLine);
      return {
        period: p + 1,
        endHeadcount: computed.reduce((s, c) => s + c.endHeadcount, 0),
        demand: periodLines.reduce((s, l) => s + n(l.newHires) + n(l.replacements) + n(l.temporaryStaff), 0),
        supply: periodLines.reduce((s, l) => s + n(l.mobilityIn) + n(l.promotions) + n(l.actingAssignments), 0),
        separations: periodLines.reduce(
          (s, l) => s + n(l.retirements) + n(l.resignations) + n(l.contractExpiries), 0),
        cost: computed.reduce((s, c) => s + c.cost, 0),
      };
    });
  }, [lines, formData.periodCount]);

  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["workforcePlan", id] });
    queryClient.invalidateQueries({ queryKey: ["workforcePlans"] });
  }, [queryClient, id]);

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    setActionMessage(res.message);
    refresh();
    return res;
  };

  /** Rebuilds the grid from the live establishment (HC055) — server-side, then refetch. */
  const populate = () => run(() => populateWorkforcePlan(id));

  /** Merges suggested retirement counts into matching period-1 Permanent lines (HC060). */
  const applySuggestions = async () => {
    setBusy(true);
    try {
      const suggestions = await suggestSeparations(id);
      let applied = 0;
      setLines((p) =>
        p.map((l) => {
          const s = suggestions.find(
            (x) =>
              x.organizationUnitId === l.organizationUnitId &&
              x.positionClassId === l.positionClassId &&
              Number(l.periodIndex) === 0 &&
              l.employmentType === "Permanent",
          );
          if (!s) return l;
          applied++;
          return { ...l, retirements: s.retirements };
        }),
      );
      setActionMessage(
        applied > 0
          ? t("Applied retirement forecasts to {{count}} line(s) — save to keep them.", { count: applied })
          : t("No matching lines for the retirement forecast."),
      );
    } finally {
      setBusy(false);
    }
  };

  const doSubmit = async () => {
    const res = await run(() => submitWorkforcePlan(id, justification));
    if (res.ok) setShowSubmit(false);
    queryClient.invalidateQueries({ queryKey: ["workflows"] });
    queryClient.invalidateQueries({ queryKey: ["workflowStats"] });
  };

  const newVersion = async () => {
    setBusy(true);
    const res = await createWorkforcePlanVersion(id);
    setBusy(false);
    setActionMessage(res.message);
    if (res.ok && res.id) {
      refresh();
      setId(res.id);
    }
  };

  const inputCls =
    "h-7 w-14 rounded border border-border bg-background px-1 text-right text-xs text-foreground disabled:opacity-60";
  const selectCls =
    "h-7 rounded border border-border bg-background px-1 text-xs text-foreground disabled:opacity-60";
  const groupTh = "border-b border-border px-1 py-1 text-center text-[10px] font-bold uppercase tracking-wide text-muted";
  const th = "border-b border-border px-1 py-1 text-[10px] font-semibold uppercase text-table-header whitespace-nowrap";

  return (
    <div className="text-foreground">
      {pending && <Loading />}

      {/* Status strip */}
      {record && (
        <div className="mb-2 flex flex-wrap items-center gap-2 text-sm">
          <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[record.status ?? ""] ?? ""}`}>
            {t(record.status ?? "")}
          </span>
          <span className="rounded bg-secondary px-2 py-0.5 text-xs">v{record.version}</span>
          <span className="rounded bg-secondary px-2 py-0.5 text-xs">{t(record.scenario ?? "")}</span>
          {record.awaitingWorkflow && (
            <span className="flex items-center gap-1 rounded border border-info/30 bg-info/10 px-2 py-0.5 text-xs text-info">
              <Hourglass size={12} /> {t("Awaiting workflow approval")}
            </span>
          )}
          {readOnly && (
            <button
              type="button"
              disabled={busy}
              onClick={newVersion}
              className="ml-auto inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary hover:bg-primary/20 disabled:opacity-50"
            >
              <GitBranch size={14} /> {t("Create New Version")}
            </button>
          )}
        </div>
      )}

      {/* Plan header */}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 3,
          submitHandler,
          labelWidth: "w-[38%]",
          isPending: isLoading,
          // An unknown placement renders no submit button — read-only plans cannot be saved.
          SubmitButton: (readOnly ? "none" : "top") as "top",
          submitBtnTitle: "Save Plan",
          components: [
            {
              name: "name", label: "Plan Name", required: true, type: "text",
              value: formData.name, onChange: changeHandler, disabled: readOnly,
              error: formState?.zodErrors?.name, placeholder: "e.g. FY2026 Baseline Plan",
            },
            {
              name: "scenario", label: "Scenario", type: "dropDown", onSelect: selectHandler,
              value: formData.scenario, displayValue: formData.scenario, disabled: readOnly,
              data: planScenarioOptions as never,
            },
            {
              name: "horizon", label: "Horizon", type: "dropDown", onSelect: selectHandler,
              value: formData.horizon, displayValue: planHorizonLabel(formData.horizon), disabled: readOnly,
              data: planHorizonOptions as never,
            },
            {
              name: "startFiscalYearId", label: "Start Fiscal Year", required: true, type: "dropDown",
              onSelect: selectHandler, value: formData.startFiscalYearId, disabled: readOnly,
              displayValue: formData.startFiscalYearName,
              error: formState?.zodErrors?.startFiscalYearId,
              data: (fiscalYears?.data ?? []).map((f) => ({ id: f.id, name: f.name })) as never,
            },
            {
              name: "periodCount", label: "Periods (Years)", type: "text",
              value: formData.periodCount, onChange: changeHandler, disabled: readOnly, placeholder: "1",
            },
            {
              name: "organizationUnitId", label: "Scope (Unit)", type: "dropDown", onSelect: selectHandler,
              value: formData.organizationUnitId, displayValue: formData.organizationUnitName,
              disabled: readOnly, placeholder: "Organization-wide",
              data: (units?.data ?? []).map((u) => ({ id: u.id, name: u.name })) as never,
            },
            {
              name: "totalBudget", label: "Approved Budget", type: "text",
              value: formData.totalBudget, onChange: changeHandler, disabled: readOnly, placeholder: "0",
            },
            {
              name: "budgetThresholdPercent", label: "Escalation Threshold %", type: "text",
              value: formData.budgetThresholdPercent, onChange: changeHandler, disabled: readOnly, placeholder: "0",
            },
            {
              name: "description", label: "Description", type: "text",
              value: formData.description, onChange: changeHandler, disabled: readOnly,
            },
          ],
        }}
      />

      {/* Cost & budget summary (HC064–HC066) */}
      <div className="mt-3 grid grid-cols-2 gap-3 md:grid-cols-5">
        {[
          { label: t("Projected End Headcount"), value: totals.endHeadcount.toLocaleString() },
          { label: t("Headcount Gap"), value: totals.gap.toLocaleString() },
          { label: t("Projected Cost"), value: fmtMoney(totals.cost) },
          {
            label: t("Budget Variance"),
            value: fmtMoney(totals.variance),
            tone: totals.variance < 0 ? "text-error" : "text-success",
          },
          {
            label: t("Beyond Threshold"),
            value: fmtMoney(totals.excess),
            tone: totals.excess > 0 ? "text-error" : "text-muted",
          },
        ].map((c) => (
          <div key={c.label} className="rounded-lg border border-border bg-card px-3 py-2">
            <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{c.label}</p>
            <p className={`text-lg font-bold tabular-nums ${c.tone ?? "text-foreground"}`}>{c.value}</p>
          </div>
        ))}
      </div>
      {totals.excess > 0 && (
        <p className="mt-1 text-xs text-error">
          {t("The projected cost exceeds the budget threshold — submission will require an escalation justification.")}
        </p>
      )}

      {/* Time-phased projections (HC069/HC073): headcount, demand, mobility, attrition, cost trend */}
      {lines.length > 0 && (
        <div className="mt-3 overflow-x-auto rounded-lg border border-border bg-card">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-border text-left text-[10px] font-semibold uppercase text-table-header">
                <th className="px-3 py-1.5">{t("Projection")}</th>
                {periodProjections.map((p) => (
                  <th key={p.period} className="px-3 py-1.5 text-right">
                    {t("Year")} {p.period}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {[
                { label: t("End Headcount"), get: (p: (typeof periodProjections)[number]) => p.endHeadcount.toLocaleString() },
                { label: t("Hiring Demand"), get: (p: (typeof periodProjections)[number]) => p.demand.toLocaleString() },
                { label: t("Internal Mobility (Supply)"), get: (p: (typeof periodProjections)[number]) => p.supply.toLocaleString() },
                { label: t("Attrition (Separations)"), get: (p: (typeof periodProjections)[number]) => p.separations.toLocaleString() },
                { label: t("Projected Cost"), get: (p: (typeof periodProjections)[number]) => fmtMoney(p.cost) },
              ].map((row) => (
                <tr key={row.label} className="border-b border-border/50">
                  <td className="px-3 py-1.5 font-medium text-muted">{row.label}</td>
                  {periodProjections.map((p) => (
                    <td key={p.period} className="px-3 py-1.5 text-right tabular-nums text-foreground">
                      {row.get(p)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Actions */}
      <div className="mt-3 flex flex-wrap items-center gap-2">
        {!readOnly && (
          <>
            <button
              type="button"
              onClick={() => setLines((p) => [...p, emptyLine()])}
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary"
            >
              <Plus size={14} /> {t("Add Line")}
            </button>
            {id && (
              <>
                <button
                  type="button"
                  disabled={busy}
                  onClick={populate}
                  title={t("Rebuild the grid from the live establishment (replaces current lines)")}
                  className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
                >
                  <Database size={14} /> {t("Populate from Establishment")}
                </button>
                <button
                  type="button"
                  disabled={busy}
                  onClick={applySuggestions}
                  title={t("Pre-fill retirements from the workforce age profile")}
                  className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
                >
                  <Lightbulb size={14} /> {t("Suggest Separations")}
                </button>
                <button
                  type="button"
                  disabled={busy || lines.length === 0}
                  onClick={() => {
                    setJustification("");
                    setShowSubmit(true);
                  }}
                  className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
                >
                  <Send size={14} /> {t("Submit for Approval")}
                </button>
              </>
            )}
          </>
        )}
        {actionMessage && <span className="text-xs text-muted">{actionMessage}</span>}
      </div>

      {/* Plan lines grid */}
      <div className="mt-3 overflow-x-auto rounded-lg border border-border bg-card">
        <table className="w-full min-w-[1200px] text-xs">
          <thead>
            <tr>
              <th className={groupTh} colSpan={4}>{t("Role & Scope")}</th>
              <th className={groupTh} colSpan={3}>{t("Establishment")}</th>
              <th className={groupTh} colSpan={3}>{t("Demand")}</th>
              <th className={groupTh} colSpan={3}>{t("Supply")}</th>
              <th className={groupTh} colSpan={3}>{t("Separations")}</th>
              <th className={groupTh} colSpan={3}>{t("Annual Cost / Head")}</th>
              <th className={groupTh} colSpan={5}>{t("Result")}</th>
            </tr>
            <tr className="text-left">
              <th className={th}>{t("Unit")}</th>
              <th className={th}>{t("Role")}</th>
              <th className={th}>{t("Type")}</th>
              <th className={th}>{t("Yr")}</th>
              <th className={th}>{t("Auth")}</th>
              <th className={th}>{t("Filled")}</th>
              <th className={th}>{t("Vac")}</th>
              <th className={th}>{t("New")}</th>
              <th className={th}>{t("Repl")}</th>
              <th className={th}>{t("Temp")}</th>
              <th className={th}>{t("MobIn")}</th>
              <th className={th}>{t("Promo")}</th>
              <th className={th}>{t("Acting")}</th>
              <th className={th}>{t("Retire")}</th>
              <th className={th}>{t("Resign")}</th>
              <th className={th}>{t("Expiry")}</th>
              <th className={th}>{t("Salary")}</th>
              <th className={th}>{t("Allow")}</th>
              <th className={th}>{t("Benefit")}</th>
              <th className={th}>{t("Crit")}</th>
              <th className={th}>{t("End HC")}</th>
              <th className={th}>{t("Gap")}</th>
              <th className={th}>{t("Cost")}</th>
              <th className={th}></th>
            </tr>
          </thead>
          <tbody>
            {lines.length === 0 && (
              <tr>
                <td colSpan={24} className="px-4 py-6 text-center text-sm text-muted">
                  {t("No lines yet — add lines manually or populate from the establishment.")}
                </td>
              </tr>
            )}
            {lines.map((l, i) => {
              const c = computeLine(l);
              const numeric = (name: keyof WorkforcePlanLineModel, w = "w-12") => (
                <input
                  type="text"
                  disabled={readOnly}
                  value={String(l[name] ?? 0)}
                  onChange={(e) => setLine(i, { [name]: e.target.value } as never)}
                  className={`${inputCls} ${w}`}
                />
              );
              return (
                <tr key={i} className="border-b border-border/50">
                  <td className="px-1 py-1">
                    <select
                      disabled={readOnly}
                      value={l.organizationUnitId}
                      onChange={(e) => setLine(i, { organizationUnitId: e.target.value })}
                      className={`${selectCls} max-w-36`}
                    >
                      <option value="">{t("Unit…")}</option>
                      {(units?.data ?? []).map((u) => (
                        <option key={u.id} value={u.id}>{u.name}</option>
                      ))}
                    </select>
                  </td>
                  <td className="px-1 py-1">
                    <select
                      disabled={readOnly}
                      value={l.positionClassId}
                      onChange={(e) => setLine(i, { positionClassId: e.target.value })}
                      className={`${selectCls} max-w-36`}
                    >
                      <option value="">{t("Role…")}</option>
                      {(classes?.data ?? []).map((c2) => (
                        <option key={c2.id} value={c2.id}>{c2.title}</option>
                      ))}
                    </select>
                  </td>
                  <td className="px-1 py-1">
                    <select
                      disabled={readOnly}
                      value={l.employmentType}
                      onChange={(e) => setLine(i, { employmentType: e.target.value })}
                      className={selectCls}
                    >
                      {plannedEmploymentTypeOptions.map((o) => (
                        <option key={o.id} value={o.id}>{o.name}</option>
                      ))}
                    </select>
                  </td>
                  <td className="px-1 py-1">
                    <select
                      disabled={readOnly}
                      value={l.periodIndex}
                      onChange={(e) => setLine(i, { periodIndex: Number(e.target.value) })}
                      className={selectCls}
                    >
                      {Array.from({ length: Number(formData.periodCount) || 1 }, (_, p) => (
                        <option key={p} value={p}>{p + 1}</option>
                      ))}
                    </select>
                  </td>
                  <td className="px-1 py-1">{numeric("authorizedHeadcount")}</td>
                  <td className="px-1 py-1">{numeric("filledCount")}</td>
                  <td className="px-1 py-1">{numeric("vacantCount")}</td>
                  <td className="px-1 py-1">{numeric("newHires")}</td>
                  <td className="px-1 py-1">{numeric("replacements")}</td>
                  <td className="px-1 py-1">{numeric("temporaryStaff")}</td>
                  <td className="px-1 py-1">{numeric("mobilityIn")}</td>
                  <td className="px-1 py-1">{numeric("promotions")}</td>
                  <td className="px-1 py-1">{numeric("actingAssignments")}</td>
                  <td className="px-1 py-1">{numeric("retirements")}</td>
                  <td className="px-1 py-1">{numeric("resignations")}</td>
                  <td className="px-1 py-1">{numeric("contractExpiries")}</td>
                  <td className="px-1 py-1">{numeric("annualSalaryCost", "w-20")}</td>
                  <td className="px-1 py-1">{numeric("annualAllowances", "w-16")}</td>
                  <td className="px-1 py-1">{numeric("annualBenefits", "w-16")}</td>
                  <td className="px-1 py-1 text-center">
                    <input
                      type="checkbox"
                      disabled={readOnly}
                      checked={l.isCriticalRole === true}
                      onChange={(e) => setLine(i, { isCriticalRole: e.target.checked })}
                      title={t("Critical role (HC062)")}
                    />
                  </td>
                  <td className="px-1 py-1 text-right font-semibold tabular-nums">{c.endHeadcount}</td>
                  <td
                    className={`px-1 py-1 text-right tabular-nums ${c.gap > 0 ? "font-semibold text-warning" : "text-muted"}`}
                    title={t("Headcount beyond the authorized establishment (HC062)")}
                  >
                    {c.gap}
                  </td>
                  <td className="px-1 py-1 text-right tabular-nums">{fmtMoney(c.cost)}</td>
                  <td className="px-1 py-1 text-center">
                    {!readOnly && (
                      <button
                        type="button"
                        onClick={() => removeLine(i)}
                        className="rounded p-0.5 text-muted hover:bg-error/10 hover:text-error"
                        title={t("Remove line")}
                      >
                        <Trash2 size={13} />
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Submit modal — escalation justification when over threshold (HC066) */}
      {showSubmit && (
        <Modal
          visible
          size="md"
          title={t("Submit for Approval")}
          description={formData.name}
          onClose={() => setShowSubmit(false)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setShowSubmit(false)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={busy || (totals.excess > 0 && !justification.trim())}
                onClick={doSubmit}
                className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
              >
                <Send size={15} /> {t("Submit")}
              </button>
            </>
          }
        >
          <div className="space-y-2 text-sm text-foreground">
            <p>
              {t("Projected cost")}: <strong>{fmtMoney(totals.cost)}</strong> · {t("Budget")}:{" "}
              <strong>{fmtMoney(Number(formData.totalBudget) || 0)}</strong>
            </p>
            {totals.excess > 0 ? (
              <>
                <p className="rounded-md border border-error/30 bg-error/10 px-3 py-2 text-xs text-error">
                  {t("The plan exceeds the budget threshold by")} {fmtMoney(totals.excess)}.{" "}
                  {t("An escalation justification is required (HC066).")}
                </p>
                <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                  {t("Escalation Justification")} <span className="text-error">*</span>
                </label>
                <textarea
                  autoFocus
                  rows={4}
                  value={justification}
                  onChange={(e) => setJustification(e.target.value)}
                  className="w-full resize-y rounded-md border border-border bg-background px-3 py-2 text-sm text-foreground focus:border-primary focus:outline-none"
                />
              </>
            ) : (
              <p className="text-xs text-muted">
                {t("The plan is within budget — it will be routed through the approval workflow.")}
              </p>
            )}
            <p className="text-xs text-muted">
              {t("Unsaved grid changes are not submitted — save the plan first.")}
            </p>
          </div>
        </Modal>
      )}
    </div>
  );
}

export default WorkforcePlanForm;
