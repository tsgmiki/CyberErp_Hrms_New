"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllSetting from "@/services/admin/annualLeaveSetting/getAll";
import deleteSetting from "@/services/admin/annualLeaveSetting/delete";
import generateEntitlements from "@/services/admin/annualLeaveSetting/generate";
import type { AnnualLeaveSettingModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function AnnualLeaveSettingList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "annualLeaveSettings",
    fetchPage: getAllSetting,
    deleteById: deleteSetting,
  });

  const doGenerate = async (r: AnnualLeaveSettingModel) => {
    if (!r.id) return;
    if (!window.confirm(`Generate entitlements for all active employees under "${r.fiscalYearName}"? Already-generated employees are skipped.`)) return;
    const result = await generateEntitlements(r.id);
    window.alert(result?.message ?? "Done");
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "fiscalYearName",
          label: "Fiscal Year",
          render: (_t: unknown, r: AnnualLeaveSettingModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
              {r.fiscalYearName}
            </button>
          ),
        },
        { name: "leaveTypeName", label: "Leave Type" },
        { name: "baseLeaveDays", label: "Base" },
        { name: "managerialLeaveDays", label: "Managerial" },
        {
          name: "incrementDays",
          label: "Increment",
          render: (_t: unknown, r: AnnualLeaveSettingModel) =>
            `+${r.incrementDays}/${r.incrementIntervalYears}yrs`,
        },
        { name: "maxLeaveDays", label: "Max" },
        { name: "minExperienceMonths", label: "Min Svc (mo)" },
        {
          name: "isActive",
          label: "Status",
          render: (_t: unknown, r: AnnualLeaveSettingModel) => (r.isActive ? "Active" : "Inactive"),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, r: AnnualLeaveSettingModel) => (
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => doGenerate(r)}
                className="rounded-md border border-border px-2 py-1 text-xs hover:bg-primary/10"
                title="Generate service-based entitlements for all active employees"
              >
                Generate
              </button>
              <GridAction
                id={r.id || ""}
                record={r}
                showAdd={false}
                showEdit
                showDelete
                editHandler={editHandler}
                deleteHandler={() => r.id && list.deleteRecord(r.id)}
              />
            </div>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return (
    <EntityListShell
      listKey="annualLeaveSettings"
      listLabel="Annual Leave Settings"
      columns={columns}
      {...list}
    />
  );
}

export default AnnualLeaveSettingList;
