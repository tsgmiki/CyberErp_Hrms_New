"use client";

import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import {
  getAllClearanceDepartments,
  deleteClearanceDepartment,
} from "@/services/admin/clearanceDepartment";
import type { ClearanceDepartmentModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function ClearanceDepartmentList({ editHandler }: Props) {
  const { t } = useTranslation();

  const list = useEntityList({
    queryKey: "clearanceDepartments",
    fetchPage: getAllClearanceDepartments,
    deleteById: deleteClearanceDepartment,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Department",
          sort: true,
          render: (text: string, record: ClearanceDepartmentModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "description", label: "Clearance Requirement" },
        {
          name: "approvers",
          label: "Approvers",
          render: (_v: unknown, r: ClearanceDepartmentModel) =>
            (r.approvers ?? []).map((a) => a.displayName).join(", ") || (
              <span className="text-xs italic text-muted">{t("Open — anyone may clear")}</span>
            ),
        },
        { name: "sortOrder", label: "Order" },
        {
          name: "isActive",
          label: "Active",
          render: (v: unknown) => (v === true || v === "true" ? t("Yes") : t("No")),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: ClearanceDepartmentModel) => (
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
    <EntityListShell
      listKey="clearanceDepartments"
      listLabel="Clearance Departments"
      columns={columns}
      {...list}
    />
  );
}

export default ClearanceDepartmentList;
