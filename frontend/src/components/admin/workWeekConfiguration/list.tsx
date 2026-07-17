"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllWorkWeekConfiguration from "@/services/admin/workWeekConfiguration/getAll";
import deleteWorkWeekConfiguration from "@/services/admin/workWeekConfiguration/delete";
import type { WorkWeekConfigurationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { weekDays, dayModeTag } from "@/constants/leave";

interface Props {
  editHandler: (id: string) => void;
}

function WorkWeekConfigurationList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "workWeekConfigurations",
    fetchPage: getAllWorkWeekConfiguration,
    deleteById: deleteWorkWeekConfiguration,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: WorkWeekConfigurationModel) => (
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
          name: "week",
          label: "Week (Mon → Sun)",
          render: (_t: unknown, r: WorkWeekConfigurationModel) => (
            <div className="flex gap-1">
              {weekDays.map((d) => {
                const mode = (r as any)[d.key] as string;
                return (
                  <span
                    key={d.key}
                    title={`${d.label}: ${mode}`}
                    className={`inline-flex h-6 w-8 items-center justify-center rounded text-xs font-semibold ${
                      mode === "Full"
                        ? "bg-emerald-500/15 text-emerald-600"
                        : mode === "Half"
                          ? "bg-amber-500/15 text-amber-600"
                          : "bg-slate-500/10 text-slate-500"
                    }`}
                  >
                    {dayModeTag[mode] ?? "·"}
                  </span>
                );
              })}
            </div>
          ),
        },
        {
          name: "isActive",
          label: "Status",
          render: (_t: unknown, r: WorkWeekConfigurationModel) =>
            r.isActive ? (
              <span className="rounded-full bg-emerald-500/15 px-2 py-0.5 text-xs font-medium text-emerald-600">
                Active
              </span>
            ) : (
              <span className="text-xs text-muted">Inactive</span>
            ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: WorkWeekConfigurationModel) => (
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
    <EntityListShell
      listKey="workWeekConfigurations"
      listLabel="Work Week Configurations"
      columns={columns}
      {...list}
    />
  );
}

export default WorkWeekConfigurationList;
