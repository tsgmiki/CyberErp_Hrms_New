"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Pencil, Trash2, CheckCircle2, Ban, Users, Video } from "lucide-react";
import { getAllTrainingSessions, deleteTrainingSession, completeTrainingSession, cancelTrainingSession } from "@/services/admin/trainingSession";
import type { TrainingSessionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import ParticipantsModal from "./participantsModal";

interface Props {
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Scheduled: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function TrainingSessionList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [participantsFor, setParticipantsFor] = useState<TrainingSessionModel | null>(null);
  const [actionMsg, setActionMsg] = useState<string>("");

  const list = useEntityList({
    queryKey: "trainingSessions",
    fetchPage: getAllTrainingSessions,
    deleteById: deleteTrainingSession,
  });

  const runAction = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    const res = await fn();
    setActionMsg(res.message);
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["trainingSessions"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "courseName",
          label: "Course",
          sort: true,
          render: (text: string, record: TrainingSessionModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">
                {record.deliveryMode}
                {record.meetingUrl && <Video size={11} className="ml-1 inline text-info" />}
              </span>
            </button>
          ),
        },
        {
          name: "startDate",
          label: "Dates",
          sort: true,
          render: (_v: unknown, record: TrainingSessionModel) => (
            <span className="text-xs">{fmtDate(record.startDate)} → {fmtDate(record.endDate)}</span>
          ),
        },
        { name: "venue", label: "Venue", render: (v: string) => v || "—" },
        {
          name: "trainerName",
          label: "Trainer",
          render: (v: string, record: TrainingSessionModel) => (
            <span>
              <span className="block">{v || "—"}</span>
              <span className="block text-xs text-muted">{record.providerName || record.trainerType}</span>
            </span>
          ),
        },
        {
          name: "enrolledCount",
          label: "Seats",
          render: (v: number, record: TrainingSessionModel) => (
            <span className="text-xs font-semibold">
              {v ?? 0}{record.maxParticipants ? ` / ${record.maxParticipants}` : ""}
            </span>
          ),
        },
        {
          name: "status",
          label: "Status",
          render: (v: string) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-secondary/40 text-foreground"}`}>{v}</span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: TrainingSessionModel) => {
            const scheduled = record.status === "Scheduled";
            return (
              <span className="flex items-center gap-1.5">
                <button type="button" title={t("Participants")} onClick={() => setParticipantsFor(record)} className="rounded p-1 text-muted hover:text-primary">
                  <Users size={15} />
                </button>
                {scheduled && (
                  <button type="button" title={t("Edit / reschedule")} onClick={() => record.id && editHandler(record.id)} className="rounded p-1 text-muted hover:text-primary">
                    <Pencil size={15} />
                  </button>
                )}
                {scheduled && (
                  <button
                    type="button"
                    title={t("Mark completed (raises the provider payment)")}
                    onClick={() => record.id && runAction(() => completeTrainingSession(record.id!))}
                    className="rounded p-1 text-muted hover:text-success"
                  >
                    <CheckCircle2 size={15} />
                  </button>
                )}
                {scheduled && (
                  <button type="button" title={t("Cancel session")} onClick={() => record.id && runAction(() => cancelTrainingSession(record.id!))} className="rounded p-1 text-muted hover:text-error">
                    <Ban size={15} />
                  </button>
                )}
                <button type="button" title={t("Delete")} onClick={() => record.id && list.deleteRecord(record.id)} className="rounded p-1 text-muted hover:text-error">
                  <Trash2 size={15} />
                </button>
              </span>
            );
          },
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="space-y-2">
      {actionMsg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{actionMsg}</p>}
      <EntityListShell listKey="trainingSessions" listLabel="Training Sessions" columns={columns} {...list} />
      {participantsFor && <ParticipantsModal session={participantsFor} onClose={() => setParticipantsFor(null)} />}
    </div>
  );
}

export default TrainingSessionList;
