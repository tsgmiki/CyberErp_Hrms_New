"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllReviewCycle from "@/services/admin/reviewCycle/getAll";
import deleteReviewCycle from "@/services/admin/reviewCycle/delete";
import type { ReviewCycleModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { reviewPeriodTypeLabel } from "@/constants/performance";

interface Props {
  editHandler: (id: string) => void;
}

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function ReviewCycleList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "reviewCycles",
    fetchPage: getAllReviewCycle,
    deleteById: deleteReviewCycle,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: ReviewCycleModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "periodType", label: "Period", render: (v: string) => reviewPeriodTypeLabel(v) },
        { name: "ratingScaleName", label: "Rating Scale" },
        { name: "startDate", label: "Start", render: fmtDate },
        { name: "endDate", label: "End", render: fmtDate },
        { name: "status", label: "Status" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: ReviewCycleModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return (
    <EntityListShell listKey="reviewCycles" listLabel="Review Cycles" columns={columns} {...list} />
  );
}

export default ReviewCycleList;
