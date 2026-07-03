"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllUser from "@/services/admin/user/getAll";
import deleteUser from "@/services/admin/user/delete";
import type { UserModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EmailBadge, PhoneBadge, UserBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";

interface UserListProps {
  editHandler: (id: string) => void;
}

function UserList({ editHandler }: UserListProps) {
  const list = useEntityList({
    queryKey: "users",
    fetchPage: getAllUser,
    deleteById: deleteUser,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "fullName",
          label: "Full Name",
          sort: true,
          render: (text: string, record: UserModel) => (
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
          name: "email",
          label: "Email",
          sort: true,
          responsive: "md" as const,
          render: (text: string) => <EmailBadge value={text} />,
        },
        {
          name: "phoneNumber",
          label: "Phone Number",
          sort: true,
          responsive: "md" as const,
          render: (text: string) => <PhoneBadge value={text} />,
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: string, record: UserModel) => (
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
      listKey="users"
      listLabel="Users"
      columns={columns}
      {...list}
    />
  );
}

export default UserList;
