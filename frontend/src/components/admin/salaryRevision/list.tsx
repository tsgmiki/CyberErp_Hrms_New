"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getAllSalaryRevisions } from "@/services/admin/compensation";
import type { SalaryRevisionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { money, revisionStatusBadge } from "./shared";

interface Props {
  onSelect: (id: string) => void;
}

function SalaryRevisionList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "salaryRevisions", fetchPage: getAllSalaryRevisions });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, r: SalaryRevisionModel) => (
            <button type="button" onClick={() => r.id && onSelect(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        {
          name: "revisionType",
          label: "Type",
          render: (_t: unknown, r: SalaryRevisionModel) => (
            <span className="text-muted">{r.revisionType} · {r.basis === "Percentage" ? `${r.rate}%` : `+${money(r.rate)}`}</span>
          ),
        },
        { name: "employeeCount", label: "Employees", render: (_t: unknown, r: SalaryRevisionModel) => <span className="tabular-nums">{r.employeeCount}</span> },
        {
          name: "totalIncrease",
          label: "Increase",
          render: (_t: unknown, r: SalaryRevisionModel) => <span className="tabular-nums text-primary">+{money(r.totalIncrease)} ({r.averagePercent}%)</span>,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${revisionStatusBadge(text)}`}>{t(text ?? "")}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="salaryRevisions" listLabel="Salary Revisions" columns={columns} {...list} />;
}

export default SalaryRevisionList;
