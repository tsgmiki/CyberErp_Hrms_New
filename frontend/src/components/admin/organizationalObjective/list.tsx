"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllOrganizationalObjective from "@/services/admin/organizationalObjective/getAll";
import deleteOrganizationalObjective from "@/services/admin/organizationalObjective/delete";
import type { OrganizationalObjectiveModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function OrganizationalObjectiveList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "organizationalObjectives",
    fetchPage: getAllOrganizationalObjective,
    deleteById: deleteOrganizationalObjective,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "title",
          label: "Title",
          sort: true,
          render: (text: string, record: OrganizationalObjectiveModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "reviewCycleName", label: "Review Cycle" },
        { name: "organizationUnitName", label: "Unit", render: (v: unknown) => (v ? String(v) : "—") },
        { name: "parentObjectiveTitle", label: "Parent", render: (v: unknown) => (v ? String(v) : "—") },
        { name: "weight", label: "Weight", render: (v: number) => `${v ?? 0}%` },
        { name: "status", label: "Status" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: OrganizationalObjectiveModel) => (
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
    <EntityListShell listKey="organizationalObjectives" listLabel="Organizational Objectives" columns={columns} {...list} />
  );
}

export default OrganizationalObjectiveList;
