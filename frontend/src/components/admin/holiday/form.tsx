"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { HolidayModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveHoliday from "@/services/admin/holiday/save";
import getHoliday from "@/services/admin/holiday/get";
import Loading from "../../common/loader/loader";
import { holidayTypeOptions, yesNoOptions, boolId, yesNoLabel, optionLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const NEW_DEFAULTS: HolidayModel = { holidayType: "Public", isRecurring: false, isActive: true };

function HolidayForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<HolidayModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["holiday", id],
    queryFn: () => getHoliday(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveHoliday(fd);
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
      setFormData({ ...record, date: record.date ? String(record.date).slice(0, 10) : "" });
    } else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["holidays"] });
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
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { name: "date", label: "Date", required: true, type: "date", value: formData.date, onChange: changeHandler, error: formState?.zodErrors?.date },
            { name: "name", label: "Name", placeholder: "e.g. Labour Day", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "nameA", label: "Name (Amharic)", placeholder: "ስም", value: formData.nameA, onChange: changeHandler, error: formState?.zodErrors?.nameA, type: "text" },
            {
              name: "holidayType", label: "Type", type: "dropDown", onSelect: selectHandler,
              value: formData.holidayType, displayValue: optionLabel(holidayTypeOptions, formData.holidayType),
              data: holidayTypeOptions as never,
            },
            {
              name: "isRecurring", label: "Recurs Yearly", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.isRecurring), displayValue: yesNoLabel(formData.isRecurring),
              data: yesNoOptions as never,
            },
            {
              name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive),
              data: yesNoOptions as never,
            },
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default HolidayForm;
