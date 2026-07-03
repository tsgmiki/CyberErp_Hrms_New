"use client";

import { useCallback, useMemo } from "react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllOperation from "@/services/admin/operation/getAll";
import deleteOperation from "@/services/admin/operation/delete";
import type { OperationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { LinkBadge } from "@/components/common/badge";
import { EntityListShell, useEntityList } from "@/template";

interface OperationListProps {
  editHandler: (id: string) => void;
}

function OperationList({ editHandler }: OperationListProps) {
  const { t } = useTranslation();
  const list = useEntityList({
    queryKey: "operations",
    fetchPage: getAllOperation,
    deleteById: deleteOperation,
    initialParam: { sortCol: "module" },
  });

  const tableData = useMemo(
    () =>
      (list.rows ?? []).map((op) => {
        const row = op as unknown as OperationModel;
        return {
          ...row,
          module: row.module?.trim() || row.moduleId || "",
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
      groupBy="module"
      getGroupLabel={getGroupLabel}
      className="flex h-full min-h-0 flex-col"
    />
  );
}

export default OperationList;
