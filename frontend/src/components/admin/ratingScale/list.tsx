"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllRatingScales, deleteRatingScale } from "@/services/admin/ratingScale";
import type { RatingScaleModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function RatingScaleList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "ratingScalesList",
    fetchPage: getAllRatingScales,
    deleteById: deleteRatingScale,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: RatingScaleModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "scoreType", label: "Score Type" },
        { name: "levels", label: "Levels", render: (v: unknown) => (Array.isArray(v) ? v.length : 0) },
        { name: "sortOrder", label: "Order" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: RatingScaleModel) => (
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
    <EntityListShell listKey="ratingScalesList" listLabel="Rating Scales" columns={columns} {...list} />
  );
}

export default RatingScaleList;
