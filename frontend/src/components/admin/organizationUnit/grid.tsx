"use client";

import { useEffect, useMemo, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import deleteOrganizationUnit from "@/services/admin/organizationUnit/delete";
import type { OrganizationUnitModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  /** Currently selected tree node; when empty the grid shows root units. */
  parentId?: string;
  parentName?: string;
  editHandler: (id: string) => void;
}

/** Right-hand data grid showing the children of the selected tree node. */
function OrganizationUnitGrid({ parentId, parentName, editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "organizationUnits",
    fetchPage: getAllOrganizationUnit,
    initialParam: parentId ? { parentId } : { isRoot: true },
  });

  // Re-scope the grid whenever the selected tree node changes.
  useEffect(() => {
    setError(null);
    list.setParam((p) => ({
      ...p,
      parentId: parentId || undefined,
      isRoot: !parentId,
      skip: 0,
    }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [parentId]);

  const { mutate: deleteRecord } = useMutation({
    mutationFn: (id: string) => deleteOrganizationUnit(id),
    onSuccess: (result: any) => {
      if (result?.status === "error") {
        setError(result.message || "Delete failed.");
        return;
      }
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["organizationUnits"] });
      queryClient.invalidateQueries({ queryKey: ["organizationTree"] });
    },
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: OrganizationUnitModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "name", label: "Name", sort: true },
        { name: "unitType", label: "Type", sort: true },
        { name: "allocatedHeadcount", label: "Headcount" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: OrganizationUnitModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              editHandler={editHandler}
              deleteHandler={() => record.id && deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, deleteRecord],
  );

  return (
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="border-b border-border px-3 py-2 text-sm font-semibold text-foreground">
        {parentName ? `${t("Units under")}: ${parentName}` : t("Root Units")}
      </div>
      {error && (
        <div className="mx-3 mt-2 flex items-center justify-between rounded border border-red-300 bg-red-50 px-3 py-2 text-xs text-red-700">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="font-semibold">
            ×
          </button>
        </div>
      )}
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="organizationUnits"
          listLabel="Organization Units"
          columns={columns}
          {...list}
        />
      </div>
    </div>
  );
}

export default OrganizationUnitGrid;
