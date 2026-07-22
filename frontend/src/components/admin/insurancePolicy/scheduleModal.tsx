"use client";
import { memo, useCallback, useMemo, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ListPlus } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import FormProviders from "@/components/common/formProvider/formProvider";
import GridAction from "../../common/gridAction/gridAction";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import {
  getInsurancePolicy, generatePremiumSchedule, addPremiumSchedule, removePremiumSchedule, payPremium,
} from "@/services/admin/insurance";

const FormProvider = memo(FormProviders);
const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

function ScheduleModal({ policyId, onClose }: { policyId: string; onClose: () => void }) {
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();
  const [row, setRow] = useState({ dueDate: "", amount: "" });
  const [busy, setBusy] = useState(false);

  const { data: policy, isLoading } = useQuery({ queryKey: ["insurancePolicy", policyId], queryFn: () => getInsurancePolicy(policyId) });
  const schedule = policy?.schedule ?? [];

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["insurancePolicy", policyId] });
    queryClient.invalidateQueries({ queryKey: ["insurancePolicies"] });
  };
  const act = async (fn: () => Promise<unknown>) => { setBusy(true); await fn(); setBusy(false); refresh(); };

  const changeHandler = useCallback((e: any) => setRow((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const addHandler = async (e: any) => {
    e.preventDefault();
    if (!row.dueDate || row.amount === "") return;
    await act(() => addPremiumSchedule({ insurancePolicyId: policyId, dueDate: row.dueDate, amount: Number(row.amount) }));
    setRow({ dueDate: "", amount: "" });
    formRef.current?.reset();
  };

  const columns = useMemo(
    () =>
      [
        { name: "installment", label: "#" },
        { name: "dueDate", label: "Due Date", render: (t: string) => t?.slice(0, 10) },
        { name: "amount", label: "Amount", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.amount)}</span> },
        {
          name: "status", label: "Status",
          render: (t: string, r: any) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${t === "Paid" ? "bg-success/15 text-success" : "bg-warning/15 text-warning"}`}>
              {t}{r.paymentReference ? ` · ${r.paymentReference}` : ""}
            </span>
          ),
        },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: any) =>
            r.status === "Paid" ? null : (
              <GridAction
                id={r.id}
                record={r}
                showAdd={false}
                showEdit={false}
                showDelete
                showAction
                actionName="Mark Paid"
                actionHandler={(id) => act(() => payPremium(id))}
                deleteHandler={(id) => act(() => removePremiumSchedule(id))}
              />
            ),
        },
      ] as DataTableColumnModel[],
    [],
  );

  return (
    <DialogModal title={`Premium Schedule — ${policy?.policyNumber ?? ""}`} visible onClose={onClose} hideOk cancelLabel="Close">
      <div className="space-y-3">
        <div className="flex flex-wrap items-center gap-4 text-sm">
          <span className="text-muted">Annual premium: <b className="tabular-nums text-foreground">{money(policy?.annualPremium)}</b></span>
          <span className="text-success">Paid: <b className="tabular-nums">{money(policy?.premiumPaid)}</b></span>
          <span className="text-warning">Outstanding: <b className="tabular-nums">{money(policy?.premiumOutstanding)}</b></span>
          <button type="button" disabled={busy} onClick={() => act(() => generatePremiumSchedule(policyId))} className="ml-auto inline-flex items-center gap-1.5 rounded-md border border-border px-2.5 py-1 text-xs hover:bg-secondary/30 disabled:opacity-50">
            <ListPlus size={13} /> Generate from frequency
          </button>
        </div>

        <DataTableProvider
          dataTable={{ columns, data: schedule, count: schedule.length, pagination: "None", search: "None", isLoading, key: "id" }}
        />

        <FormProvider
          ref={formRef}
          form={{
            columnsNo: 2,
            submitHandler: addHandler,
            labelWidth: "w-[35%]",
            isPending: busy,
            SubmitButton: "bottom",
            submitBtnTitle: "Add row",
            components: [
              { name: "dueDate", label: "Due Date", type: "date", value: row.dueDate, onChange: changeHandler },
              { name: "amount", label: "Amount", value: row.amount, onChange: changeHandler, inputType: "number", type: "text" },
            ],
          }}
        />
      </div>
    </DialogModal>
  );
}

export default memo(ScheduleModal);
