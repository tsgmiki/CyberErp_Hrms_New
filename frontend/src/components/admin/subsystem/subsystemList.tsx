"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllSubsystems from "@/services/admin/subsystem/getAll";
import deleteSubsystem from "@/services/admin/subsystem/delete";
import type { SubsystemModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";

interface SubsystemListProps {
  editHandler: (id: string) => void;
}

function SubsystemList({ editHandler }: SubsystemListProps) {
  const list = useEntityList({
    queryKey: "subsystems",
    fetchPage: getAllSubsystems,
    deleteById: deleteSubsystem,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: SubsystemModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string) => <EntityBadge value={text} kind="organization" />,
        },
        {
          name: "sortOrder",
          label: "Sort Order",
          sort: true,
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: unknown, record: SubsystemModel) => (
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
      listKey="subsystems"
      listLabel="Subsystems"
      columns={columns}
      {...list}
    />
  );
}

export default SubsystemList;
