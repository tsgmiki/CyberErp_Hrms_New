"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllReports, deleteReport } from "@/services/admin/report";
import type { ReportDefinitionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function ReportDefinitionList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "reportDefinitions",
    fetchPage: getAllReports,
    deleteById: deleteReport,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "reportName",
          label: "Report",
          sort: true,
          render: (text: string, record: ReportDefinitionModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "reportKey", label: "Key" },
        { name: "reportGrouping", label: "Category" },
        { name: "storedProc", label: "Stored Procedure" },
        {
          name: "fields",
          label: "Parameters",
          render: (_t: unknown, r: ReportDefinitionModel) => String(r.fields?.length ?? 0),
        },
        {
          name: "isActive",
          label: "Status",
          render: (_t: unknown, r: ReportDefinitionModel) => (r.isActive ? "Active" : "Inactive"),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: ReportDefinitionModel) => (
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

  return <EntityListShell listKey="reportDefinitions" listLabel="Report Definitions" columns={columns} {...list} />;
}

export default ReportDefinitionList;
