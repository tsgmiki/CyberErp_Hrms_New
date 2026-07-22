"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllMedicalProviders from "@/services/admin/medicalProvider/getAll";
import deleteMedicalProvider from "@/services/admin/medicalProvider/delete";
import type { MedicalProviderModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function MedicalProviderList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "medicalProviders",
    fetchPage: getAllMedicalProviders,
    deleteById: deleteMedicalProvider,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: MedicalProviderModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "providerType", label: "Type" },
        { name: "specialization", label: "Specialization" },
        { name: "phoneNumber", label: "Phone" },
        { name: "email", label: "Email" },
        {
          name: "isActive",
          label: "Active",
          render: (_t: unknown, r: MedicalProviderModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${r.isActive ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
              {r.isActive ? "Active" : "Inactive"}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: MedicalProviderModel) => (
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

  return <EntityListShell listKey="medicalProviders" listLabel="Medical Providers" columns={columns} {...list} />;
}

export default MedicalProviderList;
