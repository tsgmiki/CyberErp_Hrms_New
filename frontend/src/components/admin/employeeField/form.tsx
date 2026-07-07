"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { EmployeeFieldModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveEmployeeField from "@/services/admin/employeeField/save";
import getEmployeeField from "@/services/admin/employeeField/get";
import Loading from "../../common/loader/loader";
import {
  fieldDataTypeOptions,
  yesNoOptions,
  activeStatusOptions,
  activeId,
  activeLabel,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);

function EmployeeFieldForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as EmployeeFieldModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["employeeField", id],
    queryFn: () => getEmployeeField(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveEmployeeField(fd);
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
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as EmployeeFieldModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["employeeFields"] });
      setId("");
    }
  }, [formState]);

  const isSelect = formData.dataType === "Select";

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
            { name: "label", label: "Label", placeholder: "e.g. Blood Type", required: true, value: formData.label, onChange: changeHandler, error: formState?.zodErrors?.label, type: "text" },
            { name: "name", label: "Field Key", placeholder: "e.g. bloodType", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "dataType", label: "Data Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.dataType, displayValue: formData.dataType,
              error: formState?.zodErrors?.dataType, data: fieldDataTypeOptions as never,
            },
            {
              name: "isRequired", label: "Required", type: "dropDown", onSelect: selectHandler,
              value: formData.isRequired === true ? "true" : "false",
              displayValue: formData.isRequired === true ? "Yes" : "No",
              data: yesNoOptions as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "sortOrder", label: "Sort Order", value: formData.sortOrder, onChange: changeHandler, inputType: "number", type: "text" },
            ...(isSelect
              ? [{
                  name: "options", label: "Options (comma-separated)", placeholder: "e.g. A+,A-,B+,O+",
                  required: true, value: formData.options, onChange: changeHandler,
                  type: "textarea" as const, colSpan: "full" as const,
                }]
              : [{ name: "options", value: formData.options ?? "", type: "hidden" as const }]),
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default EmployeeFieldForm;
