"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import deletePositionClass from "@/services/admin/positionClass/delete";
import type { PositionClassModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function PositionClassList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "positionClasses",
    fetchPage: getAllPositionClass,
    deleteById: deletePositionClass,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: PositionClassModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "title", label: "Title", sort: true },
        { name: "jobGradeName", label: "Grade" },
        {
          name: "salary",
          label: "Salary",
          render: (_t: unknown, record: PositionClassModel) =>
            record.salary != null
              ? Number(record.salary).toLocaleString(undefined, { minimumFractionDigits: 2 })
              : "",
        },
        { name: "jobCategoryName", label: "Category" },
        { name: "allocatedHeadcount", label: "Headcount" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: PositionClassModel) => (
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
    <EntityListShell listKey="positionClasses" listLabel="Position Classes" columns={columns} {...list} />
  );
}

export default PositionClassList;
