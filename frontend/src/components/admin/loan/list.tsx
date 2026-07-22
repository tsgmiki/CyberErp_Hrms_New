"use client";
import { useMemo } from "react";
import { getAllLoans } from "@/services/admin/loan";
import type { LoanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import { money, loanStatusBadge } from "./shared";

interface Props {
  onSelect: (id: string) => void;
}

const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "Requested", label: "Requested" },
      { value: "Approved", label: "Approved" },
      { value: "Active", label: "Active" },
      { value: "Settled", label: "Settled" },
      { value: "Rejected", label: "Rejected" },
      { value: "Cancelled", label: "Cancelled" },
    ],
  },
];

function LoanList({ onSelect }: Props) {
  const list = useEntityList({ queryKey: "loans", fetchPage: getAllLoans });

  const columns = useMemo(
    () =>
      [
        {
          name: "loanNumber",
          label: "Loan #",
          sort: true,
          render: (text: string, r: LoanModel) => (
            <button type="button" onClick={() => r.id && onSelect(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee", sort: true },
        { name: "loanTypeName", label: "Type" },
        { name: "principalAmount", label: "Principal", render: (_t: unknown, r: LoanModel) => <span className="tabular-nums">{money(r.principalAmount)}</span> },
        { name: "monthlyInstallment", label: "Monthly", render: (_t: unknown, r: LoanModel) => <span className="tabular-nums">{money(r.monthlyInstallment)}</span> },
        { name: "outstandingBalance", label: "Outstanding", render: (_t: unknown, r: LoanModel) => <span className="tabular-nums">{money(r.outstandingBalance)}</span> },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${loanStatusBadge(text)}`}>{text}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect],
  );

  return <EntityListShell listKey="loans" listLabel="Employee Loans" columns={columns} listFilters={STATUS_FILTER} {...list} />;
}

export default LoanList;
