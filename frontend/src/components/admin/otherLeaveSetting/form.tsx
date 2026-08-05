"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { OtherLeaveSettingModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { getOtherLeaveSetting, saveOtherLeaveSetting } from "@/services/admin/otherLeave";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import getAllLeaveType from "@/services/admin/leaveType/getAll";
import { parameterInitialData } from "@/constants/initialization";
import {
  otherLeaveGenderOptions, otherLeaveGenderLabel,
  otherLeaveDayCountingOptions, otherLeaveDayCountingLabel, boolId,
} from "@/constants/leave";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);

function OtherLeaveSettingForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<OtherLeaveSettingModel>({ gender: "All", dayCounting: "WorkingDays", isActive: true });

  // stale-form guard: when the id is cleared (back / Add-new) while this form stays
  // mounted, drop the previously loaded record so Add never shows stale values.
  useEffect(() => {
    if (!id) setFormData({ gender: "All", dayCounting: "WorkingDays", isActive: true });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["otherLeaveSetting", id],
    queryFn: () => getOtherLeaveSetting(id),
    enabled: typeof id != "undefined" && id != "",
  });
  useEffect(() => { if (record) setFormData(record); }, [record]);

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscalYears", "otherLeaveLookup"],
    queryFn: () => getAllFiscalYear({ ...parameterInitialData, take: 50 }),
    staleTime: 60_000,
  });
  const fiscalOptions = (fiscalYears?.data ?? []).map((f) => ({
    id: f.id!, name: `${f.name}${f.isActive ? " (active)" : ""}`,
  }));

  const { data: leaveTypes } = useQuery({
    queryKey: ["leaveTypes", "otherLeaveLookup"],
    queryFn: () => getAllLeaveType({ ...parameterInitialData, take: 100 }),
    staleTime: 60_000,
  });
  const leaveTypeOptions = (leaveTypes?.data ?? []).map((t: any) => ({
    id: t.id, name: `${t.code} — ${t.name}`,
  }));

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const res = await saveOtherLeaveSetting({
      ...formData,
      standardDays: Number(formData.standardDays),
      managerialDays: Number(formData.managerialDays),
      isLumpSum: formData.isLumpSum === true || String(formData.isLumpSum) === "true",
      isActive: formData.isActive === true || String(formData.isActive) === "true",
    });
    setFormState(res.ok
      ? { status: "success", message: res.message }
      : { status: "error", message: res.message, zodErrors: {} });
    setIsLoading(false);
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ["otherLeaveSettings"] });
      queryClient.invalidateQueries({ queryKey: ["otherLeaveBalances"] });
      setId("");
    }
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  return (
    <div className="text-foreground">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          formId: "otherLeaveSettingForm",
          components: [
            {
              name: "fiscalYearId", label: "Fiscal Year", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.fiscalYearId, displayValue: formData.fiscalYearName,
              error: formState?.zodErrors?.fiscalYearId, data: fiscalOptions as never,
            },
            {
              name: "leaveTypeId", label: "Leave Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.leaveTypeId, displayValue: formData.name,
              error: formState?.zodErrors?.leaveTypeId, data: leaveTypeOptions as never,
            },
            {
              name: "gender", label: "Gender Eligibility", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.gender ?? "All", displayValue: otherLeaveGenderLabel(formData.gender ?? "All"),
              data: otherLeaveGenderOptions as never,
            },
            {
              name: "standardDays", label: "Standard Days", required: true, type: "text", inputType: "number",
              value: formData.standardDays as never, onChange: changeHandler, error: formState?.zodErrors?.standardDays,
            },
            {
              name: "managerialDays", label: "Managerial Days", required: true, type: "text", inputType: "number",
              value: formData.managerialDays as never, onChange: changeHandler, error: formState?.zodErrors?.managerialDays,
            },
            {
              name: "isLumpSum", label: "Lump-sum (one block)", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.isLumpSum),
              displayValue: formData.isLumpSum === true || String(formData.isLumpSum) === "true" ? "Yes" : "No",
              data: [{ id: "true", name: "Yes" }, { id: "false", name: "No" }] as never,
            },
            {
              name: "dayCounting", label: "Holiday & Weekend Handling", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.dayCounting ?? "WorkingDays",
              displayValue: otherLeaveDayCountingLabel(formData.dayCounting ?? "WorkingDays"),
              error: formState?.zodErrors?.dayCounting, data: otherLeaveDayCountingOptions as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive === true || String(formData.isActive) === "true"),
              displayValue: activeLabel(formData.isActive === true || String(formData.isActive) === "true"),
              data: activeStatusOptions as never,
            },
            {
              name: "description", label: "Description", type: "textarea", colSpan: "full",
              value: formData.description, onChange: changeHandler,
            },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <p className="mt-3 rounded-md bg-secondary/30 px-3 py-2 text-xs text-muted">
        Static entitlement — it never accrues. Managerial positions receive the managerial allocation;
        everyone else the standard one. Usage never touches the annual-leave ledger.
      </p>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(OtherLeaveSettingForm);
