"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { TrendingUp } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllLearningPaths, deleteLearningPath } from "@/services/admin/learningPath";
import type { LearningPathModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import ProgressModal from "./progressModal";

interface Props {
  editHandler: (id: string) => void;
}

function LearningPathList({ editHandler }: Props) {
  const { t } = useTranslation();
  const [progressFor, setProgressFor] = useState<LearningPathModel | null>(null);

  const list = useEntityList({
    queryKey: "learningPaths",
    fetchPage: getAllLearningPaths,
    deleteById: deleteLearningPath,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Path",
          sort: true,
          render: (text: string, record: LearningPathModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text}</span>
              <span className="block text-xs text-muted">{record.description}</span>
            </button>
          ),
        },
        { name: "stepCount", label: "Courses", render: (v: number) => <span className="font-semibold">{v ?? 0}</span> },
        { name: "targetPositionName", label: "Target Position", render: (v: string) => v || "—" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: LearningPathModel) => (
            <span className="flex items-center gap-1.5">
              <button type="button" title={t("Employee progress")} onClick={() => setProgressFor(record)} className="rounded p-1 text-muted hover:text-primary">
                <TrendingUp size={15} />
              </button>
              <GridAction
                id={record.id || ""}
                record={record}
                showAdd={false}
                showEdit
                showDelete
                editHandler={editHandler}
                deleteHandler={() => record.id && list.deleteRecord(record.id)}
              />
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="space-y-2">
      <EntityListShell listKey="learningPaths" listLabel="Learning Paths" columns={columns} {...list} />
      {progressFor && <ProgressModal path={progressFor} onClose={() => setProgressFor(null)} />}
    </div>
  );
}

export default LearningPathList;
