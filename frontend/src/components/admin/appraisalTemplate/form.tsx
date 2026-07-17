"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { AppraisalTemplateModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveAppraisalTemplate from "@/services/admin/appraisalTemplate/save";
import getAppraisalTemplate from "@/services/admin/appraisalTemplate/get";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: AppraisalTemplateModel = { goalsWeight: 60, competenciesWeight: 40, isActive: true };

function AppraisalTemplateForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<AppraisalTemplateModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["appraisalTemplate", id],
    queryFn: () => getAppraisalTemplate(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveAppraisalTemplate(fd);
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
      queryClient.invalidateQueries({ queryKey: ["appraisalTemplates"] });
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
          labelWidth: "w-[30%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { name: "name", label: "Name", placeholder: "e.g. Standard 60/40", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "goalsWeight", label: "Goals Weight (%)", placeholder: "60", required: true, value: formData.goalsWeight, onChange: changeHandler, error: formState?.zodErrors?.goalsWeight, inputType: "number", type: "text" },
            { name: "competenciesWeight", label: "Competencies Weight (%)", placeholder: "40", required: true, value: formData.competenciesWeight, onChange: changeHandler, error: formState?.zodErrors?.competenciesWeight, inputType: "number", type: "text" },
            { name: "description", label: "Description", placeholder: "Goals and competencies weights must add up to 100%.", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default AppraisalTemplateForm;
