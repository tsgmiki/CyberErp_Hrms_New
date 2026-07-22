"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllCareerPath from "@/services/admin/careerPath/getAll";
import deleteCareerPath from "@/services/admin/careerPath/delete";
import type { CareerPathModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

function CareerPathList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({
    queryKey: "careerPaths",
    fetchPage: getAllCareerPath,
    deleteById: deleteCareerPath,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name", label: "Career Path", sort: true,
          render: (t: string, r: CareerPathModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{t}</span>
              <span className="block text-xs text-muted">{r.code}</span>
            </button>
          ),
        },
        { name: "description", label: "Description" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: CareerPathModel) => (
            <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
              editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="careerPaths" listLabel="Career Paths" columns={columns} {...list} />;
}

export default CareerPathList;
