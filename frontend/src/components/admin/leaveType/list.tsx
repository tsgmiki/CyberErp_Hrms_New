"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllLeaveType from "@/services/admin/leaveType/getAll";
import deleteLeaveType from "@/services/admin/leaveType/delete";
import type { LeaveTypeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function LeaveTypeList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "leaveTypes",
    fetchPage: getAllLeaveType,
    deleteById: deleteLeaveType,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: LeaveTypeModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "name", label: "Name", sort: true },
        {
          name: "isPaid",
          label: "Paid",
          render: (_t: unknown, r: LeaveTypeModel) => (r.isPaid ? "Paid" : "Unpaid"),
        },
        { name: "accrualMethod", label: "Accrual" },
        {
          name: "defaultAnnualEntitlement",
          label: "Entitlement (days)",
          render: (_t: unknown, r: LeaveTypeModel) => (r.defaultAnnualEntitlement ?? 0).toString(),
        },
        {
          name: "isActive",
          label: "Status",
          render: (_t: unknown, r: LeaveTypeModel) => (r.isActive ? "Active" : "Inactive"),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: LeaveTypeModel) => (
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

  return <EntityListShell listKey="leaveTypes" listLabel="Leave Types" columns={columns} {...list} />;
}

export default LeaveTypeList;
