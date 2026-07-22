"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllAllowanceTypes from "@/services/admin/allowanceType/getAll";
import deleteAllowanceType from "@/services/admin/allowanceType/delete";
import type { AllowanceTypeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function AllowanceTypeList({ editHandler }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({
    queryKey: "allowanceTypes",
    fetchPage: getAllAllowanceTypes,
    deleteById: deleteAllowanceType,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: AllowanceTypeModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
              {record.code ? <span className="ml-1 text-xs text-muted">({record.code})</span> : ""}
            </button>
          ),
        },
        { name: "calcMethod", label: "Method", render: (v: string) => (v === "PercentOfBase" ? t("% of base") : t("Fixed")) },
        {
          name: "defaultRate",
          label: "Default",
          render: (_t: unknown, r: AllowanceTypeModel) => (
            <span className="tabular-nums">{r.defaultRate ?? "—"}{r.calcMethod === "PercentOfBase" && r.defaultRate != null ? "%" : ""}</span>
          ),
        },
        { name: "isTaxable", label: "Taxable", render: (_t: unknown, r: AllowanceTypeModel) => (r.isTaxable ? t("Yes") : t("No")) },
        {
          name: "isActive",
          label: "Active",
          render: (_t: unknown, r: AllowanceTypeModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${r.isActive ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
              {r.isActive ? t("Active") : t("Inactive")}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: AllowanceTypeModel) => (
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

  return <EntityListShell listKey="allowanceTypes" listLabel="Allowance Types" columns={columns} {...list} />;
}

export default AllowanceTypeList;
