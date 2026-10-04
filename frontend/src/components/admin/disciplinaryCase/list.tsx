"use client";

import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Pencil, Trash2, Ban, Award, TrendingUp } from "lucide-react";
import { getAllDisciplinaryCases } from "@/services/admin/disciplinaryCase";
import { deleteDisciplinaryMeasure } from "@/services/admin/employee/personnelActions";
import Badge, { type BadgeVariant } from "@/components/common/badge/badge";
import type { DisciplinaryMeasureModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import {
  measureTypeLabel,
  disciplinaryStatusLabel,
  disciplinaryStatusOptions,
} from "@/constants/orgStructure";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` is a `DisciplinaryStatus` ENUM NAME — `Enum.TryParse<DisciplinaryStatus>(…, true, …)`.
 *
 * Options are DERIVED from `disciplinaryStatusOptions`, the same constant the form's dropdown and
 * `disciplinaryStatusLabel` already use, so the filter cannot drift out of step with the statuses
 * the rest of the screen offers.
 */
const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      ...disciplinaryStatusOptions.map((o) => ({ value: o.id, label: o.name })),
    ],
  },
];

/** Case status → the shared Badge's variants, which the hand-rolled tones mapped onto anyway. */
const STATUS_VARIANT: Record<string, BadgeVariant> = {
  Open: "warning",
  UnderReview: "info",
  Resolved: "success",
  Cancelled: "muted",
};

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : null);

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
          gridPrimary: true,
          gridHideLabel: true,
          render: (text: string, record: DisciplinaryMeasureModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold text-primary hover:underline">{text || "—"}</span>
              <span className="block font-mono text-xs text-muted">{record.employeeNumber}</span>
            </button>
          ),
        },
        {
          name: "violationType",
          label: "Violation",
          sort: true,
          width: "w-44",
          render: (text: string) =>
            text ? <span className="line-clamp-2">{text}</span> : dash,
        },
        {
          name: "measureType",
          label: "Measure",
          gridHighlight: true,
          render: (v: string) => {
            const label = measureTypeLabel(String(v ?? ""));
            return label ? <Badge variant="secondary">{label}</Badge> : dash;
          },
        },
        {
          name: "violationDate",
          label: "Violation Date",
          sort: true,
          responsive: "md",
          render: (v: unknown) => {
            const d = fmtDate(v);
            return d ? <span className="tabular-nums">{d}</span> : dash;
          },
        },
        {
          name: "validUntil",
          label: "Lifetime",
          render: (v: unknown, r: DisciplinaryMeasureModel) => (
            <span className="flex flex-wrap items-center gap-1.5">
              <span className="whitespace-nowrap text-xs tabular-nums">
                {v ? `${t("until")} ${fmtDate(v)}` : t("open-ended")}
              </span>
              {/* ⚠️ These are not decoration: each one is a HARD BLOCK that the eligibility service
                  enforces against a reward, a promotion or a salary increment. They are the reason
                  this column exists, so they stay visually loud. */}
              {r.affectsPromotion && (
                <Badge variant="error" title={t("Blocks promotion") ?? ""}>
                  <Ban size={10} /> {t("Promo")}
                </Badge>
              )}
              {r.affectsReward && (
                <Badge variant="error" title={t("Blocks reward") ?? ""}>
                  <Award size={10} /> {t("Reward")}
                </Badge>
              )}
              {/* `!== false`, since this one defaults to blocking — a record from before the flag
                  existed has no value and still blocks. */}
              {r.affectsSalaryIncrement !== false && (
                <Badge variant="error" title={t("Blocks salary increment") ?? ""}>
                  <TrendingUp size={10} /> {t("Increment")}
                </Badge>
              )}
            </span>
          ),
        },
        {
          name: "raisedByName",
          label: "Raised By",
          responsive: "lg",
          render: (v: string) => v || t("HR"),
        },
        {
          name: "status",
          label: "Status",
          gridHighlight: true,
          render: (text: string) =>
            text ? (
              <Badge variant={STATUS_VARIANT[text] ?? "muted"}>
                {disciplinaryStatusLabel(String(text))}
              </Badge>
            ) : (
              dash
            ),
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
          // ⚠️ Edit carried `hover:bg-primary/10`, which is unregistered and emits nothing, while
          // Delete's `hover:bg-error/10` happens to be in theme.css — so one button responded to
          // the pointer and the other did not. Both use the same core-Tailwind opacity now.
          render: (_t: unknown, r: DisciplinaryMeasureModel) => (
            <span className="inline-flex items-center gap-0.5">
              <button type="button" title={t("Edit") ?? ""} onClick={() => r.id && editHandler(r.id)}
                className="rounded p-1 text-primary transition-opacity hover:opacity-70">
                <Pencil size={15} />
              </button>
              <button type="button" title={t("Delete") ?? ""} onClick={() => r.id && list.deleteRecord(r.id)}
                className="rounded p-1 text-error transition-opacity hover:opacity-70">
                <Trash2 size={15} />
              </button>
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <EntityListShell
      listKey="disciplinaryCases"
      listLabel="Disciplinary Cases"
      columns={columns}
      listFilters={STATUS_FILTER}
      {...list}
    />
  );
}

export default DisciplinaryCaseList;
