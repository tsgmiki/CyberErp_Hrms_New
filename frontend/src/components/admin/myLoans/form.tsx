"use client";
import { memo, useCallback, useMemo, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import FormProviders from "@/components/common/formProvider/formProvider";
import InputField from "@/components/ui/inputField";
import ButtonField from "@/components/ui/buttonField";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { getAllLoanTypes, requestLoan } from "@/services/admin/loan";
import type { LoanTypeModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import { money } from "../loan/shared";

const FormProvider = memo(FormProviders);
type Guar = { fullName: string; relationship: string; phoneNumber: string; guaranteedAmount: string };
const blankGuar: Guar = { fullName: "", relationship: "", phoneNumber: "", guaranteedAmount: "" };
const NEW_DEFAULTS = { loanTypeId: "", principalAmount: "", termMonths: "", purpose: "" };

/** HC252 — the signed-in employee's loan request form (loan type, amount, term, purpose, guarantors). */
function MyLoanRequestForm({ onDone }: { onDone: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();

  const [formState, setFormState] = useState<any>({});
  const [form, setForm] = useState({ ...NEW_DEFAULTS });
  const [guars, setGuars] = useState<Guar[]>([]);
  const [busy, setBusy] = useState(false);

  const { data: types, isLoading } = useQuery({
    queryKey: ["loanTypeOpts"],
    queryFn: () => getAllLoanTypes({ ...parameterInitialData, take: 200, status: "true" }),
    staleTime: 60_000,
  });
  const activeTypes = (types?.data ?? []) as LoanTypeModel[];
  const selType = activeTypes.find((x) => x.id === form.loanTypeId);
  const typeOptions = activeTypes.map((x) => ({ id: x.id!, name: `${x.name}${x.interestRatePct ? ` (${x.interestRatePct}%)` : ` (${t("interest-free")})`}` }));
  const minGuar = selType?.requiresGuarantor ? Math.max(1, selType.minGuarantors ?? 1) : 0;

  const changeHandler = useCallback((e: any) => setForm((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setForm((p) => ({ ...p, [name]: r.id })), []);
  const setGuar = (i: number, patch: Partial<Guar>) => setGuars((s) => s.map((x, j) => (j === i ? { ...x, ...patch } : x)));

  const estMonthly = useMemo(() => {
    const p = Number(form.principalAmount), n = Number(form.termMonths), r = Number(selType?.interestRatePct ?? 0);
    if (!p || !n) return 0;
    const interest = Math.round((p * r / 100 * n / 12) * 100) / 100;
    return Math.round(((p + interest) / n) * 100) / 100;
  }, [form.principalAmount, form.termMonths, selType]);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    if (!form.loanTypeId || form.principalAmount === "" || form.termMonths === "") {
      setFormState({ status: "error", message: t("Please fill the loan type, amount and term."), zodErrors: {} });
      return;
    }
    setBusy(true);
    const res = await requestLoan({
      loanTypeId: form.loanTypeId,
      principalAmount: Number(form.principalAmount),
      termMonths: Number(form.termMonths),
      purpose: form.purpose.trim() || undefined,
      guarantors: guars
        .filter((g) => g.fullName.trim())
        .map((g) => ({ fullName: g.fullName.trim(), relationship: g.relationship || undefined, phoneNumber: g.phoneNumber || undefined, guaranteedAmount: g.guaranteedAmount !== "" ? Number(g.guaranteedAmount) : undefined })),
    });
    setBusy(false);
    setFormState({ status: res.ok ? "success" : "error", message: res.message, zodErrors: {} });
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ["myLoans"] });
      onDone();
    }
  };

  if (isLoading) return <Loading />;
  if (activeTypes.length === 0)
    return <p className="m-4 rounded-lg border border-dashed border-border p-6 text-center text-sm text-muted">{t("No loan products are available. Contact HR.")}</p>;

  return (
    <div className="text-white">
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: busy,
          SubmitButton: "top",
          submitBtnTitle: "Submit request",
          components: [
            { name: "loanTypeId", label: "Loan Type", required: true, type: "dropDown", onSelect: selectHandler, value: form.loanTypeId, displayValue: typeOptions.find((o) => o.id === form.loanTypeId)?.name, data: typeOptions as never },
            { name: "principalAmount", label: "Amount", required: true, value: form.principalAmount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "termMonths", label: "Term (months)", required: true, value: form.termMonths, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "purpose", label: "Purpose", value: form.purpose, onChange: changeHandler, type: "text", colSpan: "full" },
          ],
        }}
      >
        {selType && (
          <div className="mt-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
            {t("Est. monthly installment")}: <b className="text-foreground tabular-nums">{money(estMonthly)}</b>
            {selType.maxAmount != null ? ` · ${t("max")} ${selType.maxAmount.toLocaleString()}` : ""}
            {selType.maxSalaryMultiple ? ` · ${t("max")} ${selType.maxSalaryMultiple}× ${t("salary")}` : ""}
            {` · ${t("max")} ${selType.maxTermMonths} ${t("mo")}`}
            {selType.serviceCommitmentMonths ? ` · ${selType.serviceCommitmentMonths}${t("mo")} ${t("service commitment")}` : ""}
          </div>
        )}

        <div className="mt-3">
          <div className="mb-1 flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wide text-muted">
              {t("Guarantors")}{minGuar > 0 ? ` · ${t("at least")} ${minGuar}` : ` · ${t("optional")}`}
            </span>
            <ButtonField value="Add" variant="outline" icon={<Plus size={13} />} onClick={() => setGuars((g) => [...g, { ...blankGuar }])} />
          </div>
          <div className="space-y-1">
            {guars.map((g, i) => (
              <div key={i} className="flex flex-wrap items-center gap-2">
                <div className="w-40"><InputField type="text" name={`gFullName${i}`} label="" placeholder="Full name" value={g.fullName} onChange={(e: any) => setGuar(i, { fullName: e.target.value })} /></div>
                <div className="w-32"><InputField type="text" name={`gRel${i}`} label="" placeholder="Relationship" value={g.relationship} onChange={(e: any) => setGuar(i, { relationship: e.target.value })} /></div>
                <div className="w-32"><InputField type="text" name={`gPhone${i}`} label="" placeholder="Phone" value={g.phoneNumber} onChange={(e: any) => setGuar(i, { phoneNumber: e.target.value })} /></div>
                <div className="w-32"><InputField type="text" inputType="number" name={`gAmt${i}`} label="" placeholder="Guaranteed amt" value={g.guaranteedAmount} onChange={(e: any) => setGuar(i, { guaranteedAmount: e.target.value })} /></div>
                <button type="button" onClick={() => setGuars((s) => s.filter((_, j) => j !== i))} className="rounded p-1 text-error hover:bg-error/10"><Trash2 size={14} /></button>
              </div>
            ))}
          </div>
        </div>
      </FormProvider>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default MyLoanRequestForm;
