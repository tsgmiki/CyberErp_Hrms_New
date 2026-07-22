"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { PerDiemRateModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import savePerDiemRate from "@/services/admin/perDiemRate/save";
import getPerDiemRate from "@/services/admin/perDiemRate/get";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { yesNoOptions, boolId, yesNoLabel, optionLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const TRIP_TYPE_OPTIONS = ["Local", "International"].map((x) => ({ id: x, name: x }));

const NEW_DEFAULTS: PerDiemRateModel = {
  tripType: "Local",
  currency: "ETB",
  isActive: true,
};

function PerDiemRateForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<PerDiemRateModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["perDiemRate", id],
    queryFn: () => getPerDiemRate(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: grades, isLoading: gradesLoading } = useQuery({
    queryKey: ["jobGradeOptions"],
    queryFn: () => getAllJobGrade({ ...parameterInitialData, take: 200 }),
    staleTime: 60_000,
  });
  const gradeOptions = (grades?.data ?? []).map((g) => ({ id: g.id!, name: g.name ?? "" }));

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await savePerDiemRate(fd);
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
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["perDiemRates"] });
      setId("");
    }
  }, [formState]);

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
            { name: "jobGradeId", label: "Job Grade", required: true, type: "dropDown", onSelect: selectHandler, value: formData.jobGradeId, displayValue: optionLabel(gradeOptions, formData.jobGradeId) || formData.jobGradeName, error: formState?.zodErrors?.jobGradeId, isLoading: gradesLoading, data: gradeOptions as never },
            { name: "tripType", label: "Trip Type", type: "dropDown", onSelect: selectHandler, value: formData.tripType, displayValue: optionLabel(TRIP_TYPE_OPTIONS, formData.tripType), data: TRIP_TYPE_OPTIONS as never },
            { name: "dailyRate", label: "Daily Rate", value: formData.dailyRate, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "currency", label: "Currency", value: formData.currency, onChange: changeHandler, type: "text" },
            { name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive), data: yesNoOptions as never },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default PerDiemRateForm;
