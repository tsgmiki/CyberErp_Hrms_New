"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllRecognitionProgram from "@/services/admin/recognitionProgram/getAll";
import deleteRecognitionProgram from "@/services/admin/recognitionProgram/delete";
import type { RecognitionProgramModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function RecognitionProgramList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "recognitionPrograms",
    fetchPage: getAllRecognitionProgram,
    deleteById: deleteRecognitionProgram,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: RecognitionProgramModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "period", label: "Period" },
        { name: "badgeName", label: "Award", render: (v: string) => v || "Chosen per nomination" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: RecognitionProgramModel) => (
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
    <EntityListShell listKey="recognitionPrograms" listLabel="Recognition Programs" columns={columns} {...list} />
  );
}

export default RecognitionProgramList;
