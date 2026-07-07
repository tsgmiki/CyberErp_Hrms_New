"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { LeaveTypeModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveLeaveType from "@/services/admin/leaveType/save";
import getLeaveType from "@/services/admin/leaveType/get";
import Loading from "../../common/loader/loader";
import {
  yesNoOptions,
  leaveAccrualOptions,
  genderEligibilityOptions,
  boolId,
  yesNoLabel,
  optionLabel,
} from "@/constants/leave";

const FormProvider = memo(FormProviders);

const NEW_DEFAULTS: LeaveTypeModel = {
  isPaid: true,
  requiresApproval: true,
  allowHalfDay: false,
  genderEligibility: "Any",
  accrualMethod: "Annual",
  isActive: true,
};

function LeaveTypeForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<LeaveTypeModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["leaveType", id],
    queryFn: () => getLeaveType(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveLeaveType(fd);
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
    if (typeof record != "undefined" && record != null) setFormData(record);
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["leaveTypes"] });
      setId("");
    }
  }, [formState]);

  const boolField = (name: keyof LeaveTypeModel, label: string) => ({
    name, label, type: "dropDown" as const, onSelect: selectHandler,
    value: boolId(formData[name]), displayValue: yesNoLabel(formData[name]),
    data: yesNoOptions as never,
  });

  return (
    <div className="text-white">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[40%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { name: "code", label: "Code", placeholder: "e.g. AL", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
            { name: "name", label: "Name", placeholder: "e.g. Annual Leave", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "nameA", label: "Name (Amharic)", placeholder: "ስም", value: formData.nameA, onChange: changeHandler, error: formState?.zodErrors?.nameA, type: "text" },
            {
              name: "accrualMethod", label: "Accrual Method", type: "dropDown", onSelect: selectHandler,
              value: formData.accrualMethod, displayValue: optionLabel(leaveAccrualOptions, formData.accrualMethod),
              data: leaveAccrualOptions as never,
            },
            { name: "defaultAnnualEntitlement", label: "Annual Entitlement (days)", value: formData.defaultAnnualEntitlement, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "carryForwardMaxDays", label: "Carry-forward Max (days)", value: formData.carryForwardMaxDays, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "maxConsecutiveDays", label: "Max Consecutive Days", value: formData.maxConsecutiveDays, onChange: changeHandler, inputType: "number", type: "text" },
            {
              name: "genderEligibility", label: "Gender Eligibility", type: "dropDown", onSelect: selectHandler,
              value: formData.genderEligibility, displayValue: optionLabel(genderEligibilityOptions, formData.genderEligibility),
              data: genderEligibilityOptions as never,
            },
            boolField("isPaid", "Paid Leave"),
            boolField("requiresApproval", "Requires Approval"),
            boolField("allowHalfDay", "Allow Half-day"),
            boolField("isActive", "Active"),
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default LeaveTypeForm;
