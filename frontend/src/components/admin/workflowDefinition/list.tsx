"use client";

import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Sparkles } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import {
  getAllWorkflowDefinitions,
  deleteWorkflowDefinition,
  seedDefaultWorkflows,
} from "@/services/admin/workflow";
import type { WorkflowDefinitionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

function WorkflowDefinitionList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [seedMessage, setSeedMessage] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "workflowDefinitions",
    fetchPage: getAllWorkflowDefinitions,
    deleteById: deleteWorkflowDefinition,
  });

  const seed = async () => {
    const res = await seedDefaultWorkflows();
    setSeedMessage(res.message);
    queryClient.invalidateQueries({ queryKey: ["workflowDefinitions"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: WorkflowDefinitionModel) => (
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
          name: "entityType",
          label: "Process",
          render: (v: string) => workflowEntityTypeLabel(v),
        },
        {
          name: "steps",
          label: "Approval Chain",
          render: (_v: unknown, r: WorkflowDefinitionModel) =>
            (r.steps ?? []).map((s) => s.name).join(" → ") || "—",
        },
        {
          name: "isActive",
          label: "Active",
          render: (v: unknown) => (v === true || v === "true" ? t("Yes") : t("No")),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: WorkflowDefinitionModel) => (
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
    [editHandler, list.deleteRecord, t],
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
          <Sparkles size={14} /> {t("Seed default workflows")}
        </button>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="workflowDefinitions"
          listLabel="Workflow Definitions"
          columns={columns}
          {...list}
        />
      </div>
    </div>
  );
}

export default WorkflowDefinitionList;
