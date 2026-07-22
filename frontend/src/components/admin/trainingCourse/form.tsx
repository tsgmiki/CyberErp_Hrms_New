"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { TrainingCourseModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveTrainingCourse from "@/services/admin/trainingCourse/save";
import getTrainingCourse from "@/services/admin/trainingCourse/get";
import getAllTrainingCategory from "@/services/admin/trainingCategory/getAll";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel, trainingDeliveryModeOptions } from "@/constants/orgStructure";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: TrainingCourseModel = { isActive: true, deliveryMode: "InPerson", isExternal: false };

function TrainingCourseForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<TrainingCourseModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["trainingCourse", id],
    queryFn: () => getTrainingCourse(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [catParam, setCatParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: categories, isLoading: isCatLoading } = useQuery({
    queryKey: ["trainingCategories", catParam],
    queryFn: () => getAllTrainingCategory(catParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveTrainingCourse(fd);
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
      queryClient.invalidateQueries({ queryKey: ["trainingCourses"] });
      setId("");
    }
  }, [formState]);

  const isExternal = formData.isExternal === true || String(formData.isExternal) === "true";

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
            { name: "name", label: "Name", placeholder: "e.g. Advanced SQL", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "code", label: "Code", placeholder: "Optional course code", value: formData.code, onChange: changeHandler, type: "text" },
            {
              name: "trainingCategoryId", label: "Category", placeholder: "Select category", type: "dropDown",
              value: formData.trainingCategoryId, displayValue: formData.categoryName,
              param: catParam, setParam: setCatParam as any, isLoading: isCatLoading,
              onSelect: selectHandler,
              data: categories?.data?.map((c) => ({ id: c.id, name: c.name })) as never,
            },
            {
              name: "deliveryMode", label: "Delivery Mode", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.deliveryMode,
              displayValue: trainingDeliveryModeOptions.find((o) => o.id === formData.deliveryMode)?.name,
              error: formState?.zodErrors?.deliveryMode,
              data: trainingDeliveryModeOptions as never,
            },
            { name: "durationHours", label: "Duration (hours)", placeholder: "e.g. 24", value: formData.durationHours, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "cpdHours", label: "CPD Hours", placeholder: "Credited on completion", value: formData.cpdHours, onChange: changeHandler, inputType: "number", type: "text" },
            {
              name: "isExternal", label: "External Course", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.isExternal), displayValue: yesNoLabel(formData.isExternal),
              data: yesNoOptions as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            ...(isExternal
              ? [
                  { name: "providerName", label: "Provider", placeholder: "e.g. Coursera", required: true, value: formData.providerName, onChange: changeHandler, type: "text" as const },
                  { name: "externalUrl", label: "Course URL", placeholder: "https://…", value: formData.externalUrl, onChange: changeHandler, type: "text" as const },
                ]
              : []),
            { name: "targetAudience", label: "Target Audience", placeholder: "Who should attend", value: formData.targetAudience, onChange: changeHandler, type: "text" },
            { name: "prerequisites", label: "Prerequisites", placeholder: "Required background", value: formData.prerequisites, onChange: changeHandler, type: "text" },
            { name: "objectives", label: "Objectives", placeholder: "What participants will learn", value: formData.objectives, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "description", label: "Description", placeholder: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default TrainingCourseForm;
