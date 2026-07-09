"use client";

import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Star } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllCandidates, deleteCandidate } from "@/services/admin/recruitment";
import type { CandidateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const FILTERS = [
  { id: "", label: "Active" },
  { id: "TalentPool", label: "Talent Pool" },
  { id: "Internal", label: "Internal" },
  { id: "Archived", label: "Archived" },
];

function CandidateList({ editHandler }: Props) {
  const { t } = useTranslation();

  const list = useEntityList({
    queryKey: "candidates",
    fetchPage: getAllCandidates,
    deleteById: deleteCandidate,
  });

  const activeFilter = (list.param.status as string) || "";
  const setFilter = (status: string) =>
    list.setParam((p) => ({ ...p, status: status || undefined, skip: 0 }) as never);

  const columns = useMemo(
    () =>
      [
        {
          name: "fullName",
          label: "Candidate",
          sort: true,
          render: (text: string, r: CandidateModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="flex items-center gap-1.5 font-semibold">
                {text}
                {r.isInTalentPool && <Star size={13} className="text-warning" />}
              </span>
              <span className="block text-xs text-muted">
                {r.candidateNumber}
                {r.email ? ` · ${r.email}` : ""}
              </span>
            </button>
          ),
        },
        {
          name: "source",
          label: "Source",
          render: (v: string) => <span className="rounded bg-secondary px-2 py-0.5 text-xs">{t(v)}</span>,
        },
        {
          name: "skillsSummary",
          label: "Skills",
          render: (v: string) => (
            <span className="block max-w-[240px] truncate text-xs text-muted" title={v}>
              {v || "—"}
            </span>
          ),
        },
        {
          name: "yearsOfExperience",
          label: "Exp (Yrs)",
          render: (v: number) => <span className="tabular-nums">{v ?? "—"}</span>,
        },
        {
          name: "applicationCount",
          label: "Applications",
          render: (v: number) => <span className="tabular-nums">{v ?? 0}</span>,
        },
        {
          name: "anonymizedAt",
          label: "Status",
          render: (v: string, r: CandidateModel) =>
            v ? (
              <span className="rounded bg-muted/30 px-2 py-0.5 text-xs text-muted">{t("Anonymized")}</span>
            ) : r.isArchived ? (
              <span className="rounded bg-info/15 px-2 py-0.5 text-xs text-info">{t("Archived")}</span>
            ) : (
              <span className="rounded bg-success/15 px-2 py-0.5 text-xs text-success">{t("Active")}</span>
            ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: CandidateModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete={(record.applicationCount ?? 0) === 0}
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col">
      <div className="mb-2 flex items-center gap-1 px-1">
        {FILTERS.map((f) => (
          <button
            key={f.id}
            type="button"
            onClick={() => setFilter(f.id)}
            className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
              activeFilter === f.id
                ? "bg-primary/10 text-primary"
                : "text-muted hover:bg-secondary hover:text-foreground"
            }`}
          >
            {t(f.label)}
          </button>
        ))}
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell listKey="candidates" listLabel="Candidates" columns={columns} {...list} />
      </div>
    </div>
  );
}

export default CandidateList;
