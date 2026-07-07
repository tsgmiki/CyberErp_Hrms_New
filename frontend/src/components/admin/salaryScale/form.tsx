"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { SalaryScaleModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveSalaryScale from "@/services/admin/salaryScale/save";
import getSalaryScale from "@/services/admin/salaryScale/get";
import getAllStep from "@/services/admin/step/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

interface Props {
  id: string;
  setId: (id: string) => void;
  jobGradeId: string;
  gradeLabel?: string;
}

function SalaryScaleForm({ id, setId, jobGradeId, gradeLabel }: Props) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as SalaryScaleModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["salaryScale", id],
    queryFn: () => getSalaryScale(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [stepParam, setStepParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: steps, isLoading: isStepsLoading } = useQuery({
    queryKey: ["steps", stepParam],
    queryFn: () => getAllStep(stepParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveSalaryScale(fd);
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

  // Editing an existing row loads its values; adding a new row seeds the selected grade.
  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
    else setFormData({ jobGradeId } as SalaryScaleModel);
  }, [record, jobGradeId]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ jobGradeId } as SalaryScaleModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["salaryScales"] });
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
            {
              name: "jobGradeDisplay",
              label: "Job Grade",
              value: formData.jobGrade || gradeLabel || "",
              type: "text",
              disabled: true,
            },
            {
              name: "stepId",
              label: "Step",
              placeholder: "Step",
              required: true,
              value: formData.stepId,
              displayValue: formData.step,
              error: formState?.zodErrors?.stepId,
              type: "dropDown",
              param: stepParam,
              setParam: setStepParam as any,
              isLoading: isStepsLoading,
              onSelect: selectHandler,
              data: steps?.data?.map((item: any) => ({ id: item.id, name: `${item.code} — ${item.name}` })) as never,
            },
            {
              name: "salary",
              label: "Salary",
              placeholder: "Salary",
              required: true,
              value: formData.salary,
              onChange: changeHandler,
              error: formState?.zodErrors?.salary,
              inputType: "number",
              type: "text",
            },
            { name: "jobGradeId", value: formData.jobGradeId || jobGradeId, type: "hidden" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default SalaryScaleForm;
