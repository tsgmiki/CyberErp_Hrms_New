"use client";
import { memo, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Wallet, Plus, ShieldOff, Ban } from "lucide-react";
import {
  getCompensationSummary, getEmployeeAllowances, saveEmployeeAllowance, deleteEmployeeAllowance,
  getEmployeeBenefits, enrollBenefit, waiveBenefit, terminateBenefit, getDeductions,
  getAllAllowanceTypes, getAllBenefitPlans,
} from "@/services/admin/compensation";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { parameterInitialData } from "@/constants/initialization";
import { EntityModuleShell } from "@/template";
import EmployeePicker from "@/components/common/employeePicker";
import DropDownField from "@/components/ui/dropDownField";
import DateField from "@/components/ui/dateField";
import InputField from "@/components/ui/inputField";
import ButtonField from "@/components/ui/buttonField";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import GridAction from "@/components/common/gridAction/gridAction";
import DetailSection from "@/components/common/detailSection";

const money = (n?: number) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const iso = (d: Date) => d.toISOString().slice(0, 10);

/** HC226/HC230 — HR per-employee compensation: allowances, benefits and the deductions summary. */
function EmployeeCompensation() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [emp, setEmp] = useState<{ id: string; name: string } | null>(null);
  const [msg, setMsg] = useState("");
  const [newAllow, setNewAllow] = useState({ allowanceTypeId: "", allowanceTypeName: "", value: "", effectiveFrom: iso(new Date()) });
  const [newBenefit, setNewBenefit] = useState({ benefitPlanId: "", benefitPlanName: "", coverageStart: iso(new Date()) });

  const empId = emp?.id;
  const { data: summary } = useQuery({ queryKey: ["compSummary", empId], queryFn: () => getCompensationSummary(empId!), enabled: !!empId });
  const { data: allowances } = useQuery({ queryKey: ["empAllowances", empId], queryFn: () => getEmployeeAllowances(empId!), enabled: !!empId });
  const { data: benefits } = useQuery({ queryKey: ["empBenefits", empId], queryFn: () => getEmployeeBenefits(empId!), enabled: !!empId });
  const { data: deductions } = useQuery({ queryKey: ["empDeductions", empId], queryFn: () => getDeductions(empId!), enabled: !!empId });
  const { data: types } = useQuery({ queryKey: ["allowanceTypeOpts"], queryFn: () => getAllAllowanceTypes({ ...parameterInitialData, take: 200, status: "true" }), staleTime: 60_000 });
  const { data: plans } = useQuery({ queryKey: ["benefitPlanOpts"], queryFn: () => getAllBenefitPlans({ ...parameterInitialData, take: 200, status: "true" }), staleTime: 60_000 });

  const typeOptions = (types?.data ?? []).map((x) => ({ id: x.id!, name: x.name! }));
  const planOptions = (plans?.data ?? []).map((x) => ({ id: x.id!, name: x.name! }));

  const refresh = (m: string) => { setMsg(m); ["compSummary", "empAllowances", "empBenefits", "empDeductions"].forEach((k) => queryClient.invalidateQueries({ queryKey: [k, empId] })); };

  const addAllowance = async () => {
    if (!empId || !newAllow.allowanceTypeId || newAllow.value === "") return;
    const res = await saveEmployeeAllowance({ employeeId: empId, allowanceTypeId: newAllow.allowanceTypeId, value: Number(newAllow.value), effectiveFrom: newAllow.effectiveFrom });
    refresh(res.message);
    if (res.ok) setNewAllow({ allowanceTypeId: "", allowanceTypeName: "", value: "", effectiveFrom: iso(new Date()) });
  };
  const enroll = async () => {
    if (!empId || !newBenefit.benefitPlanId) return;
    const res = await enrollBenefit({ employeeId: empId, benefitPlanId: newBenefit.benefitPlanId, coverageStart: newBenefit.coverageStart });
    refresh(res.message);
    if (res.ok) setNewBenefit({ benefitPlanId: "", benefitPlanName: "", coverageStart: iso(new Date()) });
  };

  const allowanceColumns = useMemo(
    () =>
      [
        {
          name: "allowanceTypeName", label: "Allowance",
          render: (_t: unknown, a: any) => (
            <span className={a.isCurrentlyActive ? "" : "opacity-50"}>
              {a.allowanceTypeName}{!a.isTaxable && <span className="ml-1 text-[10px] text-muted">{t("exempt")}</span>}
            </span>
          ),
        },
        {
          name: "resolvedAmount", label: "Amount",
          render: (_t: unknown, a: any) => <span className="tabular-nums">{money(a.resolvedAmount)}{a.calcMethod === "PercentOfBase" ? ` (${a.value}%)` : ""}</span>,
        },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, a: any) => (
            <GridAction id={a.id} record={a} showAdd={false} showEdit={false} showDelete deleteHandler={(id) => deleteEmployeeAllowance(id).then((r) => refresh(r.message))} />
          ),
        },
      ] as DataTableColumnModel[],
    [t, empId],
  );

  const benefitColumns = useMemo(
    () =>
      [
        {
          name: "benefitPlanName", label: "Plan",
          render: (_t: unknown, b: any) => (
            <span className={b.status === "Enrolled" ? "" : "opacity-50"}>
              {b.benefitPlanName}<span className="ml-1 text-[10px] text-muted">{t(b.status ?? "")}</span>
            </span>
          ),
        },
        { name: "employeeContribution", label: "Contribution", render: (_t: unknown, b: any) => <span className="tabular-nums">{money(b.employeeContribution)}</span> },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, b: any) =>
            b.status === "Enrolled" ? (
              <div className="flex items-center gap-1">
                <ButtonField value="Waive" variant="ghost" icon={<ShieldOff size={13} />} onClick={() => b.id && waiveBenefit(b.id).then((r: any) => refresh(r.message))} />
                <ButtonField value="Terminate" variant="danger" icon={<Ban size={13} />} onClick={() => b.id && terminateBenefit(b.id, iso(new Date())).then((r: any) => refresh(r.message))} />
              </div>
            ) : null,
        },
      ] as DataTableColumnModel[],
    [t, empId],
  );

  return (
    <EntityModuleShell
      title="Employee Compensation"
      headerDescription="Manage an employee's allowances and benefits and see their resolved pay & deductions"
      headerIcon={<Wallet className="h-6 w-6 text-primary" />}
      tableTitle="Employee Compensation"
      hideAdd
      hideBack
      showForm={false}
      onList={() => undefined}
      onAdd={() => undefined}
    >
      <div className="m-2 flex min-h-0 flex-1 flex-col gap-3">
        <div className="max-w-md"><EmployeePicker value={emp?.id} displayValue={emp?.name} onSelect={(id, name) => setEmp({ id, name })} placeholder={t("Select an employee…")} /></div>
        {msg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

        {emp && (
          <div className="grid min-h-0 flex-1 grid-cols-1 gap-3 overflow-auto lg:grid-cols-2">
            <div className="space-y-3">
              <DetailSection title="Compensation Summary">
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <Stat label={t("Base salary")} v={money(summary?.baseSalary)} />
                  <Stat label={t("Grade / Step")} v={`${summary?.jobGradeName ?? "—"} / ${summary?.stepName ?? "—"}`} />
                  <Stat label={t("Taxable allowances")} v={money(summary?.taxableAllowances)} />
                  <Stat label={t("Non-taxable")} v={money(summary?.nonTaxableAllowances)} />
                  <Stat label={t("Gross pay")} v={money(summary?.grossPay)} strong />
                  <Stat label={t("Taxable gross")} v={money(summary?.taxableGross)} />
                </div>
              </DetailSection>
              {deductions && (
                <DetailSection title="Deductions & Net">
                  <div className="space-y-1 text-sm">
                    <div className="flex justify-between"><span className="text-muted">{t("Income tax")}</span><span className="tabular-nums">- {money(deductions.incomeTax)}</span></div>
                    <div className="flex justify-between"><span className="text-muted">{t("Benefit contributions")}</span><span className="tabular-nums">- {money(deductions.employeeBenefitContributions)}</span></div>
                    <div className="mt-1 flex justify-between border-t border-border/60 pt-1 font-semibold"><span>{t("Net pay")}</span><span className="tabular-nums text-primary">{money(deductions.netPay)}</span></div>
                  </div>
                </DetailSection>
              )}
            </div>

            <div className="space-y-3">
              <DetailSection title="Allowances">
                <div className="mb-2 flex flex-wrap items-end gap-2">
                  <div className="min-w-44 flex-1"><DropDownField type="dropDown" compact name="allowanceType" placeholder="Type…" value={newAllow.allowanceTypeId} displayValue={newAllow.allowanceTypeName} data={typeOptions} onSelect={(_n, item) => setNewAllow((p) => ({ ...p, allowanceTypeId: item.id, allowanceTypeName: item.name }))} /></div>
                  <div className="w-28"><InputField type="text" inputType="number" name="allowanceValue" placeholder={t("Value") ?? ""} value={newAllow.value} onChange={(e) => setNewAllow((p) => ({ ...p, value: (e.target as HTMLInputElement).value }))} /></div>
                  <div className="w-40"><DateField type="date" compact name="effectiveFrom" value={newAllow.effectiveFrom} onChange={(e) => setNewAllow((p) => ({ ...p, effectiveFrom: (e.target as HTMLInputElement).value }))} /></div>
                  <ButtonField value="Add" variant="primary" icon={<Plus size={14} />} disabled={!newAllow.allowanceTypeId || newAllow.value === ""} onClick={addAllowance} />
                </div>
                <DataTableProvider dataTable={{ columns: allowanceColumns, data: allowances ?? [], count: allowances?.length ?? 0, pagination: "None", search: "None", key: "id" }} />
              </DetailSection>

              <DetailSection title="Benefits">
                <div className="mb-2 flex flex-wrap items-end gap-2">
                  <div className="min-w-44 flex-1"><DropDownField type="dropDown" compact name="benefitPlan" placeholder="Plan…" value={newBenefit.benefitPlanId} displayValue={newBenefit.benefitPlanName} data={planOptions} onSelect={(_n, item) => setNewBenefit((p) => ({ ...p, benefitPlanId: item.id, benefitPlanName: item.name }))} /></div>
                  <div className="w-40"><DateField type="date" compact name="coverageStart" value={newBenefit.coverageStart} onChange={(e) => setNewBenefit((p) => ({ ...p, coverageStart: (e.target as HTMLInputElement).value }))} /></div>
                  <ButtonField value="Enroll" variant="primary" icon={<Plus size={14} />} disabled={!newBenefit.benefitPlanId} onClick={enroll} />
                </div>
                <DataTableProvider dataTable={{ columns: benefitColumns, data: benefits ?? [], count: benefits?.length ?? 0, pagination: "None", search: "None", key: "id" }} />
              </DetailSection>
            </div>
          </div>
        )}
      </div>
    </EntityModuleShell>
  );
}

function Stat({ label, v, strong }: { label: string; v: string; strong?: boolean }) {
  return <div><p className="text-[11px] uppercase tracking-wide text-muted">{label}</p><p className={`tabular-nums ${strong ? "text-base font-bold text-primary" : "font-semibold text-foreground"}`}>{v}</p></div>;
}

export default memo(EmployeeCompensation);
