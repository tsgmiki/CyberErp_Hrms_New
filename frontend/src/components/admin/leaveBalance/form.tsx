"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery } from "@tanstack/react-query";
import saveLeaveBalance from "@/services/admin/leaveBalance/save";
import getAllLeaveType from "@/services/admin/leaveType/getAll";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

interface Props {
  employeeId: string;
  employeeName?: string;
  onSaved: () => void;
}

function LeaveBalanceForm({ employeeId, employeeName, onSaved }: Props) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<any>({});
  const formRef = React.createRef<HTMLFormElement>();

  const [typeParam, setTypeParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: types, isLoading: typesLoading } = useQuery({
    queryKey: ["leaveTypes", typeParam],
    queryFn: () => getAllLeaveType(typeParam),
  });
  const [fyParam, setFyParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: fiscalYears, isLoading: fyLoading } = useQuery({
    queryKey: ["fiscalYears", fyParam],
    queryFn: () => getAllFiscalYear(fyParam),
  });

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p: any) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p: any) => ({ ...p, [name]: r.id }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    fd.set("employeeId", employeeId);
    setIsLoading(true);
    const result = await saveLeaveBalance(fd);
    setFormState(result);
    setIsLoading(false);
    if (result.status === "success") onSaved();
  };

  return (
    <div className="text-white">
      <p className="mb-2 text-sm text-muted">Setting balance for <b className="text-foreground">{employeeName}</b></p>
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[40%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "leaveTypeId", label: "Leave Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.leaveTypeId, error: formState?.zodErrors?.leaveTypeId,
              param: typeParam, setParam: setTypeParam as any, isLoading: typesLoading,
              data: (types?.data ?? []).map((t: any) => ({ id: t.id, name: `${t.code} — ${t.name}` })) as never,
            },
            {
              name: "fiscalYearId", label: "Fiscal Year", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.fiscalYearId, error: formState?.zodErrors?.fiscalYearId,
              param: fyParam, setParam: setFyParam as any, isLoading: fyLoading,
              data: (fiscalYears?.data ?? []).filter((f: any) => !f.isClosed).map((f: any) => ({ id: f.id, name: f.name })) as never,
            },
            { name: "entitled", label: "Entitled (days)", value: formData.entitled, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "carriedForward", label: "Carried Forward", value: formData.carriedForward, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "adjusted", label: "Adjustment (+/-)", value: formData.adjusted, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "reason", label: "Reason", value: formData.reason, onChange: changeHandler, type: "text", colSpan: "full" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default LeaveBalanceForm;
