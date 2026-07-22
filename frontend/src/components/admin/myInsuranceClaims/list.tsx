"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getAllInsuranceClaims } from "@/services/admin/insurance";
import type { InsuranceClaimModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  onSelect: (id: string) => void;
}

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const statusBadge = (s?: string) =>
  ({ Pending: "bg-warning/15 text-warning", UnderReview: "bg-primary/15 text-primary", Approved: "bg-success/15 text-success", Rejected: "bg-error/15 text-error", Paid: "bg-secondary/40 text-foreground" }[s ?? ""] ?? "bg-muted/30 text-muted");

function MyInsuranceClaimList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "myInsuranceClaims", fetchPage: getAllInsuranceClaims });

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

  return <EntityListShell listKey="myInsuranceClaims" listLabel="My Insurance Claims" columns={columns} {...list} />;
}

export default MyInsuranceClaimList;
