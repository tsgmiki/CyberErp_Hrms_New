"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { FlaskConical, Plus, Trash2 } from "lucide-react";
import InputField from "@/components/ui/inputField";
import type { SalaryRevisionModel, SalarySimulationModel, SalaryRevisionBandModel } from "@/models";
import { simulateSalaryRevision, saveSalaryRevision } from "@/services/admin/compensation";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import getAllOrganizationUnits from "@/services/admin/organizationUnit/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { optionLabel } from "@/constants/leave";
import DetailSection from "@/components/common/detailSection";
import ButtonField from "@/components/ui/buttonField";
import { StatusMessage } from "../../common/statusMessage/status";
import { money } from "./shared";

const FormProvider = memo(FormProviders);

const PERFORMANCE = "Performance";

const TYPE_OPTIONS = [
  { id: PERFORMANCE, name: "By performance (score bands)" },
  { id: "Merit", name: "Merit" },
  { id: "Market", name: "Market" },
  { id: "CostOfLiving", name: "Cost of Living" },
];
const BASIS_OPTIONS = [
  { id: "Percentage", name: "Percentage" },
  { id: "FixedAmount", name: "Fixed amount" },
  { id: "Step", name: "By step (salary scale)" },
];

/** The `rate` field means something different per basis, so it is labelled and hinted per basis. */
const RATE_FIELD: Record<string, { label: string; help: string }> = {
  Percentage: { label: "Percent", help: "Uplift applied to each current salary, e.g. 7.5 for 7.5%." },
  FixedAmount: { label: "Amount", help: "Flat amount added to each current salary." },
  Step: {
    label: "Step increment",
    help: "Steps to advance on each employee's own grade ladder. Fractions are allowed (e.g. 1.5, 2.5) — "
      + "a landing step between two rungs is interpolated between them. Employees already at the grade "
      + "ceiling stay there.",
  },
};
/**
 * Starting bands per basis, matching the standard policy shape. They are only DEFAULTS: thresholds
 * are data because the appraisal score scale is configured per tenant (live rating scales here run
 * 1-5, 1-3 and 0-130), so a "90" threshold is meaningless on a 1-5 scale. The simulation reports the
 * score range actually observed so a mis-scaled set is obvious before anything is applied.
 */
const DEFAULT_BANDS: Record<string, SalaryRevisionBandModel[]> = {
  Step: [
    { minScore: 90, value: 2.5, label: "Outstanding" },
    { minScore: 70, value: 2, label: "Strong" },
    { minScore: 0, value: 1, label: "Standard" },
  ],
  Percentage: [
    { minScore: 90, value: 15, label: "Outstanding" },
    { minScore: 70, value: 10, label: "Strong" },
    { minScore: 0, value: 0, label: "Standard" },
  ],
  FixedAmount: [
    { minScore: 90, value: 3000, label: "Outstanding" },
    { minScore: 70, value: 2500, label: "Strong" },
    { minScore: 0, value: 0, label: "Standard" },
  ],
};

const NEW_DEFAULTS: SalaryRevisionModel = { revisionType: "CostOfLiving", basis: "Percentage", rate: 0 };

function SalaryRevisionForm({ onDone }: { onDone: () => void }) {
  const { t } = useTranslation();
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<SalaryRevisionModel>({ ...NEW_DEFAULTS });
  const [sim, setSim] = useState<SalarySimulationModel | null>(null);
  const [simBusy, setSimBusy] = useState(false);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: grades } = useQuery({ queryKey: ["jobGradeOptions"], queryFn: () => getAllJobGrade({ ...parameterInitialData, take: 200 }), staleTime: 60_000 });
  const { data: units } = useQuery({ queryKey: ["unitOptions"], queryFn: () => getAllOrganizationUnits({ ...parameterInitialData, take: 500 }), staleTime: 60_000 });

  const gradeOptions = useMemo(() => [{ id: "", name: t("All grades") }, ...(grades?.data ?? []).map((g) => ({ id: g.id!, name: g.name! }))], [grades, t]);
  const unitOptions = useMemo(() => [{ id: "", name: t("All units") }, ...(units?.data ?? []).map((u) => ({ id: u.id!, name: u.name! }))], [units, t]);

  const isPerformance = formData.revisionType === PERFORMANCE;
  const basisKey = formData.basis ?? "Percentage";
  const bands = formData.bands ?? [];

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  // Seed the bands the first time Performance is chosen, and re-seed when the BASIS changes, since a
  // band value means steps / percent / amount depending on it — carrying "15" from percent over to
  // steps would silently request a 15-step jump.
  useEffect(() => {
    if (!isPerformance) return;
    setFormData((p) => ({ ...p, bands: DEFAULT_BANDS[basisKey] ?? DEFAULT_BANDS.Percentage }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isPerformance, basisKey]);

  const setBand = useCallback((index: number, patch: Partial<SalaryRevisionBandModel>) => {
    setFormData((p) => {
      const next = [...(p.bands ?? [])];
      next[index] = { ...next[index], ...patch };
      return { ...p, bands: next };
    });
  }, []);

  const addBand = useCallback(() => {
    setFormData((p) => ({ ...p, bands: [...(p.bands ?? []), { minScore: 0, value: 0, label: "" }] }));
  }, []);

  const removeBand = useCallback((index: number) => {
    setFormData((p) => ({ ...p, bands: (p.bands ?? []).filter((_, i) => i !== index) }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  const runSim = async () => {
    setSimBusy(true);
    try {
      setSim(await simulateSalaryRevision({
        basis: formData.basis || "Percentage",
        revisionType: formData.revisionType || "CostOfLiving",
        bands: isPerformance ? bands.map((b) => ({ ...b, minScore: Number(b.minScore ?? 0), value: Number(b.value ?? 0) })) : [],
        rate: Number(formData.rate ?? 0),
        targetJobGradeId: formData.targetJobGradeId || undefined,
        targetOrganizationUnitId: formData.targetOrganizationUnitId || undefined,
      }));
    } catch {
      setSim(null);
    }
    setSimBusy(false);
  };

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const res = await saveSalaryRevision({
      ...formData,
      bands: isPerformance ? bands.map((b) => ({ ...b, minScore: Number(b.minScore ?? 0), value: Number(b.value ?? 0) })) : [],
      rate: Number(formData.rate ?? 0),
      targetJobGradeId: formData.targetJobGradeId || undefined,
      targetOrganizationUnitId: formData.targetOrganizationUnitId || undefined,
    });
    setIsLoading(false);
    setFormState(res.ok ? { status: "success", message: res.message } : { status: "error", message: res.message });
  };

  useEffect(() => {
    if (formState.status === "success") {
      setFormData({ ...NEW_DEFAULTS });
      setSim(null);
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["salaryRevisions"] });
      onDone();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <div>
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[40%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { name: "name", label: "Name", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "revisionType", label: "Type", type: "select", value: formData.revisionType, onChange: changeHandler, data: TYPE_OPTIONS as never },
            { name: "effectiveDate", label: "Effective date", required: true, type: "date", value: formData.effectiveDate, onChange: changeHandler },
            { name: "basis", label: "Basis", type: "select", value: formData.basis, onChange: changeHandler, data: BASIS_OPTIONS as never },
            // InputField already emits step="0.0001" for inputType number, so fractional step
            // increments (1.5, 2.5) are accepted without a bespoke control.
            // Under Performance the amount comes from the bands, so the flat rate is hidden rather
            // than left on screen doing nothing.
            ...(isPerformance ? [] : [{
              name: "rate", type: "text", inputType: "number",
              label: (RATE_FIELD[formData.basis ?? "Percentage"] ?? RATE_FIELD.Percentage).label,
              placeholder: formData.basis === "Step" ? "e.g. 1.5" : undefined,
              value: formData.rate, onChange: changeHandler, error: formState?.zodErrors?.rate,
            } as never]),
            { name: "targetJobGradeId", label: "Target grade", type: "dropDown", onSelect: selectHandler, value: formData.targetJobGradeId ?? "", displayValue: optionLabel(gradeOptions, formData.targetJobGradeId ?? ""), data: gradeOptions as never },
            { name: "targetOrganizationUnitId", label: "Target unit", type: "dropDown", onSelect: selectHandler, value: formData.targetOrganizationUnitId ?? "", displayValue: optionLabel(unitOptions, formData.targetOrganizationUnitId ?? ""), data: unitOptions as never },
            { name: "notes", label: "Notes", type: "textarea", colSpan: "full", rowNo: 2, value: formData.notes, onChange: changeHandler },
          ],
        }}
      />

      {/* What `rate` means changes with the basis, so say so where the number is entered. */}
      <p className="mt-1 px-1 text-xs text-muted">
        {isPerformance
          ? t("Each employee's award comes from their appraisal score and the bands below, expressed in the units of the chosen basis.")
          : t((RATE_FIELD[formData.basis ?? "Percentage"] ?? RATE_FIELD.Percentage).help)}
      </p>

      {isPerformance && (
        <div className="mt-3">
          <DetailSection title="Score bands">
            <div className="space-y-2">
              <p className="text-xs text-muted">
                {t("Highest matching threshold wins, and thresholds are inclusive (a score of 90 gets the 90 band). Give the lowest band a threshold of 0 so it acts as a catch-all; employees with no completed appraisal are left unchanged.")}
              </p>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[520px] text-sm">
                  <thead>
                    <tr className="border-b border-border text-left text-[11px] uppercase text-muted">
                      <th className="px-2 py-1.5 font-semibold">{t("Score at least")}</th>
                      <th className="px-2 py-1.5 font-semibold">
                        {t(`Award (${(RATE_FIELD[basisKey] ?? RATE_FIELD.Percentage).label.toLowerCase()})`)}
                      </th>
                      <th className="px-2 py-1.5 font-semibold">{t("Label")}</th>
                      <th className="px-2 py-1.5" />
                    </tr>
                  </thead>
                  <tbody>
                    {bands.map((b, i) => (
                      <tr key={i} className="border-b border-border/60">
                        <td className="px-2 py-1.5">
                          <InputField name={`bandMin${i}`} type="text" inputType="number" value={b.minScore}
                            onChange={(e: any) => setBand(i, { minScore: e.target.value })} />
                        </td>
                        <td className="px-2 py-1.5">
                          <InputField name={`bandVal${i}`} type="text" inputType="number" value={b.value}
                            onChange={(e: any) => setBand(i, { value: e.target.value })} />
                        </td>
                        <td className="px-2 py-1.5">
                          <InputField name={`bandLbl${i}`} type="text" value={b.label}
                            onChange={(e: any) => setBand(i, { label: e.target.value })} />
                        </td>
                        <td className="px-2 py-1.5 text-right">
                          <ButtonField value="Remove" variant="outline" icon={<Trash2 size={13} />}
                            disabled={bands.length <= 1} onClick={() => removeBand(i)} />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <ButtonField value="Add band" variant="outline" icon={<Plus size={14} />} onClick={addBand} />
            </div>
          </DetailSection>
        </div>
      )}

      <div className="mt-3">
        <DetailSection title="Simulation">
          <div className="space-y-2">
            <ButtonField value="Simulate" variant="outline" icon={<FlaskConical size={14} />} disabled={simBusy} onClick={runSim} />
            {sim && (
              <div className="grid grid-cols-2 gap-2 rounded-md border border-info/30 bg-info/5 p-3 text-sm sm:grid-cols-4">
                <div><p className="text-[11px] uppercase text-muted">{t("Employees")}</p><p className="font-semibold tabular-nums">{sim.employeeCount}</p></div>
                <div><p className="text-[11px] uppercase text-muted">{t("Current total")}</p><p className="font-semibold tabular-nums">{money(sim.totalCurrent)}</p></div>
                <div><p className="text-[11px] uppercase text-muted">{t("Proposed total")}</p><p className="font-semibold tabular-nums">{money(sim.totalProposed)}</p></div>
                <div><p className="text-[11px] uppercase text-muted">{t("Increase")}</p><p className="font-semibold tabular-nums text-primary">+{money(sim.totalIncrease)} ({sim.averagePercent}%)</p></div>
              </div>
            )}
            {/* A step revision can legitimately move nobody (everyone at the ceiling, or off-scale).
                Without this the aggregate would just read 0 and look like a broken simulation. */}
            {/* Score scales are configured per tenant, so thresholds meant for 0-100 silently match
                nobody on a 1-5 scale. Showing the observed range next to the no-score count makes a
                mis-scaled band set obvious before the revision is submitted. */}
            {sim && isPerformance && (
              <div className="flex flex-wrap gap-4 rounded-md border border-border bg-secondary/20 px-3 py-2 text-xs">
                <span className="text-muted">
                  {t("Scores seen")}:{" "}
                  <span className="font-semibold text-foreground tabular-nums">
                    {sim.minObservedScore == null ? t("none") : `${sim.minObservedScore} – ${sim.maxObservedScore}`}
                  </span>
                </span>
                <span className="text-muted">
                  {t("No appraisal score")}:{" "}
                  <span className={`font-semibold tabular-nums ${(sim.noScoreCount ?? 0) > 0 ? "text-warning" : "text-foreground"}`}>
                    {sim.noScoreCount ?? 0}
                  </span>
                </span>
                {sim.maxObservedScore != null && bands.some((b) => Number(b.minScore ?? 0) > Number(sim.maxObservedScore)) && (
                  <span className="font-semibold text-warning">
                    {t("A band threshold is above every score seen — check it matches your rating scale.")}
                  </span>
                )}
              </div>
            )}
            {sim && formData.basis === "Step" && (
              <div className="flex flex-wrap gap-4 rounded-md border border-border bg-secondary/20 px-3 py-2 text-xs">
                <span className="text-muted">
                  {t("Interpolated between rungs")}:{" "}
                  <span className="font-semibold text-foreground tabular-nums">{sim.interpolatedCount ?? 0}</span>
                </span>
                <span className="text-muted">
                  {t("Not moved by the scale")}:{" "}
                  <span className={`font-semibold tabular-nums ${(sim.unresolvedCount ?? 0) > 0 ? "text-warning" : "text-foreground"}`}>
                    {sim.unresolvedCount ?? 0}
                  </span>
                </span>
                {(sim.unresolvedCount ?? 0) > 0 && (
                  <span className="text-muted">{t("Off-scale employees, grades without scale rows, or already at the ceiling.")}</span>
                )}
              </div>
            )}
          </div>
        </DetailSection>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default SalaryRevisionForm;
