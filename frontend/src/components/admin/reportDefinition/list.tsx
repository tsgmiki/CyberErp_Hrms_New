"use client";
import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Sprout } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import ButtonField from "@/components/ui/buttonField";
import { getAllReports, deleteReport, seedDefaultReports } from "@/services/admin/report";
import type { ReportDefinitionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function ReportDefinitionList({ editHandler }: Props) {
  const queryClient = useQueryClient();
  const [seeding, setSeeding] = useState(false);
  const [seedMsg, setSeedMsg] = useState("");
  const list = useEntityList({
    queryKey: "reportDefinitions",
    fetchPage: getAllReports,
    deleteById: deleteReport,
  });

  const seed = async () => {
    setSeeding(true);
    const res = await seedDefaultReports();
    setSeeding(false);
    setSeedMsg(res.message);
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ["reportDefinitions"] });
      queryClient.invalidateQueries({ queryKey: ["reportCatalog"] });
    }
  };

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

  return (
    <EntityListShell
      listKey="reportDefinitions"
      listLabel="Report Definitions"
      columns={columns}
      header={
        <div className="flex flex-wrap items-center justify-end gap-2">
          {seedMsg && <span className="text-xs text-muted">{seedMsg}</span>}
          <ButtonField
            value={seeding ? "Seeding…" : "Seed Standard Reports"}
            variant="outline"
            icon={<Sprout size={14} />}
            disabled={seeding}
            onClick={seed}
          />
        </div>
      }
      {...list}
    />
  );
}

export default ReportDefinitionList;
