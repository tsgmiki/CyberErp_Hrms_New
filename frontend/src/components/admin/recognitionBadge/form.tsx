"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { RecognitionBadgeModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveRecognitionBadge from "@/services/admin/recognitionBadge/save";
import getRecognitionBadge from "@/services/admin/recognitionBadge/get";
import getAllAwardCategory from "@/services/admin/awardCategory/getAll";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel, rewardKindOptions } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: RecognitionBadgeModel = { isActive: true, color: "#F59E0B", rewardKind: "Badge" };

function RecognitionBadgeForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<RecognitionBadgeModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["recognitionBadge", id],
    queryFn: () => getRecognitionBadge(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [catParam, setCatParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: categories, isLoading: isCatLoading } = useQuery({
    queryKey: ["awardCategories", catParam],
    queryFn: () => getAllAwardCategory(catParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveRecognitionBadge(fd);
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
      queryClient.invalidateQueries({ queryKey: ["recognitionBadges"] });
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
            { name: "name", label: "Name", placeholder: "e.g. Employee of the Month", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "color", label: "Color", placeholder: "#F59E0B", value: formData.color, onChange: changeHandler, type: "text" },
            { name: "icon", label: "Icon", placeholder: "Trophy", value: formData.icon, onChange: changeHandler, type: "text" },
            {
              name: "rewardKind", label: "Reward Kind", type: "dropDown", onSelect: selectHandler,
              value: formData.rewardKind,
              displayValue: rewardKindOptions.find((o) => o.id === formData.rewardKind)?.name,
              data: rewardKindOptions as never,
            },
            { name: "monetaryValue", label: "Monetary Value", placeholder: "Gift card / bonus amount", value: formData.monetaryValue, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "pointsValue", label: "Reward Points", placeholder: "Points credited on grant", value: formData.pointsValue, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "autoGrantMinScore", label: "Auto-Grant ≥ Score %", placeholder: "e.g. 85 — blank = manual only", value: formData.autoGrantMinScore, onChange: changeHandler, inputType: "number", type: "text" },
            {
              name: "awardCategoryId", label: "Award Category", placeholder: "Select category", type: "dropDown",
              value: formData.awardCategoryId,
              displayValue: categories?.data?.find((c) => c.id === formData.awardCategoryId)?.name,
              param: catParam, setParam: setCatParam as any, isLoading: isCatLoading,
              onSelect: selectHandler,
              data: categories?.data?.map((c) => ({ id: c.id, name: c.name })) as never,
            },
            { name: "sortOrder", label: "Sort Order", placeholder: "0", value: formData.sortOrder, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "criteria", label: "Criteria", placeholder: "Eligibility criteria shown to nominators", value: formData.criteria, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "description", label: "Description", placeholder: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default RecognitionBadgeForm;
