import { memo, useMemo, useState } from "react";
import { BookOpenCheck, Calculator, Loader2 } from "lucide-react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
import getAllSetting from "@/services/admin/annualLeaveSetting/getAll";
import getAnnualLeaveLedger from "@/services/admin/annualLeaveLedger/get";
import calculateAnnualLeaveLedger from "@/services/admin/annualLeaveLedger/calculate";
import { parameterInitialData } from "@/constants/initialization";
import type { AnnualLeaveLedgerRow } from "@/models";

const num = (v?: number) => (v ?? 0).toLocaleString(undefined, { minimumFractionDigits: 1 });
const day = (v?: string) => (v ? String(v).slice(0, 10) : "—");

function AnnualLeaveLedger() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [settingId, setSettingId] = useState("");
  const queryClient = useQueryClient();

  const [settingParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: settings } = useQuery({
    queryKey: ["annualLeaveSettings", settingParam],
    queryFn: () => getAllSetting(settingParam),
  });
  const settingOptions = useMemo(() => settings?.data ?? [], [settings]);

  const { data: ledger, isLoading } = useQuery({
    queryKey: ["annualLeaveLedger", settingId],
    queryFn: () => getAnnualLeaveLedger(settingId),
    enabled: !!settingId,
  });

  const calculate = useMutation({
    mutationFn: () => calculateAnnualLeaveLedger(settingId),
    onSuccess: (r) => {
      window.alert(r?.message ?? "Ledger calculated.");
      queryClient.invalidateQueries({ queryKey: ["annualLeaveLedger", settingId] });
      queryClient.invalidateQueries({ queryKey: ["leaveBalances"] });
    },
    onError: () => window.alert("Failed to calculate the ledger."),
  });

  const body = (
    <div className="space-y-4">
      {/* Controls: setting selector + Calculate trigger */}
      <div className="flex flex-wrap items-end gap-3">
        <div className="min-w-[22rem] flex-1">
          <label className="mb-1 block text-sm font-medium text-muted">Annual Leave Setting</label>
          <select
            value={settingId}
            onChange={(e) => setSettingId(e.target.value)}
            className="h-9 w-full rounded-lg border border-border bg-background px-3 text-sm text-foreground outline-none focus:border-primary"
          >
            <option value="">Select a setting…</option>
            {settingOptions.map((s: any) => (
              <option key={s.id} value={s.id}>
                {s.fiscalYearName} — {s.leaveTypeName}
              </option>
            ))}
          </select>
        </div>
        <button
          type="button"
          disabled={!settingId || calculate.isPending || ledger?.fiscalYearClosed}
          onClick={() => calculate.mutate()}
          className="inline-flex h-9 items-center gap-2 rounded-lg bg-primary px-4 text-sm font-medium text-on-accent disabled:opacity-50"
          title={ledger?.fiscalYearClosed ? "Fiscal year is closed" : "Generate entitlements for all eligible employees"}
        >
          {calculate.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Calculator className="h-4 w-4" />}
          Calculate
        </button>
      </div>

      {!settingId ? (
        <div className="rounded-md border border-dashed border-border p-8 text-center text-sm text-muted">
          Select an annual leave setting to preview and calculate the ledger.
        </div>
      ) : isLoading ? (
        <div className="p-4 text-sm text-muted">Loading ledger…</div>
      ) : ledger ? (
        <>
          <div className="flex flex-wrap gap-4 rounded-lg border border-border bg-muted/20 px-4 py-2 text-sm text-foreground">
            <span>Fiscal Year: <b>{ledger.fiscalYearName}</b> ({day(ledger.fiscalYearStart)} → {day(ledger.fiscalYearEnd)})</span>
            <span>Leave Type: <b>{ledger.leaveTypeName}</b></span>
            <span>Employees: <b>{ledger.generatedCount}/{ledger.totalEmployees}</b> generated</span>
            {ledger.fiscalYearClosed && <span className="font-medium text-amber-600">Fiscal year closed</span>}
          </div>

          <div className="overflow-x-auto rounded-lg border border-border">
            <table className="w-full text-sm">
              <thead className="bg-muted/30 text-left text-muted">
                <tr>
                  <th className="px-3 py-2">Employee</th>
                  <th className="px-3 py-2">Hire Date</th>
                  <th className="px-3 py-2 text-right">Service (yrs)</th>
                  <th className="px-3 py-2 text-right">Calculated</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2 text-right">Entitled</th>
                  <th className="px-3 py-2 text-right">Carried</th>
                  <th className="px-3 py-2 text-right">Adjusted</th>
                  <th className="px-3 py-2 text-right">Taken</th>
                  <th className="px-3 py-2 text-right">Available</th>
                </tr>
              </thead>
              <tbody>
                {ledger.rows.map((r: AnnualLeaveLedgerRow) => (
                  <tr key={r.employeeId} className="border-t border-border">
                    <td className="px-3 py-2">
                      <span className="font-medium">{r.employeeName}</span>{" "}
                      <span className="text-muted">({r.employeeNumber})</span>
                      {r.isManagerial && <span className="ml-1 rounded bg-primary/10 px-1.5 text-[11px] text-primary">Mgr</span>}
                    </td>
                    <td className="px-3 py-2">{day(r.hireDate)}</td>
                    <td className="px-3 py-2 text-right">{r.serviceYears}</td>
                    <td className="px-3 py-2 text-right font-semibold">{num(r.calculatedEntitlement)}</td>
                    <td className="px-3 py-2">
                      {r.isGenerated ? (
                        <span className="rounded-full bg-emerald-500/15 px-2 py-0.5 text-xs text-emerald-600">Generated</span>
                      ) : (
                        <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-xs text-amber-600">Pending</span>
                      )}
                    </td>
                    <td className="px-3 py-2 text-right">{r.isGenerated ? num(r.entitled) : "—"}</td>
                    <td className="px-3 py-2 text-right">{r.isGenerated ? num(r.carriedForward) : "—"}</td>
                    <td className="px-3 py-2 text-right">{r.isGenerated ? num(r.adjusted) : "—"}</td>
                    <td className="px-3 py-2 text-right">{r.isGenerated ? num(r.taken) : "—"}</td>
                    <td className="px-3 py-2 text-right font-semibold text-primary">{r.isGenerated ? num(r.available) : "—"}</td>
                  </tr>
                ))}
                {ledger.rows.length === 0 && (
                  <tr><td colSpan={10} className="px-3 py-6 text-center text-muted">No active employees found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </>
      ) : null}
    </div>
  );

  return (
    <EntityModuleShell
      title="Annual Leave Ledger"
      headerDescription="Calculate service-based leave entitlements for all eligible employees"
      headerIcon={<BookOpenCheck className="h-6 w-6 text-primary" />}
      tableTitle="Annual Leave Ledger"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      hideAdd
      form={<div />}
      list={body}
    />
  );
}

export default memo(AnnualLeaveLedger);
