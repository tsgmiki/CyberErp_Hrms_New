"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllMedicalContracts from "@/services/admin/medicalContract/getAll";
import deleteMedicalContract from "@/services/admin/medicalContract/delete";
import type { MedicalContractModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const money = (n?: number | null) => (n == null ? "—" : n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
const statusBadge = (s?: string) => ({ Active: "bg-success/15 text-success", Terminated: "bg-error/15 text-error", Expired: "bg-muted/30 text-muted" }[s ?? ""] ?? "bg-muted/30 text-muted");

interface Props {
  editHandler: (id: string) => void;
}

function MedicalContractList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "medicalContracts",
    fetchPage: getAllMedicalContracts,
    deleteById: deleteMedicalContract,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "providerName",
          label: "Provider",
          sort: true,
          render: (text: string, record: MedicalContractModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "contractNumber", label: "Contract #" },
        {
          name: "period",
          label: "Period",
          render: (_t: unknown, r: MedicalContractModel) => `${r.startDate?.slice(0, 10) ?? "?"} → ${r.endDate?.slice(0, 10) ?? "—"}`,
        },
        {
          name: "creditLimit",
          label: "Credit Limit",
          render: (_t: unknown, r: MedicalContractModel) => <span className="tabular-nums">{money(r.creditLimit)}</span>,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${statusBadge(text)}`}>{text}</span>,
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: MedicalContractModel) => (
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

  return <EntityListShell listKey="medicalContracts" listLabel="Service Contracts" columns={columns} {...list} />;
}

export default MedicalContractList;
