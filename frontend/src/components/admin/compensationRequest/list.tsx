"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getCompensationRequests } from "@/services/admin/compensation";
import type { CompensationRequestModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  onSelect: (record: CompensationRequestModel) => void;
}

const STATUS_TONE: Record<string, string> = {
  Submitted: "bg-info/15 text-info",
  UnderReview: "bg-warning/15 text-warning",
  Resolved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
};

const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "Submitted", label: "Submitted" },
      { value: "UnderReview", label: "UnderReview" },
      { value: "Resolved", label: "Resolved" },
      { value: "Rejected", label: "Rejected" },
    ],
  },
];

function CompensationRequestList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "compensationRequests", fetchPage: getCompensationRequests });

  const columns = useMemo(
    () =>
      [
        {
          name: "subject",
          label: "Subject",
          sort: true,
          render: (text: string, r: CompensationRequestModel) => (
            <button type="button" onClick={() => onSelect(r)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee", sort: true },
        {
          name: "requestType",
          label: "Type",
          render: (v: string) => (v === "PayrollDiscrepancy" ? t("Discrepancy") : t("Benefit change")),
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text ?? ""] ?? "bg-muted/30 text-muted"}`}>{t(text ?? "")}</span>,
        },
        { name: "submittedOn", label: "Submitted", render: (v: string) => (v ? String(v).slice(0, 10) : "") },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="compensationRequests" listLabel="Compensation Requests" columns={columns} listFilters={STATUS_FILTER} {...list} />;
}

export default CompensationRequestList;
