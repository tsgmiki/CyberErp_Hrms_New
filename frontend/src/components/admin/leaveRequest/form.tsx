"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import type { LeaveRequestModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveLeaveRequest from "@/services/admin/leaveRequest/save";
import getLeaveRequest from "@/services/admin/leaveRequest/get";
import getWorkingDays from "@/services/admin/leaveRequest/workingDays";
import getLeaveBalances from "@/services/admin/leaveBalance/getByEmployee";
import getAllEmployee from "@/services/admin/employee/getAll";
import getAllLeaveType from "@/services/admin/leaveType/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { dayPartOptions, optionLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };
const NEW_DEFAULTS: LeaveRequestModel = { dayPart: "Full" };

function LeaveRequestForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const viewing = typeof id !== "undefined" && id !== "";

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<LeaveRequestModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["leaveRequest", id],
    queryFn: () => getLeaveRequest(id),
    enabled: viewing,
  });

  const [empParam, setEmpParam] = useState({ ...lookupParam });
  const { data: employees, isLoading: empLoading } = useQuery({
    queryKey: ["employees", empParam],
    queryFn: () => getAllEmployee(empParam),
  });
  const [typeParam, setTypeParam] = useState({ ...lookupParam });
  const { data: types, isLoading: typesLoading } = useQuery({
    queryKey: ["leaveTypes", typeParam],
    queryFn: () => getAllLeaveType(typeParam),
  });

  const halfDay = formData.dayPart !== "Full";
  const rangeReady = !!formData.startDate && !!formData.endDate && formData.endDate >= formData.startDate;

  const { data: preview } = useQuery({
    queryKey: ["workingDays", formData.startDate, formData.endDate, halfDay],
    queryFn: () => getWorkingDays(formData.startDate!, formData.endDate!, halfDay),
    enabled: !viewing && rangeReady,
  });
  const { data: balances } = useQuery({
    queryKey: ["leaveBalances", formData.employeeId],
    queryFn: () => getLeaveBalances(formData.employeeId!),
    enabled: !viewing && !!formData.employeeId,
  });
  const available = useMemo(() => {
    const b = (balances ?? []).find((x) => x.leaveTypeId === formData.leaveTypeId);
    return b?.available;
  }, [balances, formData.leaveTypeId]);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveLeaveRequest(fd);
    setFormState(result);
    setIsLoading(false);
  };
  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  useEffect(() => {
    if (viewing && record) {
      setFormData({ ...record, startDate: (record.startDate || "").slice(0, 10), endDate: (record.endDate || "").slice(0, 10) });
    } else if (!viewing) setFormData({ ...NEW_DEFAULTS });
  }, [record, viewing]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["leaveRequests"] });
      setId("");
    }
  }, [formState]);

  // Read-only detail view for an existing request.
  if (viewing) {
    const row = (label: string, value: React.ReactNode) => (
      <div className="flex gap-2 py-1"><span className="w-40 text-muted">{label}</span><span className="font-medium">{value}</span></div>
    );
    return (
      <div className="text-foreground">
        {pending && <Loading />}
        {record && (
          <div className="max-w-xl rounded-lg border border-border p-4 text-sm">
            {row("Employee", `${record.employeeName ?? ""} (${record.employeeNumber ?? ""})`)}
            {row("Leave Type", record.leaveTypeName)}
            {row("Period", `${(record.startDate || "").slice(0, 10)} → ${(record.endDate || "").slice(0, 10)}`)}
            {row("Day Part", record.dayPart)}
            {row("Working Days", record.workingDays)}
            {row("Status", record.status)}
            {row("Reason", record.reason || "—")}
            {record.decisionComment && row("Decision", record.decisionComment)}
            {record.cancelReason && row("Cancel Reason", record.cancelReason)}
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="text-white">
      {(preview || available != null) && (
        <div className="mb-3 flex flex-wrap gap-4 rounded-lg border border-border bg-muted/20 px-4 py-2 text-sm text-foreground">
          <span>Working days: <b>{rangeReady ? preview?.workingDays ?? "…" : "—"}</b></span>
          <span>Available balance: <b>{available != null ? available : "—"}</b></span>
          {rangeReady && preview && available != null && preview.workingDays > available && (
            <span className="font-medium text-error">Exceeds available balance</span>
          )}
        </div>
      )}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "employeeId", label: "Employee", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.employeeId, displayValue: formData.employeeName,
              error: formState?.zodErrors?.employeeId,
              param: empParam, setParam: setEmpParam as any, isLoading: empLoading,
              data: (employees?.data ?? []).map((e: any) => ({ id: e.id, name: e.fullName || e.employeeNumber })) as never,
            },
            {
              name: "leaveTypeId", label: "Leave Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.leaveTypeId, displayValue: formData.leaveTypeName,
              error: formState?.zodErrors?.leaveTypeId,
              param: typeParam, setParam: setTypeParam as any, isLoading: typesLoading,
              data: (types?.data ?? []).map((t: any) => ({ id: t.id, name: `${t.code} — ${t.name}` })) as never,
            },
            { name: "startDate", label: "Start Date", required: true, type: "date", value: formData.startDate, onChange: changeHandler, error: formState?.zodErrors?.startDate },
            { name: "endDate", label: "End Date", required: true, type: "date", value: formData.endDate, onChange: changeHandler, error: formState?.zodErrors?.endDate },
            {
              name: "dayPart", label: "Day Part", type: "dropDown", onSelect: selectHandler,
              value: formData.dayPart, displayValue: optionLabel(dayPartOptions, formData.dayPart),
              data: dayPartOptions as never,
            },
            { name: "reason", label: "Reason", value: formData.reason, onChange: changeHandler, type: "textarea", colSpan: "full" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default LeaveRequestForm;
