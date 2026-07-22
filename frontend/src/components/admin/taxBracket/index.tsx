"use client";
import { memo, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Landmark, Plus, Pencil, Calculator } from "lucide-react";
import { getAllTaxBrackets, saveTaxBracket, deleteTaxBracket, getDeductions } from "@/services/admin/compensation";
import type { TaxBracketModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { parameterInitialData } from "@/constants/initialization";
import { EntityModuleShell } from "@/template";
import EmployeePicker from "@/components/common/employeePicker";
import InputField from "@/components/ui/inputField";
import ButtonField from "@/components/ui/buttonField";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import GridAction from "@/components/common/gridAction/gridAction";
import DetailSection from "@/components/common/detailSection";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const empty: TaxBracketModel = { lowerBound: 0, upperBound: null, ratePercent: 0, sortOrder: 0 };

/** HC231/HC232 — progressive income-tax table + the deductions preview. */
function TaxBracket() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [form, setForm] = useState<TaxBracketModel>({ ...empty });
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);
  const [preview, setPreview] = useState<{ id: string; name: string } | null>(null);

  const [param] = useState({ ...parameterInitialData, take: 100, sortCol: "sortOrder", dir: "asc" });
  const { data, isLoading } = useQuery({ queryKey: ["taxBrackets", param], queryFn: () => getAllTaxBrackets(param), placeholderData: keepPreviousData });
  const brackets = data?.data ?? [];

  const { data: deductions } = useQuery({ queryKey: ["deductions", preview?.id], queryFn: () => getDeductions(preview!.id), enabled: !!preview });

  const set = (p: Partial<TaxBracketModel>) => setForm((f) => ({ ...f, ...p }));
  const refresh = (m: string) => { setMsg(m); queryClient.invalidateQueries({ queryKey: ["taxBrackets"] }); if (preview) queryClient.invalidateQueries({ queryKey: ["deductions"] }); };

  const submit = async () => {
    setBusy(true);
    const res = await saveTaxBracket({
      ...form,
      lowerBound: Number(form.lowerBound ?? 0),
      upperBound: form.upperBound != null && String(form.upperBound) !== "" ? Number(form.upperBound) : null,
      ratePercent: Number(form.ratePercent ?? 0),
      sortOrder: Number(form.sortOrder ?? 0),
    });
    setBusy(false);
    refresh(res.message);
    if (res.ok) setForm({ ...empty });
  };

  const columns = useMemo(
    () =>
      [
        { name: "lowerBound", label: "From", render: (_t: unknown, b: TaxBracketModel) => <span className="tabular-nums">{money(b.lowerBound)}</span> },
        { name: "upperBound", label: "To", render: (_t: unknown, b: TaxBracketModel) => <span className="tabular-nums">{b.upperBound == null ? "∞" : money(b.upperBound)}</span> },
        { name: "ratePercent", label: "Rate", render: (_t: unknown, b: TaxBracketModel) => <span className="tabular-nums">{b.ratePercent}%</span> },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, b: TaxBracketModel) => (
            <GridAction
              id={b.id ?? ""}
              record={b}
              showAdd={false}
              showEdit
              showDelete
              editHandler={() => setForm({ ...b, upperBound: b.upperBound ?? null })}
              deleteHandler={(id: string) => deleteTaxBracket(id).then((r) => refresh(r.message))}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [],
  );

  return (
    <EntityModuleShell
      title="Income Tax & Deductions"
      headerDescription="Configure the progressive tax brackets and preview an employee's automated deductions"
      headerIcon={<Landmark className="h-6 w-6 text-primary" />}
      tableTitle="Income Tax & Deductions"
      hideAdd
      hideBack
      showForm={false}
      onList={() => undefined}
      onAdd={() => undefined}
    >
      <div className="m-2 flex min-h-0 flex-1 flex-col gap-3">
        {msg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}
        <div className="grid min-h-0 flex-1 grid-cols-1 gap-3 overflow-auto lg:grid-cols-2">
          <DetailSection title="Tax Brackets">
            <div className="mb-3 flex flex-wrap items-end gap-2">
              <div className="w-28"><InputField type="text" inputType="number" name="lowerBound" label="From" value={form.lowerBound ?? 0} onChange={(e) => set({ lowerBound: e.target.value as unknown as number })} /></div>
              <div className="w-28"><InputField type="text" inputType="number" name="upperBound" label="To (∞)" value={form.upperBound ?? ""} onChange={(e) => set({ upperBound: (e.target.value === "" ? null : e.target.value) as unknown as number })} /></div>
              <div className="w-24"><InputField type="text" inputType="number" name="ratePercent" label="Rate %" value={form.ratePercent ?? 0} onChange={(e) => set({ ratePercent: e.target.value as unknown as number })} /></div>
              <div className="w-20"><InputField type="text" inputType="number" name="sortOrder" label="Order" value={form.sortOrder ?? 0} onChange={(e) => set({ sortOrder: e.target.value as unknown as number })} /></div>
              <ButtonField value={form.id ? "Update" : "Add"} variant="primary" icon={form.id ? <Pencil size={14} /> : <Plus size={14} />} disabled={busy} onClick={submit} />
            </div>
            <DataTableProvider dataTable={{ columns, data: brackets, count: brackets.length, pagination: "None", search: "None", isLoading, key: "id" }} />
          </DetailSection>

          <DetailSection title="Deductions Preview">
            <div className="mb-2 flex items-center gap-1.5 text-xs text-muted"><Calculator size={14} className="text-primary" /> {t("Pick an employee to preview the automated deductions.")}</div>
            <EmployeePicker value={preview?.id} displayValue={preview?.name} onSelect={(id, name) => setPreview({ id, name })} placeholder={t("Pick an employee…")} />
            {preview && deductions && (
              <div className="mt-3 space-y-1.5 text-sm">
                <Row label={t("Gross pay")} value={money(deductions.grossPay)} />
                <Row label={t("Taxable gross")} value={money(deductions.taxableGross)} muted />
                <div className="my-1 border-t border-border/60" />
                {(deductions.lines ?? []).map((l, i) => (
                  <Row key={i} label={l.label ?? ""} value={`- ${money(l.amount)}`} sub />
                ))}
                <Row label={t("Total deductions")} value={`- ${money(deductions.totalDeductions)}`} />
                <div className="my-1 border-t border-border/60" />
                <Row label={t("Net pay")} value={money(deductions.netPay)} strong />
                <p className="pt-1 text-[11px] text-muted">{t("Employer contributions (info)")}: {money(deductions.employerBenefitContributions)}</p>
              </div>
            )}
          </DetailSection>
        </div>
      </div>
    </EntityModuleShell>
  );
}

function Row({ label, value, muted, sub, strong }: { label: string; value: string; muted?: boolean; sub?: boolean; strong?: boolean }) {
  return (
    <div className={`flex items-center justify-between ${sub ? "pl-3" : ""}`}>
      <span className={`${muted || sub ? "text-xs text-muted" : "text-foreground"}`}>{label}</span>
      <span className={`tabular-nums ${strong ? "text-base font-bold text-primary" : muted || sub ? "text-xs text-muted" : "font-semibold"}`}>{value}</span>
    </div>
  );
}

export default memo(TaxBracket);
