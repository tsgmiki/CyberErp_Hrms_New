"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
import deleteWorkLocation from "@/services/admin/workLocation/delete";
import type { WorkLocationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function WorkLocationList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "workLocations",
    fetchPage: getAllWorkLocation,
    deleteById: deleteWorkLocation,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: WorkLocationModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "name", label: "Name", sort: true },
        { name: "locationType", label: "Type", sort: true },
        { name: "parentName", label: "Parent" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: WorkLocationModel) => (
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
    <EntityListShell listKey="workLocations" listLabel="Work Locations" columns={columns} {...list} />
  );
}

export default WorkLocationList;
