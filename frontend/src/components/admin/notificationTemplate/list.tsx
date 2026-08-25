"use client";

import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Sparkles } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import {
  getAllNotificationTemplates,
  deleteNotificationTemplate,
  seedNotificationEvents,
} from "@/services/admin/notificationTemplate";
import type { NotificationTemplateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function NotificationTemplateList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [seedMessage, setSeedMessage] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "notificationTemplates",
    fetchPage: getAllNotificationTemplates,
    deleteById: deleteNotificationTemplate,
  });

  /**
   * Loads the catalogue of events this system can notify on. Idempotent, and it deletes nothing —
   * a template points at an event row, so removing one would orphan the client's configuration.
   */
  const seed = async () => {
    const res = await seedNotificationEvents();
    setSeedMessage(res.message);
    queryClient.invalidateQueries({ queryKey: ["notificationEvents"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Template",
          sort: true,
          render: (text: string, record: NotificationTemplateModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "eventName", label: "Event", sort: true },
        { name: "subject", label: "Subject" },
        {
          name: "stepOrder",
          label: "Scope",
          // The scope is what makes two templates for one event unambiguous, so it belongs in the
          // list rather than only inside the form.
          render: (_v: unknown, r: NotificationTemplateModel) =>
            r.workflowDefinitionId
              ? r.stepOrder
                ? `${t("Step")} ${r.stepOrder}`
                : t("This workflow")
              : t("All workflows"),
        },
        { name: "channel", label: "Channel" },
        {
          name: "isActive",
          label: "Active",
          render: (v: unknown) => (v === true || v === "true" ? t("Yes") : t("No")),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: NotificationTemplateModel) => (
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
          className="inline-flex items-center gap-1.5 rounded-md border border-primary bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary transition-opacity hover:opacity-80"
        >
          <Sparkles size={14} /> {t("Load events")}
        </button>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="notificationTemplates"
          listLabel="Email Templates"
          columns={columns}
          {...list}
        />
      </div>
    </div>
  );
}

export default NotificationTemplateList;
