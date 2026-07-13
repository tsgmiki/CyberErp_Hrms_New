"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllForms, deleteForm } from "@/services/admin/dynamicForm";
import type { DynamicFormModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function FormBuilderList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "dynamicFormsList",
    fetchPage: getAllForms,
    deleteById: deleteForm,
    initialParam: { module: "Employee" },
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "label",
          label: "Tab Label",
          sort: true,
          render: (text: string, record: DynamicFormModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "name", label: "Key" },
        { name: "fields", label: "Fields", render: (v: unknown) => (Array.isArray(v) ? v.length : 0) },
        { name: "sortOrder", label: "Order" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: DynamicFormModel) => (
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

  return <EntityListShell listKey="dynamicFormsList" listLabel="Custom Forms" columns={columns} {...list} />;
}

export default FormBuilderList;
