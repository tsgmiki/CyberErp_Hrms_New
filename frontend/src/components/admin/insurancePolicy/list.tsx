"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllInsurancePolicies from "@/services/admin/insurancePolicy/getAll";
import deleteInsurancePolicy from "@/services/admin/insurancePolicy/delete";
import type { InsurancePolicyModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const statusBadge = (s?: string) => ({ Active: "bg-success/15 text-success", Renewed: "bg-secondary/40 text-foreground", Expired: "bg-muted/30 text-muted", Cancelled: "bg-error/15 text-error" }[s ?? ""] ?? "bg-muted/30 text-muted");

interface Props {
  editHandler: (id: string) => void;
  scheduleHandler: (id: string) => void;
}

function InsurancePolicyList({ editHandler, scheduleHandler }: Props) {
  const list = useEntityList({
    queryKey: "insurancePolicies",
    fetchPage: getAllInsurancePolicies,
    deleteById: deleteInsurancePolicy,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "policyNumber",
          label: "Policy #",
          sort: true,
          render: (text: string, record: InsurancePolicyModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
              {record.isRenewal ? <span className="ml-1 text-[10px] text-muted">(renewal)</span> : null}
            </button>
          ),
        },
        { name: "insurerName", label: "Insurer", sort: true },
        { name: "insuranceType", label: "Type" },
        {
          name: "period",
          label: "Period",
          render: (_t: unknown, r: InsurancePolicyModel) => `${r.startDate?.slice(0, 10) ?? "?"} → ${r.endDate?.slice(0, 10) ?? "—"}`,
        },
        {
          name: "annualPremium",
          label: "Annual Premium",
          render: (_t: unknown, r: InsurancePolicyModel) => <span className="tabular-nums">{money(r.annualPremium)}</span>,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${statusBadge(text)}`}>{text}</span>,
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: InsurancePolicyModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              showAction
              actionName="Premium Schedule"
              actionHandler={(id) => scheduleHandler(id)}
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, scheduleHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="insurancePolicies" listLabel="Insurance Policies" columns={columns} {...list} />;
}

export default InsurancePolicyList;
