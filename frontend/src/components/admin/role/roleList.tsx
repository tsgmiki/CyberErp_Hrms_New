"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllRole from "@/services/admin/role/getAll";
import deleteRole from "@/services/admin/role/delete";
import type { RoleModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";

interface RoleListProps {
  editHandler: (id: string) => void;
}

function RoleList({ editHandler }: RoleListProps) {
  const list = useEntityList({
    queryKey: "roles",
    fetchPage: getAllRole,
    deleteById: deleteRole,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: RoleModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              <EntityBadge value={text} kind="role" />
            </button>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: string, record: RoleModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit={false}
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
    <EntityListShell
      listKey="roles"
      listLabel="Roles"
      columns={columns}
      {...list}
    />
  );
}

export default RoleList;
