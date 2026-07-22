"use client";

import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Sprout } from "lucide-react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllModule from "@/services/admin/module/getAll";
import deleteModule from "@/services/admin/module/delete";
import seedDefaultMenu from "@/services/admin/module/seedDefaults";
import type { ModuleModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";

interface ModuleListProps {
  editHandler: (id: string) => void;
}

function ModuleList({ editHandler }: ModuleListProps) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [seeding, setSeeding] = useState(false);
  const [seedMsg, setSeedMsg] = useState("");
  const list = useEntityList({
    queryKey: "modules",
    fetchPage: getAllModule,
    deleteById: deleteModule,
  });

  const seed = async () => {
    setSeeding(true);
    const res = await seedDefaultMenu();
    setSeeding(false);
    setSeedMsg(res.message);
    queryClient.invalidateQueries({ queryKey: ["modules"] });
    queryClient.invalidateQueries({ queryKey: ["subsystems"] });
    queryClient.invalidateQueries({ queryKey: ["moduleWithOperations"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: ModuleModel) => (
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
          name: "subSystem",
          label: "Sub System",
          sort: true,
          render: (text: string) => <EntityBadge value={text} kind="organization" />,
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: unknown, record: ModuleModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit={false}
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
    <div>
      <div className="mb-2 flex items-center justify-end gap-2">
        {seedMsg && <span className="text-xs text-muted">{seedMsg}</span>}
        <button
          type="button"
          disabled={seeding}
          onClick={seed}
          title={t("Creates the built-in HRMS menu (subsystem, modules, operations) for this tenant; existing rows are kept.")}
          className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-sm font-medium text-foreground hover:bg-secondary/40 disabled:opacity-50"
        >
          <Sprout size={14} /> {seeding ? t("Seeding…") : t("Seed Default Menu")}
        </button>
      </div>
      <EntityListShell
        listKey="modules"
        listLabel="Modules"
        columns={columns}
        {...list}
      />
    </div>
  );
}

export default ModuleList;
