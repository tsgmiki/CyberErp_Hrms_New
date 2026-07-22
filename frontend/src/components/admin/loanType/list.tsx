"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllLoanTypes from "@/services/admin/loanType/getAll";
import deleteLoanType from "@/services/admin/loanType/delete";
import type { LoanTypeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function LoanTypeList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "loanTypes",
    fetchPage: getAllLoanTypes,
    deleteById: deleteLoanType,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: LoanTypeModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        {
          name: "maxAmount",
          label: "Max Amount",
          render: (_t: unknown, r: LoanTypeModel) => (
            <span className="tabular-nums">{r.maxAmount == null ? "∞" : r.maxAmount.toLocaleString()}</span>
          ),
        },
        { name: "maxTermMonths", label: "Max Term (months)" },
        {
          name: "interestRatePct",
          label: "Interest",
          render: (_t: unknown, r: LoanTypeModel) => (r.interestRatePct ? `${r.interestRatePct}%` : "Free"),
        },
        {
          name: "requiresGuarantor",
          label: "Guarantor",
          render: (_t: unknown, r: LoanTypeModel) => (r.requiresGuarantor ? "Yes" : "No"),
        },
        {
          name: "isActive",
          label: "Status",
          render: (_t: unknown, r: LoanTypeModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${r.isActive ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
              {r.isActive ? "Active" : "Inactive"}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: LoanTypeModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="loanTypes" listLabel="Loan Types" columns={columns} {...list} />;
}

export default LoanTypeList;
