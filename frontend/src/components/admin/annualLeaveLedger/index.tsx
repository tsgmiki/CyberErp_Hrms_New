import { memo, useMemo, useState } from "react";
import { BookOpenCheck, Calculator, Loader2 } from "lucide-react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { EntityModuleShell, useEntityCrudModule, EntityListShell } from "@/template";
import getAllSetting from "@/services/admin/annualLeaveSetting/getAll";
import getAnnualLeaveLedger from "@/services/admin/annualLeaveLedger/get";
import calculateAnnualLeaveLedger from "@/services/admin/annualLeaveLedger/calculate";
import { parameterInitialData } from "@/constants/initialization";
import type { AnnualLeaveLedgerRow } from "@/models";
import type ParameterModel from "@/models/ParameterModel";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListDisplayMode } from "@/components/common/dataTableProvider/listViewToolbar";

const num = (v?: number) => (v ?? 0).toLocaleString(undefined, { minimumFractionDigits: 1 });
const day = (v?: string) => (v ? String(v).slice(0, 10) : "—");
const rightNum = (v: number | undefined, generated: boolean, cls = "") => (
  <span className={`block text-right ${cls}`}>{generated ? num(v) : "—"}</span>
);

// Standard grid columns — same DataTableColumnModel shape every module list uses.
const COLUMNS: DataTableColumnModel[] = [
  {
    name: "employeeName",
    label: "Employee",
    sort: true,
    gridPrimary: true,
    render: (_v, r: AnnualLeaveLedgerRow) => (
      <span>
        <span className="font-medium">{r.employeeName}</span>{" "}
        <span className="text-muted">({r.employeeNumber})</span>
        {r.isManagerial && <span className="ml-1 rounded bg-primary/10 px-1.5 text-[11px] text-primary">Mgr</span>}
      </span>
    ),
  },
  { name: "hireDate", label: "Hire Date", render: (_v, r: AnnualLeaveLedgerRow) => day(r.hireDate) },
  { name: "serviceYears", label: "Service (yrs)", render: (_v, r: AnnualLeaveLedgerRow) => <span className="block text-right">{r.serviceYears}</span> },
  { name: "calculatedEntitlement", label: "Calculated", render: (_v, r: AnnualLeaveLedgerRow) => <span className="block text-right font-semibold">{num(r.calculatedEntitlement)}</span> },
  {
    name: "isGenerated",
    label: "Status",
    render: (_v, r: AnnualLeaveLedgerRow) =>
      r.isGenerated ? (
        <span className="rounded-full bg-emerald-500/15 px-2 py-0.5 text-xs text-emerald-600">Generated</span>
      ) : (
        <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-xs text-amber-600">Pending</span>
      ),
  },
  { name: "entitled", label: "Entitled", render: (_v, r: AnnualLeaveLedgerRow) => rightNum(r.entitled, r.isGenerated) },
  { name: "carriedForward", label: "Carried", render: (_v, r: AnnualLeaveLedgerRow) => rightNum(r.carriedForward, r.isGenerated) },
  { name: "adjusted", label: "Adjusted", render: (_v, r: AnnualLeaveLedgerRow) => rightNum(r.adjusted, r.isGenerated) },
  { name: "taken", label: "Taken", render: (_v, r: AnnualLeaveLedgerRow) => rightNum(r.taken, r.isGenerated) },
  { name: "available", label: "Available", render: (_v, r: AnnualLeaveLedgerRow) => rightNum(r.available, r.isGenerated, "font-semibold text-primary") },
];

function AnnualLeaveLedger() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [settingId, setSettingId] = useState("");
  const queryClient = useQueryClient();

  // Grid state (single bulk fetch → one page; grouping + collapse is the render optimization).
  const [param, setParam] = useState<ParameterModel>({ ...parameterInitialData, take: 1000 });
  const [displayMode, setDisplayMode] = useState<ListDisplayMode>("list");

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

  const rows = ledger?.rows ?? [];

  const body = (
    <div className="flex h-full min-h-0 flex-col gap-3">
      {/* Controls: setting selector + Calculate trigger */}
      <div className="flex shrink-0 flex-wrap items-end gap-3">
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
      ) : (
        <EntityListShell
          listKey="annualLeaveLedger"
          listLabel="Annual Leave Ledger"
          columns={COLUMNS}
          isLoading={isLoading}
          rows={rows}
          total={rows.length}
          param={param}
          setParam={setParam}
          displayMode={displayMode}
          setDisplayMode={setDisplayMode}
          fetchAllData={async () => rows as unknown as Record<string, unknown>[]}
          groupBy="organizationUnitName"
          getGroupLabel={(key) => key || "Unassigned"}
          rowKey="employeeId"
          className="flex min-h-0 flex-1 flex-col gap-3"
          header={
            ledger ? (
              <div className="flex shrink-0 flex-wrap gap-4 rounded-lg border border-border bg-muted/20 px-4 py-2 text-sm text-foreground">
                <span>Fiscal Year: <b>{ledger.fiscalYearName}</b> ({day(ledger.fiscalYearStart)} → {day(ledger.fiscalYearEnd)})</span>
                <span>Leave Type: <b>{ledger.leaveTypeName}</b></span>
                <span>Employees: <b>{ledger.generatedCount}/{ledger.totalEmployees}</b> generated</span>
                {ledger.fiscalYearClosed && <span className="font-medium text-amber-600">Fiscal year closed</span>}
              </div>
            ) : undefined
          }
        />
      )}
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
