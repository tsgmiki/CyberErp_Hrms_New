"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllJobCategory from "@/services/admin/jobCategory/getAll";
import deleteJobCategory from "@/services/admin/jobCategory/delete";
import type { JobCategoryModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function JobCategoryList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "jobCategories",
    fetchPage: getAllJobCategory,
    deleteById: deleteJobCategory,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: JobCategoryModel) => (
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
        { name: "description", label: "Description" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: JobCategoryModel) => (
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
    <EntityListShell listKey="jobCategories" listLabel="Job Categories" columns={columns} {...list} />
  );
}

export default JobCategoryList;
