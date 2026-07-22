"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { AnnualLeaveSettingModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveSetting from "@/services/admin/annualLeaveSetting/save";
import getSetting from "@/services/admin/annualLeaveSetting/get";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import getAllLeaveType from "@/services/admin/leaveType/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { yesNoOptions, boolId, yesNoLabel, leaveAccrualRuleTypeOptions, leaveAccrualRuleTypeLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

// Ethiopian Labour Proclamation defaults: 16 base + 1 day per 2 service years.
const NEW_DEFAULTS: AnnualLeaveSettingModel = {
  minExperienceMonths: 12,
  newEmployeeLeaveDays: 16,
  baseLeaveDays: 16,
  managerialLeaveDays: 20,
  incrementDays: 1,
  incrementIntervalYears: 2,
  maxLeaveDays: 35,
  expiryYears: 2,
  ruleType: "ServiceYears",
  considerExternalExperience: false,
  preMilestoneBaseLeaveDays: 14,
  preMilestoneIncrementDays: 1,
  preMilestoneIntervalYears: 1,
  isActive: true,
};

function AnnualLeaveSettingForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<AnnualLeaveSettingModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["annualLeaveSetting", id],
    queryFn: () => getSetting(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [fyParam, setFyParam] = useState({ ...lookupParam });
  const { data: fiscalYears, isLoading: fyLoading } = useQuery({
    queryKey: ["fiscalYears", fyParam],
    queryFn: () => getAllFiscalYear(fyParam),
  });
  const [typeParam, setTypeParam] = useState({ ...lookupParam });
  const { data: types, isLoading: typesLoading } = useQuery({
    queryKey: ["leaveTypes", typeParam],
    queryFn: () => getAllLeaveType(typeParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveSetting(fd);
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
    if (typeof record != "undefined" && record != null) {
      setFormData({ ...record, milestoneDate: (record.milestoneDate || "").slice(0, 10) });
    } else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["annualLeaveSettings"] });
      setId("");
    }
  }, [formState]);

  const num = (name: keyof AnnualLeaveSettingModel, label: string) => ({
    name, label, value: formData[name], onChange: changeHandler,
    error: formState?.zodErrors?.[name as string], inputType: "number", type: "text" as const,
  });

  return (
    <div className="text-white">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[45%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "fiscalYearId", label: "Fiscal Year", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.fiscalYearId, displayValue: formData.fiscalYearName,
              error: formState?.zodErrors?.fiscalYearId,
              param: fyParam, setParam: setFyParam as any, isLoading: fyLoading,
              data: (fiscalYears?.data ?? []).filter((f: any) => !f.isClosed).map((f: any) => ({ id: f.id, name: f.name })) as never,
            },
            {
              name: "leaveTypeId", label: "Leave Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.leaveTypeId, displayValue: formData.leaveTypeName,
              error: formState?.zodErrors?.leaveTypeId,
              param: typeParam, setParam: setTypeParam as any, isLoading: typesLoading,
              data: (types?.data ?? []).map((t: any) => ({ id: t.id, name: `${t.code} — ${t.name}` })) as never,
            },
            {
              name: "ruleType", label: "Accrual Rule", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.ruleType, displayValue: leaveAccrualRuleTypeLabel(formData.ruleType),
              error: formState?.zodErrors?.ruleType, data: leaveAccrualRuleTypeOptions as never,
            },
            {
              name: "considerExternalExperience", label: "Count External (Gov) Experience", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.considerExternalExperience), displayValue: yesNoLabel(formData.considerExternalExperience),
              data: yesNoOptions as never,
            },
            num("baseLeaveDays", "Base Leave Days"),
            num("managerialLeaveDays", "Managerial Leave Days"),
            num("incrementDays", "Increment (days)"),
            num("incrementIntervalYears", "Increment Interval (years)"),
            num("maxLeaveDays", "Maximum Leave Days (0 = uncapped)"),
            num("minExperienceMonths", "Min Service Before Leave (months)"),
            num("newEmployeeLeaveDays", "New-Employee Basis (days)"),
            num("expiryYears", "Carry-forward Expiry (years)"),
            // Milestone-split (Rule A / B) parameters — only for the service-milestone rule.
            ...(formData.ruleType === "ServiceMilestone"
              ? [
                  { name: "milestoneDate", label: "Milestone Date", required: true, type: "date" as const, value: formData.milestoneDate, onChange: changeHandler, error: formState?.zodErrors?.milestoneDate },
                  num("preMilestoneBaseLeaveDays", "Pre-Milestone Base Days"),
                  num("preMilestoneIncrementDays", "Pre-Milestone Increment (days)"),
                  num("preMilestoneIntervalYears", "Pre-Milestone Interval (years)"),
                ]
              : []),
            {
              name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive),
              data: yesNoOptions as never,
            },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default AnnualLeaveSettingForm;
