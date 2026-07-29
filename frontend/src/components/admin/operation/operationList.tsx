"use client";

import { useCallback, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllOperation from "@/services/admin/operation/getAll";
import deleteOperation from "@/services/admin/operation/delete";
import type { OperationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { LinkBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";
import SubsystemModuleFilter, {
  type MenuScope,
} from "@/components/common/menuFilters/subsystemModuleFilter";

interface OperationListProps {
  editHandler: (id: string) => void;
}

function OperationList({ editHandler }: OperationListProps) {
  const { t } = useTranslation();
  const [scope, setScope] = useState<MenuScope>({});
  const list = useEntityList({
    queryKey: "operations",
    fetchPage: getAllOperation,
    deleteById: deleteOperation,
    initialParam: { sortCol: "" },
  });

  // Cascading Subsystem → Module scope, applied SERVER-side via the paged query.
  const scopeHandler = useCallback(
    (next: MenuScope) => {
      setScope(next);
      list.setParam((prev) => ({
        ...prev,
        subsystemId: next.subsystemId ?? "",
        moduleId: next.moduleId ?? "",
        skip: 0,
      }));
    },
    [list.setParam],
  );

  const tableData = useMemo(
    () =>
      (list.rows ?? []).map((op) => {
        const row = op as unknown as OperationModel;
        const module = row.module?.trim() || row.moduleId || "";
        const subSystem = row.subSystem?.trim() || "";
        return {
          ...row,
          module,
          subSystem,
          // Group key includes the subsystem so same-named modules never merge across apps.
          moduleGroup: subSystem ? `${subSystem} / ${module}` : module,
        };
      }),
    [list.rows],
  );

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: OperationModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left font-semibold hover:underline"
            >
              {text}
            </button>
          ),
        },
        {
          name: "link",
          label: "Link",
          sort: true,
          responsive: "md" as const,
          render: (text: string) => <LinkBadge value={text} />,
        },
        {
          name: "subSystem",
          label: "Sub System",
          responsive: "md" as const,
          render: (text: string) => (
            <span className="text-xs font-medium text-muted">{text}</span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_text: unknown, record: OperationModel) => (
            <GridAction
              id={record.id as string}
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

  const getGroupLabel = useCallback(
    (key: string, rows: Record<string, unknown>[]) => {
      const moduleName =
        key?.trim() || t("Unassigned module", "Unassigned module");
      const countLabel =
        rows.length === 1
          ? t("1 operation", "1 operation")
          : t("{{count}} operations", {
              count: rows.length,
              defaultValue: `${rows.length} operations`,
            });
      return `${moduleName} · ${countLabel}`;
    },
    [t],
  );

  return (
    <EntityListShell
      listKey="operations"
      listLabel="Operations"
      columns={columns}
      {...list}
      rows={tableData as Record<string, unknown>[]}
      groupBy="moduleGroup"
      getGroupLabel={getGroupLabel}
      searchBarFilters={<SubsystemModuleFilter value={scope} onChange={scopeHandler} />}
      className="flex h-full min-h-0 flex-col"
    />
  );
}

export default OperationList;
