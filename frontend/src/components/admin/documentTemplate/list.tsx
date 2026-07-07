"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllDocumentTemplate from "@/services/admin/documentTemplate/getAll";
import deleteDocumentTemplate from "@/services/admin/documentTemplate/delete";
import type { DocumentTemplateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { documentTypeLabel } from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

function DocumentTemplateList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "documentTemplates",
    fetchPage: getAllDocumentTemplate,
    deleteById: deleteDocumentTemplate,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: DocumentTemplateModel) => (
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
          name: "documentType",
          label: "Type",
          render: (v: string) => documentTypeLabel(v),
        },
        { name: "description", label: "Description" },
        {
          name: "isActive",
          label: "Active",
          render: (v: boolean) => (v ? "Yes" : "No"),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: DocumentTemplateModel) => (
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
      listKey="documentTemplates"
      listLabel="Document Templates"
      columns={columns}
      {...list}
    />
  );
}

export default DocumentTemplateList;
