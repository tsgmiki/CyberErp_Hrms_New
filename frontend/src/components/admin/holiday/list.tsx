"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllHoliday from "@/services/admin/holiday/getAll";
import deleteHoliday from "@/services/admin/holiday/delete";
import type { HolidayModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const fmtDate = (v?: string) => (v ? String(v).slice(0, 10) : "");

function HolidayList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "holidays",
    fetchPage: getAllHoliday,
    deleteById: deleteHoliday,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "date",
          label: "Date",
          sort: true,
          render: (_t: unknown, record: HolidayModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {fmtDate(record.date)}
            </button>
          ),
        },
        { name: "name", label: "Name", sort: true },
        { name: "holidayType", label: "Type" },
        {
          name: "isRecurring",
          label: "Recurring",
          render: (_t: unknown, r: HolidayModel) => (r.isRecurring ? "Yes" : "No"),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: HolidayModel) => (
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

  return <EntityListShell listKey="holidays" listLabel="Holidays" columns={columns} {...list} />;
}

export default HolidayList;
