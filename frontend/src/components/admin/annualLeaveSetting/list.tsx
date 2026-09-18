"use client";

import { useMemo } from "react";
import { useQueryClient } from "@tanstack/react-query";
import GridAction from "../../common/gridAction/gridAction";
import getAllSetting from "@/services/admin/annualLeaveSetting/getAll";
import deleteSetting from "@/services/admin/annualLeaveSetting/delete";
import generateEntitlements from "@/services/admin/annualLeaveSetting/generate";
import rolloverLeaveSetting from "@/services/admin/annualLeaveSetting/rollover";
import type { AnnualLeaveSettingModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { confirm } from "@/components/common/dialog";
import { toast } from "@/components/common/toast";

interface Props {
  editHandler: (id: string) => void;
}

function AnnualLeaveSettingList({ editHandler }: Props) {
  const queryClient = useQueryClient();
  const list = useEntityList({
    queryKey: "annualLeaveSettings",
    fetchPage: getAllSetting,
    deleteById: deleteSetting,
  });

  const doGenerate = async (r: AnnualLeaveSettingModel) => {
    if (!r.id) return;
    if (
      !(await confirm({
        title: "Generate entitlements",
        message: `Generate entitlements for all active employees under "${r.fiscalYearName}"? Already-generated employees are skipped.`,
        confirmLabel: "Generate",
        variant: "default",
      }))
    )
      return;
    const result = await generateEntitlements(r.id);
    toast.success(result?.message ?? "Done");
  };

  /**
   * Year-end rollover. Moved here from the Fiscal Year grid (logic §12.97) because the carry cap it
   * applies is a field of THIS row.
   *
   * The confirm spells out the cap rather than saying "this cannot be undone" and leaving the
   * operator to go and look it up — closing a year and expiring carried days is not a decision to
   * take from a generic warning.
   */
  const doRollover = async (r: AnnualLeaveSettingModel) => {
    if (!r.id) return;
    const cap =
      r.carryForwardMaxDays === null || r.carryForwardMaxDays === undefined
        ? "no cap — every remaining day carries"
        : r.carryForwardMaxDays === 0
          ? "0 days — nothing carries, the remainder expires"
          : `up to ${r.carryForwardMaxDays} day(s) per employee`;
    if (
      !(await confirm({
        title: "Roll over fiscal year",
        // JSX, not a string with newlines: the host renders the message as children, so "\n\n"
        // would collapse into one run-on line.
        message: (
          <div className="space-y-2">
            <p>
              Carry remaining balances of <strong>{r.fiscalYearName}</strong> into the next fiscal year
              and <strong>close it</strong>?
            </p>
            <p>
              Carry-forward cap on this policy: <strong>{cap}</strong>. Days already carried in from the
              previous year expire now.
            </p>
            {/* text-error, not text-danger: only registered palette tokens emit anything here. */}
            <p className="font-medium text-error">This cannot be undone.</p>
          </div>
        ),
        confirmLabel: "Roll over",
        variant: "destructive",
      }))
    )
      return;
    const result = await rolloverLeaveSetting(r.id);
    toast.success(result?.message ?? "Rollover complete");
    // The fiscal year is now closed, so both grids are stale.
    queryClient.invalidateQueries({ queryKey: ["annualLeaveSettings"] });
    queryClient.invalidateQueries({ queryKey: ["fiscalYears"] });
    queryClient.invalidateQueries({ queryKey: ["leaveBalances"] });
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
              {/* Hidden once the year is closed — a rolled-over year cannot be rolled again, which is
                  the same gate the Fiscal Year grid used to apply. */}
              {!r.fiscalYearClosed && (
                <button
                  type="button"
                  onClick={() => doRollover(r)}
                  className="rounded-md border border-border px-2 py-1 text-xs hover:bg-primary/10"
                  title="Carry remaining leave into the next fiscal year and close this one"
                >
                  Rollover
                </button>
              )}
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
