"use client";

import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Pencil, Trash2, Ban, Award } from "lucide-react";
import { getAllDisciplinaryCases } from "@/services/admin/disciplinaryCase";
import { deleteDisciplinaryMeasure } from "@/services/admin/employee/personnelActions";
import type { DisciplinaryMeasureModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { measureTypeLabel, disciplinaryStatusLabel } from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Open: "bg-warning/15 text-warning",
  UnderReview: "bg-info/15 text-info",
  Resolved: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function DisciplinaryCaseList({ editHandler }: Props) {
  const { t } = useTranslation();

  const list = useEntityList({
    queryKey: "disciplinaryCases",
    fetchPage: getAllDisciplinaryCases,
    deleteById: deleteDisciplinaryMeasure,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: DisciplinaryMeasureModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">{record.employeeNumber}</span>
            </button>
          ),
        },
        { name: "violationType", label: "Violation", sort: true },
        {
          name: "measureType",
          label: "Measure",
          render: (v: string) => measureTypeLabel(String(v ?? "")),
        },
        { name: "violationDate", label: "Violation Date", sort: true, render: fmtDate },
        {
          name: "validUntil",
          label: "Lifetime",
          render: (v: unknown, r: DisciplinaryMeasureModel) => (
            <span className="inline-flex items-center gap-1.5">
              <span className="text-xs">{v ? `${t("until")} ${fmtDate(v)}` : t("open-ended")}</span>
              {r.affectsPromotion && (
                <span title={t("Blocks promotion") ?? ""} className="rounded bg-error/10 px-1 py-0.5 text-[10px] font-semibold text-error">
                  <Ban size={10} className="inline" /> {t("Promo")}
                </span>
              )}
              {r.affectsReward && (
                <span title={t("Blocks reward") ?? ""} className="rounded bg-error/10 px-1 py-0.5 text-[10px] font-semibold text-error">
                  <Award size={10} className="inline" /> {t("Reward")}
                </span>
              )}
            </span>
          ),
        },
        { name: "raisedByName", label: "Raised By", render: (v: string) => v || t("HR") },
        {
          name: "status",
          label: "Status",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text] ?? "bg-muted/30 text-muted"}`}>
              {disciplinaryStatusLabel(String(text ?? ""))}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, r: DisciplinaryMeasureModel) => (
            <span className="inline-flex items-center gap-0.5">
              <button type="button" title={t("Edit") ?? ""} onClick={() => r.id && editHandler(r.id)}
                className="rounded p-1 text-primary hover:bg-primary/10">
                <Pencil size={15} />
              </button>
              <button type="button" title={t("Delete") ?? ""} onClick={() => r.id && list.deleteRecord(r.id)}
                className="rounded p-1 text-error hover:bg-error/10">
                <Trash2 size={15} />
              </button>
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return <EntityListShell listKey="disciplinaryCases" listLabel="Disciplinary Cases" columns={columns} {...list} />;
}

export default DisciplinaryCaseList;
