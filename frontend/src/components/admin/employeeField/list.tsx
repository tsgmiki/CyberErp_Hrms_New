"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllEmployeeField from "@/services/admin/employeeField/getAll";
import deleteEmployeeField from "@/services/admin/employeeField/delete";
import type { EmployeeFieldModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function EmployeeFieldList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "employeeFields",
    fetchPage: getAllEmployeeField,
    deleteById: deleteEmployeeField,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "label",
          label: "Label",
          sort: true,
          render: (text: string, record: EmployeeFieldModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "name", label: "Field Key" },
        { name: "dataType", label: "Data Type" },
        {
          name: "isRequired",
          label: "Required",
          render: (v: boolean) => (v ? "Yes" : "No"),
        },
        {
          name: "isActive",
          label: "Active",
          render: (v: boolean) => (v ? "Yes" : "No"),
        },
        { name: "sortOrder", label: "Order" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: EmployeeFieldModel) => (
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

  return (
    <EntityListShell listKey="employeeFields" listLabel="Employee Fields" columns={columns} {...list} />
  );
}

export default EmployeeFieldList;
