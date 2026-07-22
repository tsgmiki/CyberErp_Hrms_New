"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllBranch from "@/services/admin/branch/getAll";
import deleteBranch from "@/services/admin/branch/delete";
import type { BranchModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function BranchList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "branches",
    fetchPage: getAllBranch,
    deleteById: deleteBranch,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: BranchModel) => (
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
        { name: "parentName", label: "Parent Branch" },
        {
          name: "isHeadOffice",
          label: "Head Office",
          render: (v: boolean) => (v ? "Yes" : "No"),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: BranchModel) => (
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
    <EntityListShell listKey="branches" listLabel="Branches" columns={columns} {...list} />
  );
}

export default BranchList;
