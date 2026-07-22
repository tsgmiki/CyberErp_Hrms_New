"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { FlaskConical } from "lucide-react";
import type { SalaryRevisionModel, SalarySimulationModel } from "@/models";
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

const TYPE_OPTIONS = [
  { id: "Merit", name: "Merit" },
  { id: "Market", name: "Market" },
  { id: "CostOfLiving", name: "Cost of Living" },
];
const BASIS_OPTIONS = [
  { id: "Percentage", name: "Percentage" },
  { id: "FixedAmount", name: "Fixed amount" },
];
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

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  const runSim = async () => {
    setSimBusy(true);
    try {
      setSim(await simulateSalaryRevision({
        basis: formData.basis || "Percentage",
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
            { name: "rate", label: formData.basis === "Percentage" ? "Percent" : "Amount", type: "text", inputType: "number", value: formData.rate, onChange: changeHandler },
            { name: "targetJobGradeId", label: "Target grade", type: "dropDown", onSelect: selectHandler, value: formData.targetJobGradeId ?? "", displayValue: optionLabel(gradeOptions, formData.targetJobGradeId ?? ""), data: gradeOptions as never },
            { name: "targetOrganizationUnitId", label: "Target unit", type: "dropDown", onSelect: selectHandler, value: formData.targetOrganizationUnitId ?? "", displayValue: optionLabel(unitOptions, formData.targetOrganizationUnitId ?? ""), data: unitOptions as never },
            { name: "notes", label: "Notes", type: "textarea", colSpan: "full", rowNo: 2, value: formData.notes, onChange: changeHandler },
          ],
        }}
      />

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
          </div>
        </DetailSection>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default SalaryRevisionForm;
