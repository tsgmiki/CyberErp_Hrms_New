"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllRecognitionBadge from "@/services/admin/recognitionBadge/getAll";
import deleteRecognitionBadge from "@/services/admin/recognitionBadge/delete";
import type { RecognitionBadgeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function RecognitionBadgeList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "recognitionBadges",
    fetchPage: getAllRecognitionBadge,
    deleteById: deleteRecognitionBadge,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: RecognitionBadgeModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="flex items-center gap-2 font-semibold">
              <span className="inline-block h-3 w-3 rounded-full" style={{ backgroundColor: record.color || "var(--color-primary, #888)" }} />
              {text}
            </button>
          ),
        },
        { name: "description", label: "Description" },
        { name: "sortOrder", label: "Order", sort: true },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: RecognitionBadgeModel) => (
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
    <EntityListShell listKey="recognitionBadges" listLabel="Recognition Badges" columns={columns} {...list} />
  );
}

export default RecognitionBadgeList;
