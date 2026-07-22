"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { CompetencyModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveCompetency from "@/services/admin/competency/save";
import getCompetency from "@/services/admin/competency/get";
import getAllCompetencyCategory from "@/services/admin/competencyCategory/getAll";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

function CompetencyForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as CompetencyModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["competency", id],
    queryFn: () => getCompetency(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [categoryParam, setCategoryParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: categories, isLoading: isCategoriesLoading } = useQuery({
    queryKey: ["competencyCategories", categoryParam],
    queryFn: () => getAllCompetencyCategory(categoryParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveCompetency(fd);
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
      setFormData({} as CompetencyModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["competencies"] });
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
            { name: "name", label: "Name", placeholder: "e.g. Delegation", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "competencyCategoryId",
              label: "Category",
              placeholder: "Select category",
              required: true,
              type: "dropDown",
              value: formData.competencyCategoryId,
              displayValue: formData.competencyCategoryName,
              error: formState?.zodErrors?.competencyCategoryId,
              param: categoryParam,
              setParam: setCategoryParam as any,
              isLoading: isCategoriesLoading,
              onSelect: selectHandler,
              data: categories?.data?.map((c) => ({ id: c.id, name: c.name })) as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "description", label: "Description", placeholder: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default CompetencyForm;
