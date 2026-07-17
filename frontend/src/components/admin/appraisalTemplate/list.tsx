"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllAppraisalTemplate from "@/services/admin/appraisalTemplate/getAll";
import deleteAppraisalTemplate from "@/services/admin/appraisalTemplate/delete";
import type { AppraisalTemplateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function AppraisalTemplateList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "appraisalTemplates",
    fetchPage: getAllAppraisalTemplate,
    deleteById: deleteAppraisalTemplate,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: AppraisalTemplateModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "goalsWeight", label: "Goals %", render: (v: number) => `${v ?? 0}%` },
        { name: "competenciesWeight", label: "Competencies %", render: (v: number) => `${v ?? 0}%` },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: AppraisalTemplateModel) => (
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
    <EntityListShell listKey="appraisalTemplates" listLabel="Appraisal Templates" columns={columns} {...list} />
  );
}

export default AppraisalTemplateList;
