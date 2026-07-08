"use client";

import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Sparkles } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import getAllDocumentTemplate from "@/services/admin/documentTemplate/getAll";
import deleteDocumentTemplate from "@/services/admin/documentTemplate/delete";
import seedDefaultDocumentTemplates from "@/services/admin/documentTemplate/seed";
import type { DocumentTemplateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { documentTypeLabel } from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

function DocumentTemplateList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [seedMessage, setSeedMessage] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "documentTemplates",
    fetchPage: getAllDocumentTemplate,
    deleteById: deleteDocumentTemplate,
  });

  const seed = async () => {
    const res = await seedDefaultDocumentTemplates();
    setSeedMessage(res.message);
    queryClient.invalidateQueries({ queryKey: ["documentTemplates"] });
  };

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
    <div className="flex h-full min-h-0 flex-col">
      <div className="mb-2 flex items-center justify-end gap-2 px-1">
        {seedMessage && <span className="text-xs text-muted">{seedMessage}</span>}
        <button
          type="button"
          onClick={seed}
          className="inline-flex items-center gap-1.5 rounded-md border border-primary/40 bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary hover:bg-primary/20"
        >
          <Sparkles size={14} /> {t("Seed default templates")}
        </button>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="documentTemplates"
          listLabel="Document Templates"
          columns={columns}
          {...list}
        />
      </div>
    </div>
  );
}

export default DocumentTemplateList;
