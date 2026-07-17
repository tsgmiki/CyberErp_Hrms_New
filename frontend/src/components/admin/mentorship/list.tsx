"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllMentorship from "@/services/admin/mentorship/getAll";
import deleteMentorship from "@/services/admin/mentorship/delete";
import type { MentorshipModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { mentorshipContextOptions, mentorshipStatusOptions } from "@/constants/careerDevelopment";

const STATUS_TONE: Record<string, string> = {
  Active: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};
const ctx = (v?: string) => mentorshipContextOptions.find((o) => o.id === v)?.name ?? v ?? "";

function MentorshipList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({
    queryKey: "mentorships",
    fetchPage: getAllMentorship,
    deleteById: deleteMentorship,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "menteeName", label: "Mentee", sort: true,
          render: (t: string, r: MentorshipModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{t ?? "—"}</span>
              <span className="block text-xs text-muted">Mentor: {r.mentorName ?? "—"}</span>
            </button>
          ),
        },
        { name: "context", label: "Context", render: (v: string) => ctx(v) },
        { name: "status", label: "Status", render: (v: string) => (<span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>{mentorshipStatusOptions.find((o) => o.id === v)?.name ?? v}</span>) },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: MentorshipModel) => (
            <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
              editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="mentorships" listLabel="Mentorships" columns={columns} {...list} />;
}

export default MentorshipList;
