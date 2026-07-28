"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllOtherLeaveSettings, deleteOtherLeaveSetting } from "@/services/admin/otherLeave";
import type { OtherLeaveSettingModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { otherLeaveGenderLabel } from "@/constants/leave";
import { EntityListShell, useEntityList } from "@/template";

const days = (v?: number) => (v ?? 0).toLocaleString();

function OtherLeaveSettingList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({
    queryKey: "otherLeaveSettings",
    fetchPage: getAllOtherLeaveSettings,
    deleteById: deleteOtherLeaveSetting,
  });

  const columns = useMemo(
    () =>
      [
        { name: "name", label: "Leave", sort: true, render: (v: string, r: OtherLeaveSettingModel) => (
          <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
            <span className="block font-semibold">{v}</span>
            <span className="block text-xs text-muted">{r.fiscalYearName}</span>
          </button>) },
        { name: "gender", label: "Eligibility", render: (v: string) => otherLeaveGenderLabel(v) },
        { name: "standardDays", label: "Standard Days", render: (v: number) => (
          <span className="block text-right tabular-nums">{days(v)}</span>) },
        { name: "managerialDays", label: "Managerial Days", render: (v: number) => (
          <span className="block text-right tabular-nums">{days(v)}</span>) },
        { name: "isLumpSum", label: "Lump-sum", render: (v: boolean) => (v ? (
          <span className="rounded bg-primary/10 px-2 py-0.5 text-xs font-semibold text-primary">One block</span>) : "—") },
        { name: "dayCounting", label: "Holidays & Weekends", render: (v: string) =>
          v === "CalendarDays" ? "Counted" : "Skipped" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        { name: "Action", label: "Action", render: (_t: unknown, r: OtherLeaveSettingModel) => (
          <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
            editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />) },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="otherLeaveSettings" listLabel="Other Leave Settings" columns={columns} {...list} />;
}
export default OtherLeaveSettingList;
