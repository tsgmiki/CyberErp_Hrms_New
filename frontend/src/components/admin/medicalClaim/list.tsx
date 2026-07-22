"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getAllMedicalClaims } from "@/services/admin/medical";
import type { MedicalClaimModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import { money, medicalStatusBadge } from "./shared";

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
      { value: "Pending", label: "Pending" },
      { value: "UnderReview", label: "UnderReview" },
      { value: "Approved", label: "Approved" },
      { value: "Rejected", label: "Rejected" },
      { value: "Paid", label: "Paid" },
    ],
  },
];

function MedicalClaimList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "medicalClaims", fetchPage: getAllMedicalClaims });

  const columns = useMemo(
    () =>
      [
        {
          name: "claimNumber",
          label: "Claim #",
          sort: true,
          render: (text: string, r: MedicalClaimModel) => (
            <button type="button" onClick={() => r.id && onSelect(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee", sort: true },
        {
          name: "beneficiaryName",
          label: "Beneficiary",
          render: (text: string, r: MedicalClaimModel) => (
            <>
              {text} <span className="text-[10px]">({t(r.beneficiaryCategory ?? "")})</span>
            </>
          ),
        },
        { name: "serviceDate", label: "Service date", render: (text: string) => text?.slice(0, 10) },
        { name: "claimedAmount", label: "Claimed", render: (_t: unknown, r: MedicalClaimModel) => <span className="tabular-nums">{money(r.claimedAmount)}</span> },
        { name: "approvedAmount", label: "Approved", render: (_t: unknown, r: MedicalClaimModel) => <span className="tabular-nums">{r.approvedAmount == null ? "—" : money(r.approvedAmount)}</span> },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${medicalStatusBadge(text)}`}>{t(text ?? "")}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="medicalClaims" listLabel="Medical Claims" columns={columns} listFilters={STATUS_FILTER} {...list} />;
}

export default MedicalClaimList;
