"use client";

import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import { getAllJobRequisitions, deleteJobRequisition } from "@/services/admin/recruitment";
import type { JobRequisitionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  PendingApproval: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Posted: "bg-info/15 text-info",
  Closed: "bg-secondary text-foreground",
  Cancelled: "bg-muted/30 text-muted",
  Rejected: "bg-error/15 text-error",
};

interface Props {
  editHandler: (id: string) => void;
}

function JobRequisitionList({ editHandler }: Props) {
  const { t } = useTranslation();

  const list = useEntityList({
    queryKey: "jobRequisitions",
    fetchPage: getAllJobRequisitions,
    deleteById: deleteJobRequisition,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "requisitionNumber",
          label: "Requisition",
          sort: true,
          render: (text: string, r: JobRequisitionModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">
                {text} — {r.title}
              </span>
              <span className="block text-xs text-muted">
                {r.organizationUnitName} · {t("from")} {r.hiringRequestNumber}
              </span>
            </button>
          ),
        },
        {
          name: "numberOfPositions",
          label: "Openings",
          render: (v: number, r: JobRequisitionModel) => (
            <span className="tabular-nums">
              {v} <span className="text-xs text-muted">({t(r.employmentType ?? "")})</span>
            </span>
          ),
        },
        {
          name: "postingChannel",
          label: "Channel",
          render: (v: string, r: JobRequisitionModel) =>
            r.status === "Posted" || r.postingText ? t(v) : <span className="text-xs text-muted">—</span>,
        },
        {
          name: "applicationCount",
          label: "Applications",
          render: (v: number) => <span className="tabular-nums">{v ?? 0}</span>,
        },
        {
          name: "status",
          label: "Status",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text] ?? ""}`}>
              {t(text)}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: JobRequisitionModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete={
                record.status === "Draft" || record.status === "Rejected" || record.status === "Cancelled"
              }
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
      listKey="jobRequisitions"
      listLabel="Job Requisitions"
      columns={columns}
      {...list}
    />
  );
}

export default JobRequisitionList;
