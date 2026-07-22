"use client";

import { useEffect, useMemo, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllPosition from "@/services/admin/position/getAll";
import deletePosition from "@/services/admin/position/delete";
import type { PositionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  /** Selected organization unit id from the tree; when empty the grid shows all positions. */
  organizationUnitId?: string;
  organizationUnitName?: string;
  editHandler: (id: string) => void;
}

/** Right-hand data grid showing positions of the selected organization unit. */
function PositionGrid({ organizationUnitId, organizationUnitName, editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  // Position GetAll maps `parentId` → OrganizationUnitId filter on the backend.
  const list = useEntityList({
    queryKey: "positions",
    fetchPage: getAllPosition,
    initialParam: organizationUnitId ? { parentId: organizationUnitId } : {},
  });

  useEffect(() => {
    setError(null);
    list.setParam((p) => ({
      ...p,
      parentId: organizationUnitId || undefined,
      skip: 0,
    }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [organizationUnitId]);

  const { mutate: deleteRecord } = useMutation({
    mutationFn: (id: string) => deletePosition(id),
    onSuccess: (result: any) => {
      if (result?.status === "error") {
        setError(result.message || "Delete failed.");
        return;
      }
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["positions"] });
    },
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: PositionModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "positionClassTitle", label: "Position Class", sort: true },
        { name: "organizationUnitName", label: "Organization Unit" },
        {
          name: "isVacant",
          label: "Status",
          render: (v: unknown) => {
            const vacant = v === true || v === "true";
            return (
              <span
                className={`rounded px-2 py-0.5 text-xs font-semibold ${
                  vacant ? "bg-success/15 text-success" : "bg-muted/30 text-muted"
                }`}
              >
                {vacant ? t("Vacant") : t("Occupied")}
              </span>
            );
          },
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: PositionModel) => (
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
    [editHandler, deleteRecord, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="border-b border-border px-3 py-2 text-sm font-semibold text-foreground">
        {organizationUnitName ? `${t("Positions in")}: ${organizationUnitName}` : t("All Positions")}
      </div>
      {error && (
        <div className="mx-3 mt-2 flex items-center justify-between rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="font-semibold">
            ×
          </button>
        </div>
      )}
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell listKey="positions" listLabel="Positions" columns={columns} {...list} />
      </div>
    </div>
  );
}

export default PositionGrid;
