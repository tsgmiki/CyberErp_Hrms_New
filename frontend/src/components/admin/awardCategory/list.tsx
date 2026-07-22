"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllAwardCategory from "@/services/admin/awardCategory/getAll";
import deleteAwardCategory from "@/services/admin/awardCategory/delete";
import type { AwardCategoryModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function AwardCategoryList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "awardCategories",
    fetchPage: getAllAwardCategory,
    deleteById: deleteAwardCategory,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: AwardCategoryModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "criteria", label: "Criteria" },
        { name: "sortOrder", label: "Order", sort: true },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: AwardCategoryModel) => (
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
    <EntityListShell listKey="awardCategories" listLabel="Award Categories" columns={columns} {...list} />
  );
}

export default AwardCategoryList;
