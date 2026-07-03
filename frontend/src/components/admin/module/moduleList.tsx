"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllModule from "@/services/admin/module/getAll";
import deleteModule from "@/services/admin/module/delete";
import type { ModuleModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";

interface ModuleListProps {
  editHandler: (id: string) => void;
}

function ModuleList({ editHandler }: ModuleListProps) {
  const list = useEntityList({
    queryKey: "modules",
    fetchPage: getAllModule,
    deleteById: deleteModule,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: ModuleModel) => (
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
          name: "subSystem",
          label: "Sub System",
          sort: true,
          render: (text: string) => <EntityBadge value={text} kind="organization" />,
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: unknown, record: ModuleModel) => (
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
      listKey="modules"
      listLabel="Modules"
      columns={columns}
      {...list}
    />
  );
}

export default ModuleList;
