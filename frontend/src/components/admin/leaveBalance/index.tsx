import { lazy, memo, useMemo, useState } from "react";
import { Wallet } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
import getAllEmployee from "@/services/admin/employee/getAll";
import getLeaveBalances from "@/services/admin/leaveBalance/getByEmployee";
import { parameterInitialData } from "@/constants/initialization";

const LeaveBalanceForm = memo(lazy(() => import("./form")));

const num = (v?: number) => (v ?? 0).toLocaleString(undefined, { minimumFractionDigits: 1 });

function LeaveBalances() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [employeeId, setEmployeeId] = useState("");
  const queryClient = useQueryClient();

  const [empParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: employees } = useQuery({
    queryKey: ["employees", empParam],
    queryFn: () => getAllEmployee(empParam),
  });
  const emps = useMemo(() => employees?.data ?? [], [employees]);
  const employeeName = emps.find((e: any) => e.id === employeeId)?.fullName;

  const { data: balances, isLoading } = useQuery({
    queryKey: ["leaveBalances", employeeId],
    queryFn: () => getLeaveBalances(employeeId),
    enabled: !!employeeId,
  });

  const grid = (
    <div className="space-y-4">
      <div className="max-w-md">
        <label className="mb-1 block text-sm font-medium text-muted">Employee</label>
        <select
          value={employeeId}
          onChange={(e) => setEmployeeId(e.target.value)}
          className="h-9 w-full rounded-lg border border-border bg-background px-3 text-sm text-foreground outline-none focus:border-primary"
        >
          <option value="">Select an employee…</option>
          {emps.map((e: any) => (
            <option key={e.id} value={e.id}>{e.fullName || e.employeeNumber}</option>
          ))}
        </select>
      </div>

      {!employeeId ? (
        <div className="rounded-md border border-dashed border-border p-8 text-center text-sm text-muted">
          Select an employee to view their leave balances.
        </div>
      ) : isLoading ? (
        <div className="p-4 text-sm text-muted">Loading…</div>
      ) : (balances?.length ?? 0) === 0 ? (
        <div className="rounded-md border border-dashed border-border p-8 text-center text-sm text-muted">
          No balances yet. Use “Set / Adjust” to open a balance, or they materialize on first approved leave.
        </div>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-border">
          <table className="w-full text-sm">
            <thead className="bg-muted/30 text-left text-muted">
              <tr>
                <th className="px-3 py-2">Leave Type</th><th className="px-3 py-2">Fiscal Year</th>
                <th className="px-3 py-2 text-right">Entitled</th><th className="px-3 py-2 text-right">Carried</th>
                <th className="px-3 py-2 text-right">Adjusted</th><th className="px-3 py-2 text-right">Taken</th>
                <th className="px-3 py-2 text-right">Available</th>
              </tr>
            </thead>
            <tbody>
              {(balances ?? []).map((b) => (
                <tr key={b.id} className="border-t border-border">
                  <td className="px-3 py-2 font-medium">{b.leaveTypeName} <span className="text-muted">({b.leaveTypeCode})</span></td>
                  <td className="px-3 py-2">{b.fiscalYearName}</td>
                  <td className="px-3 py-2 text-right">{num(b.entitled)}</td>
                  <td className="px-3 py-2 text-right">{num(b.carriedForward)}</td>
                  <td className="px-3 py-2 text-right">{num(b.adjusted)}</td>
                  <td className="px-3 py-2 text-right">{num(b.taken)}</td>
                  <td className="px-3 py-2 text-right font-semibold text-primary">{num(b.available)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  return (
    <EntityModuleShell
      title="Leave Balances"
      headerDescription="View and set employee leave entitlements (ledger-backed)"
      headerIcon={<Wallet className="h-6 w-6 text-primary" />}
      tableTitle="Leave Balances"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      hideAdd={!employeeId}
      form={
        <LeaveBalanceForm
          employeeId={employeeId}
          employeeName={employeeName}
          onSaved={() => {
            queryClient.invalidateQueries({ queryKey: ["leaveBalances", employeeId] });
            backHandler();
          }}
        />
      }
      list={grid}
    />
  );
}

export default LeaveBalances;
