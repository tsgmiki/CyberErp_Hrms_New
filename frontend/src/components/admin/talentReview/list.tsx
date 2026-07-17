"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllTalentReview from "@/services/admin/talentReview/getAll";
import deleteTalentReview from "@/services/admin/talentReview/delete";
import type { TalentReviewModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const STATUS_TONE: Record<string, string> = {
  Completed: "bg-success/15 text-success",
  InProgress: "bg-info/15 text-info",
  Draft: "bg-muted/30 text-muted",
};

function TalentReviewList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({ queryKey: "talentReviews", fetchPage: getAllTalentReview, deleteById: deleteTalentReview });
  const columns = useMemo(
    () =>
      [
        { name: "name", label: "Name", sort: true, render: (t: string, r: TalentReviewModel) => (
          <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">{t}</button>) },
        { name: "cycle", label: "Cycle" },
        { name: "status", label: "Status", render: (v: string) => (
          <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>{v}</span>) },
        { name: "Action", label: "Action", render: (_t: unknown, r: TalentReviewModel) => (
          <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
            editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />) },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );
  return <EntityListShell listKey="talentReviews" listLabel="Talent Reviews" columns={columns} {...list} />;
}
export default TalentReviewList;
