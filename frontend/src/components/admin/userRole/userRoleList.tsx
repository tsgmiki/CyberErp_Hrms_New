"use client";

import { useEffect, useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllUserRole from "@/services/admin/userRole/getAll";
import deleteUserRole from "@/services/admin/userRole/delete";
import type { UserRoleModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityBadge, UserBadge } from "@/components/common/badge";
import { patchListParam } from "@/components/common/dataTableProvider/listParamUtils";
import { EntityListShell, useEntityList } from "@/template";

interface UserRoleListProps {
  editHandler: (id: string) => void;
  userId?: string;
}

function UserRoleList({ editHandler, userId }: UserRoleListProps) {
  const list = useEntityList({
    queryKey: userId ? `userRoles-${userId}` : "userRoles",
    fetchPage: getAllUserRole,
    deleteById: deleteUserRole,
    initialParam: userId ? { categoryId: userId } : undefined,
  });

  useEffect(() => {
    list.setParam((prev) => {
      const categoryId = userId ?? "";
      if (prev.categoryId === categoryId) return prev;
      return patchListParam(prev, { categoryId, skip: 0 });
    });
  }, [userId, list.setParam]);

  const columns = useMemo(
    () =>
      [
        {
          name: "user",
          label: "User",
          sort: true,
          render: (text: string, record: UserRoleModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              <UserBadge name={text} />
            </button>
          ),
        },
        {
          name: "role",
          label: "Role",
          sort: true,
          responsive: "md" as const,
          render: (text: string) => <EntityBadge value={text} kind="role" />,
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: string, record: UserRoleModel) => (
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
      listKey="user-roles"
      listLabel="User Roles"
      columns={columns}
      {...list}
    />
  );
}

export default UserRoleList;
