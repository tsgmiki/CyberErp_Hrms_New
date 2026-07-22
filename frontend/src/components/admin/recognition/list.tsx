"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllRecognition from "@/services/admin/recognition/getAll";
import deleteRecognition from "@/services/admin/recognition/delete";
import type { EmployeeRecognitionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function RecognitionList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "recognitions",
    fetchPage: getAllRecognition,
    deleteById: deleteRecognition,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: EmployeeRecognitionModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        {
          name: "badgeName",
          label: "Badge",
          render: (text: string, record: EmployeeRecognitionModel) => (
            <span className="inline-flex items-center gap-1.5">
              <span className="inline-block h-3 w-3 rounded-full" style={{ backgroundColor: record.badgeColor || "var(--color-primary, #888)" }} />
              {text}
            </span>
          ),
        },
        { name: "citation", label: "Citation" },
        { name: "recognizedOn", label: "Date", render: fmtDate },
        { name: "isPublic", label: "Public", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: EmployeeRecognitionModel) => (
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
    <EntityListShell listKey="recognitions" listLabel="Recognition Board" columns={columns} {...list} />
  );
}

export default RecognitionList;
