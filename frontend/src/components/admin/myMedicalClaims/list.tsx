"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getAllMedicalClaims } from "@/services/admin/medical";
import type { MedicalClaimModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { money, medicalClaimStatusBadge } from "./shared";

interface Props {
  onSelect: (id: string) => void;
}

function MyMedicalClaimList({ onSelect }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({ queryKey: "myMedicalClaims", fetchPage: getAllMedicalClaims });

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
        {
          name: "beneficiaryName",
          label: "Beneficiary",
          render: (text: string, r: MedicalClaimModel) => (
            <>
              {text} <span className="text-[10px] text-muted">({t(r.beneficiaryCategory ?? "")})</span>
            </>
          ),
        },
        { name: "serviceDate", label: "Service Date", render: (v: string) => v?.slice(0, 10) },
        { name: "claimedAmount", label: "Claimed", render: (_t: unknown, r: MedicalClaimModel) => <span className="tabular-nums">{money(r.claimedAmount)}</span> },
        { name: "approvedAmount", label: "Approved", render: (_t: unknown, r: MedicalClaimModel) => <span className="tabular-nums">{r.approvedAmount == null ? "—" : money(r.approvedAmount)}</span> },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${medicalClaimStatusBadge(text)}`}>{t(text ?? "")}</span>,
        },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return <EntityListShell listKey="myMedicalClaims" listLabel="My Medical Claims" columns={columns} {...list} />;
}

export default MyMedicalClaimList;
