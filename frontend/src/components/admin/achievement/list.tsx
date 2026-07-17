"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllAchievement from "@/services/admin/achievement/getAll";
import deleteAchievement from "@/services/admin/achievement/delete";
import type { AchievementModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function AchievementList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "achievements",
    fetchPage: getAllAchievement,
    deleteById: deleteAchievement,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "title",
          label: "Title",
          sort: true,
          render: (text: string, record: AchievementModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee" },
        { name: "category", label: "Category" },
        { name: "achievementDate", label: "Date", render: fmtDate },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: AchievementModel) => (
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
    <EntityListShell listKey="achievements" listLabel="Achievements" columns={columns} {...list} />
  );
}

export default AchievementList;
