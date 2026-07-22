"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllTrainingCategory from "@/services/admin/trainingCategory/getAll";
import deleteTrainingCategory from "@/services/admin/trainingCategory/delete";
import type { TrainingCategoryModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function TrainingCategoryList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "trainingCategories",
    fetchPage: getAllTrainingCategory,
    deleteById: deleteTrainingCategory,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: TrainingCategoryModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
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
          render: (_t: unknown, record: TrainingCategoryModel) => (
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
    <EntityListShell listKey="trainingCategories" listLabel="Training Categories" columns={columns} {...list} />
  );
}

export default TrainingCategoryList;
