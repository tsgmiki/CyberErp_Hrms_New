"use client";
import { useMemo } from "react";
import { getAllLoans } from "@/services/admin/loan";
import type { LoanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { money, loanStatusBadge } from "../loan/shared";

interface Props {
  onSelect: (id: string) => void;
}

function MyLoanList({ onSelect }: Props) {
  const list = useEntityList({ queryKey: "myLoans", fetchPage: getAllLoans });

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

  return <EntityListShell listKey="myLoans" listLabel="My Loans" columns={columns} {...list} />;
}

export default MyLoanList;
