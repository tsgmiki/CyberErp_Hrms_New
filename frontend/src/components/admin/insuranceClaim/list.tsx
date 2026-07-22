"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getAllInsuranceClaims } from "@/services/admin/insurance";
import type { InsuranceClaimModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  onSelect: (id: string) => void;
}

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const statusBadge = (s?: string) =>
  ({
    Pending: "bg-warning/15 text-warning",
    UnderReview: "bg-primary/15 text-primary",
    Approved: "bg-success/15 text-success",
    Rejected: "bg-error/15 text-error",
    Paid: "bg-secondary/40 text-foreground",
  }[s ?? ""] ?? "bg-muted/30 text-muted");

const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "Pending", label: "Pending" },
      { value: "UnderReview", label: "UnderReview" },
      { value: "Approved", label: "Approved" },
      { value: "Rejected", label: "Rejected" },
      { value: "Paid", label: "Paid" },
    ],
  },
];

function InsuranceClaimList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "insuranceClaims", fetchPage: getAllInsuranceClaims });

  const columns = useMemo(
    () =>
      [
        {
          name: "claimNumber",
          label: "Claim #",
          sort: true,
          render: (text: string, r: InsuranceClaimModel) => (
            <button type="button" onClick={() => r.id && onSelect(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee", sort: true },
        { name: "policyNumber", label: "Policy" },
        { name: "claimType", label: "Type", render: (v: string) => t(v ?? "") },
        { name: "incidentDate", label: "Incident", render: (v: string) => v?.slice(0, 10) },
        { name: "claimedAmount", label: "Claimed", render: (_t: unknown, r: InsuranceClaimModel) => <span className="tabular-nums">{money(r.claimedAmount)}</span> },
        { name: "approvedAmount", label: "Approved", render: (_t: unknown, r: InsuranceClaimModel) => <span className="tabular-nums">{r.approvedAmount == null ? "—" : money(r.approvedAmount)}</span> },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${statusBadge(text)}`}>{t(text ?? "")}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="insuranceClaims" listLabel="Insurance Claims" columns={columns} listFilters={STATUS_FILTER} {...list} />;
}

export default InsuranceClaimList;
