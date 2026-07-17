"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllCompetency from "@/services/admin/competency/getAll";
import deleteCompetency from "@/services/admin/competency/delete";
import type { CompetencyModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function CompetencyList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "competencies",
    fetchPage: getAllCompetency,
    deleteById: deleteCompetency,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: CompetencyModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "competencyCategoryName", label: "Category", sort: true },
        { name: "description", label: "Description" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: CompetencyModel) => (
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
    <EntityListShell listKey="competencies" listLabel="Competencies" columns={columns} {...list} />
  );
}

export default CompetencyList;
