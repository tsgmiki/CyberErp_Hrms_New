"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllCompetencyCategory from "@/services/admin/competencyCategory/getAll";
import deleteCompetencyCategory from "@/services/admin/competencyCategory/delete";
import type { CompetencyCategoryModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function CompetencyCategoryList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "competencyCategories",
    fetchPage: getAllCompetencyCategory,
    deleteById: deleteCompetencyCategory,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: CompetencyCategoryModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
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
          render: (_t: unknown, record: CompetencyCategoryModel) => (
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
    <EntityListShell listKey="competencyCategories" listLabel="Competency Categories" columns={columns} {...list} />
  );
}

export default CompetencyCategoryList;
