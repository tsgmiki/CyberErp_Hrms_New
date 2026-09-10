"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllQuestionBank from "@/services/admin/questionBank/getAll";
import deleteQuestionBank from "@/services/admin/questionBank/delete";
import type { QuestionBankModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function QuestionBankList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "questionBanks",
    fetchPage: getAllQuestionBank,
    deleteById: deleteQuestionBank,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: QuestionBankModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "description", label: "Description" },
        {
          name: "questionCount",
          label: "Questions",
          // A bank with nothing in it is the thing worth spotting from the list.
          render: (v: number) => (v > 0 ? v : "—"),
        },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: QuestionBankModel) => (
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

  return <EntityListShell listKey="questionBanks" listLabel="Question Banks" columns={columns} {...list} />;
}

export default QuestionBankList;
