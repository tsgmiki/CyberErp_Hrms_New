"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { WorkWeekConfigurationModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveWorkWeekConfiguration from "@/services/admin/workWeekConfiguration/save";
import getWorkWeekConfiguration from "@/services/admin/workWeekConfiguration/get";
import Loading from "../../common/loader/loader";
import {
  dayModeOptions,
  weekDays,
  boolId,
  yesNoLabel,
  yesNoOptions,
  optionLabel,
} from "@/constants/leave";

const FormProvider = memo(FormProviders);

// Standard Mon–Fri working week; Saturday & Sunday rest by default.
const NEW_DEFAULTS: WorkWeekConfigurationModel = {
  monday: "Full",
  tuesday: "Full",
  wednesday: "Full",
  thursday: "Full",
  friday: "Full",
  saturday: "Rest",
  sunday: "Rest",
  isActive: true,
};

function WorkWeekConfigurationForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<WorkWeekConfigurationModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["workWeekConfiguration", id],
    queryFn: () => getWorkWeekConfiguration(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveWorkWeekConfiguration(fd);
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
      queryClient.invalidateQueries({ queryKey: ["workWeekConfigurations"] });
      setId("");
    }
  }, [formState]);

  // One dropDown per weekday (Full / Half / Rest).
  const dayField = (key: string, label: string) => ({
    name: key,
    label,
    type: "dropDown" as const,
    onSelect: selectHandler,
    value: (formData as any)[key],
    displayValue: optionLabel(dayModeOptions, (formData as any)[key]),
    data: dayModeOptions as never,
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
            {
              name: "name",
              label: "Name",
              placeholder: "e.g. Standard (Mon–Fri)",
              required: true,
              value: formData.name,
              onChange: changeHandler,
              error: formState?.zodErrors?.name,
              type: "text",
            },
            {
              name: "isActive",
              label: "Active",
              type: "dropDown",
              onSelect: selectHandler,
              value: boolId(formData.isActive),
              displayValue: yesNoLabel(formData.isActive),
              data: yesNoOptions as never,
            },
            ...weekDays.map((d) => dayField(d.key, d.label)),
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <p className="mt-2 text-xs text-muted">
        Only one configuration is active at a time — activating this one deactivates the others.
        A half day counts as 0.5 and a rest day as 0 when calculating leave.
      </p>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default WorkWeekConfigurationForm;
