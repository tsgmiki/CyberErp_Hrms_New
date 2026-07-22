"use client";
import { memo, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Wallet, Send, X } from "lucide-react";
import { getMyCompensation, submitCompensationRequest } from "@/services/admin/compensation";
import { EntityModuleShell } from "@/template";
import DetailSection from "@/components/common/detailSection";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import SelectField from "@/components/ui/selectField";
import InputField from "@/components/ui/inputField";
import TextreaField from "@/components/ui/textreaField";
import ButtonField from "@/components/ui/buttonField";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import Loading from "../../common/loader/loader";

const money = (n?: number) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const REQ_TYPES = [
  { id: "BenefitChange", name: "Benefit change" },
  { id: "PayrollDiscrepancy", name: "Payroll discrepancy" },
];
const emptyReq = { requestType: "BenefitChange", subject: "", details: "", referencePeriod: "", disputedAmount: "" };

function Stat({ label, v, strong }: { label: string; v: string; strong?: boolean }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-muted">{label}</p>
      <p className={`tabular-nums ${strong ? "text-base font-bold text-primary" : "font-semibold text-foreground"}`}>{v}</p>
    </div>
  );
}

/** HC233/HC234 — the signed-in employee's own compensation + raise a request. */
function MyCompensation() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading, isError } = useQuery({ queryKey: ["myCompensation"], queryFn: getMyCompensation });
  const [showReq, setShowReq] = useState(false);
  const [req, setReq] = useState({ ...emptyReq });
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);

  const submit = async () => {
    setBusy(true);
    const res = await submitCompensationRequest({
      requestType: req.requestType,
      subject: req.subject.trim(),
      details: req.details.trim(),
      referencePeriod: req.referencePeriod || undefined,
      disputedAmount: req.disputedAmount !== "" ? Number(req.disputedAmount) : undefined,
    });
    setBusy(false);
    setMsg(res.message);
    if (res.ok) {
      setShowReq(false);
      setReq({ ...emptyReq });
      queryClient.invalidateQueries({ queryKey: ["compensationRequests"] });
    }
  };

  const allowanceColumns = useMemo(
    () =>
      [
        { name: "allowanceTypeName", label: "Allowance" },
        { name: "resolvedAmount", label: "Amount", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.resolvedAmount)}</span> },
      ] as DataTableColumnModel[],
    [],
  );
  const benefitColumns = useMemo(
    () =>
      [
        { name: "benefitPlanName", label: "Plan", render: (text: string, r: any) => <>{text} <span className="text-[10px] text-muted">{r.category}</span></> },
        { name: "status", label: "Status", render: (text: string) => t(text ?? "") },
        { name: "employeeContribution", label: "My contribution", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.employeeContribution)}</span> },
      ] as DataTableColumnModel[],
    [t],
  );

  if (isLoading) return <Loading />;
  if (isError || !data) return <div className="p-6 text-center text-sm text-muted">{t("Your account is not linked to an employee record.")}</div>;

  const s = data.summary ?? {};
  const d = data.deductions ?? {};
  const activeAllowances = (s.allowances ?? []).filter((a) => a.isCurrentlyActive);
  const benefits = data.benefits ?? [];

  return (
    <EntityModuleShell
      title="My Compensation"
      headerDescription="Your pay, allowances, benefits and deductions — and raise a change or discrepancy request"
      headerIcon={<Wallet className="h-6 w-6 text-primary" />}
      tableTitle="My Compensation"
      hideAdd
      hideBack
      showForm={false}
      onList={() => undefined}
      onAdd={() => undefined}
    >
      <div className="m-2 flex min-h-0 flex-1 flex-col gap-3 overflow-auto">
        <div className="flex items-center justify-end">
          <ButtonField value={showReq ? "Cancel" : "Raise a request"} variant={showReq ? "outline" : "primary"} icon={showReq ? <X size={14} /> : <Send size={14} />} onClick={() => setShowReq((v) => !v)} />
        </div>
        {msg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

        {showReq && (
          <DetailSection title="New Request">
            <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
              <SelectField type="select" name="requestType" label="Request type" value={req.requestType} data={REQ_TYPES as never} onChange={(e) => setReq((p) => ({ ...p, requestType: (e.target as HTMLInputElement).value }))} />
              <InputField type="text" name="subject" label="Subject" required value={req.subject} onChange={(e) => setReq((p) => ({ ...p, subject: (e.target as HTMLInputElement).value }))} />
              {req.requestType === "PayrollDiscrepancy" && (
                <>
                  <InputField type="text" name="referencePeriod" label="Pay period" placeholder="2026-06" value={req.referencePeriod} onChange={(e) => setReq((p) => ({ ...p, referencePeriod: (e.target as HTMLInputElement).value }))} />
                  <InputField type="text" inputType="number" name="disputedAmount" label="Disputed amount" value={req.disputedAmount} onChange={(e) => setReq((p) => ({ ...p, disputedAmount: (e.target as HTMLInputElement).value }))} />
                </>
              )}
              <div className="md:col-span-2">
                <TextreaField type="textarea" name="details" label="Details" rowNo={3} value={req.details} onChange={(e) => setReq((p) => ({ ...p, details: (e.target as HTMLInputElement).value }))} />
              </div>
            </div>
            <div className="mt-3 flex justify-end">
              <ButtonField value={busy ? "Submitting…" : "Submit"} variant="primary" icon={<Send size={14} />} disabled={busy || !req.subject.trim() || !req.details.trim()} onClick={submit} />
            </div>
          </DetailSection>
        )}

        <div className="grid grid-cols-1 gap-3 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <DetailSection title="Pay & Allowances">
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <Stat label={t("Base")} v={money(s.baseSalary)} />
                <Stat label={t("Allowances")} v={money(s.totalAllowances)} />
                <Stat label={t("Gross pay")} v={money(s.grossPay)} strong />
                <Stat label={t("Grade / Step")} v={`${s.jobGradeName ?? "—"}`} />
              </div>
              {activeAllowances.length > 0 && (
                <div className="mt-3">
                  <DataTableProvider dataTable={{ columns: allowanceColumns, data: activeAllowances, count: activeAllowances.length, pagination: "None", search: "None", key: "id" }} />
                </div>
              )}
            </DetailSection>
          </div>
          <DetailSection title="Deductions & Net">
            <div className="space-y-1 text-sm">
              {(d.lines ?? []).map((l, i) => (
                <div key={i} className="flex justify-between">
                  <span className="text-muted">{l.label}</span>
                  <span className="tabular-nums">- {money(l.amount)}</span>
                </div>
              ))}
              <div className="flex justify-between border-t border-border/60 pt-1">
                <span className="text-muted">{t("Total deductions")}</span>
                <span className="tabular-nums">- {money(d.totalDeductions)}</span>
              </div>
              <div className="flex justify-between text-base font-bold text-primary">
                <span>{t("Net pay")}</span>
                <span className="tabular-nums">{money(d.netPay)}</span>
              </div>
            </div>
          </DetailSection>
        </div>

        <DetailSection title="My Benefits">
          {benefits.length === 0 ? (
            <p className="text-xs text-muted">{t("You are not enrolled in any benefit plans.")}</p>
          ) : (
            <DataTableProvider dataTable={{ columns: benefitColumns, data: benefits, count: benefits.length, pagination: "None", search: "None", key: "id" }} />
          )}
        </DetailSection>
      </div>
    </EntityModuleShell>
  );
}

export default memo(MyCompensation);
