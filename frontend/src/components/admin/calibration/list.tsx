"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllCalibrationSessions, deleteCalibrationSession } from "@/services/admin/calibration";
import type { CalibrationSessionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function CalibrationList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "calibrationSessions",
    fetchPage: getAllCalibrationSessions,
    deleteById: deleteCalibrationSession,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: CalibrationSessionModel) => (
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
        { name: "status", label: "Status" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: CalibrationSessionModel) => (
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
    <EntityListShell listKey="calibrationSessions" listLabel="Calibration Sessions" columns={columns} {...list} />
  );
}

export default CalibrationList;
