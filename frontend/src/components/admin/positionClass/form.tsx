"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { PositionClassModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import savePositionClass from "@/services/admin/positionClass/save";
import getPositionClass from "@/services/admin/positionClass/get";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import getAllSalaryScale from "@/services/admin/salaryScale/getAll";
import getAllJobCategory from "@/services/admin/jobCategory/getAll";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

const formatMoney = (v: unknown) =>
  v == null || v === "" ? "" : Number(v).toLocaleString(undefined, { minimumFractionDigits: 2 });

function PositionClassForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as PositionClassModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["positionClass", id],
    queryFn: () => getPositionClass(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [gradeParam, setGradeParam] = useState({ ...lookupParam });
  const { data: grades, isLoading: gradesLoading } = useQuery({
    queryKey: ["jobGrades", gradeParam],
    queryFn: () => getAllJobGrade(gradeParam),
  });

  // Salary scales are scoped to the selected job grade (the grade acts as a filter).
  const [scaleParam, setScaleParam] = useState({ ...lookupParam });
  const { data: scales, isLoading: scalesLoading } = useQuery({
    queryKey: ["salaryScales", "byGrade", formData.jobGradeId, scaleParam],
    queryFn: () => getAllSalaryScale({ ...scaleParam, jobGradeId: formData.jobGradeId }),
    enabled: !!formData.jobGradeId,
  });

  const [categoryParam, setCategoryParam] = useState({ ...lookupParam });
  const { data: categories, isLoading: categoriesLoading } = useQuery({
    queryKey: ["jobCategories", categoryParam],
    queryFn: () => getAllJobCategory(categoryParam),
  });
  const [locationParam, setLocationParam] = useState({ ...lookupParam });
  const { data: locations, isLoading: locationsLoading } = useQuery({
    queryKey: ["workLocations", locationParam],
    queryFn: () => getAllWorkLocation(locationParam),
  });
  const [reportsParam, setReportsParam] = useState({ ...lookupParam });
  const { data: classes, isLoading: classesLoading } = useQuery({
    queryKey: ["positionClasses", reportsParam],
    queryFn: () => getAllPositionClass(reportsParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await savePositionClass(fd);
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

  // Choosing a grade filters salary scales and clears any previously-picked scale.
  const gradeSelectHandler = useCallback((_name: string, r: any) => {
    setFormData((p) => ({
      ...p,
      jobGradeId: r.id,
      jobGradeName: r.name,
      salaryScaleId: undefined,
      salaryStep: undefined,
      salary: undefined,
    }));
  }, []);

  // Choosing a scale records its id and surfaces the exact salary.
  const scaleSelectHandler = useCallback(
    (_name: string, r: any) => {
      const scale = (scales?.data ?? []).find((s) => s.id === r.id);
      setFormData((p) => ({
        ...p,
        salaryScaleId: r.id,
        salaryStep: scale?.step,
        salary: scale?.salary,
      }));
    },
    [scales],
  );

  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as PositionClassModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["positionClasses"] });
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
            { name: "code", label: "Code", placeholder: "Code", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
            { name: "title", label: "Title", placeholder: "Title", required: true, value: formData.title, onChange: changeHandler, error: formState?.zodErrors?.title, type: "text" },
            {
              name: "jobGradeId", label: "Job Grade", required: true, type: "dropDown", onSelect: gradeSelectHandler,
              value: formData.jobGradeId, displayValue: formData.jobGradeName,
              param: gradeParam, setParam: setGradeParam as any, isLoading: gradesLoading,
              data: (grades?.data ?? []).map((g) => ({ id: g.id, name: g.name })) as never,
            },
            {
              name: "salaryScaleId", label: "Salary Scale (Step)", required: true, type: "dropDown", onSelect: scaleSelectHandler,
              value: formData.salaryScaleId, displayValue: formData.salaryStep,
              error: formState?.zodErrors?.salaryScaleId,
              disabled: !formData.jobGradeId,
              placeholder: formData.jobGradeId ? "Select a step" : "Select a job grade first",
              param: scaleParam, setParam: setScaleParam as any, isLoading: scalesLoading,
              data: (scales?.data ?? []).map((s) => ({
                id: s.id,
                name: `${s.step ?? "Step"} — ${formatMoney(s.salary)}`,
              })) as never,
            },
            { name: "salaryDisplay", label: "Salary", value: formatMoney(formData.salary), type: "text", disabled: true },
            {
              name: "jobCategoryId", label: "Job Category", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.jobCategoryId, displayValue: formData.jobCategoryName,
              error: formState?.zodErrors?.jobCategoryId,
              param: categoryParam, setParam: setCategoryParam as any, isLoading: categoriesLoading,
              data: (categories?.data ?? []).map((c) => ({ id: c.id, name: c.name })) as never,
            },
            {
              name: "reportsToPositionClassId", label: "Reports To", type: "dropDown", onSelect: selectHandler,
              value: formData.reportsToPositionClassId, displayValue: formData.reportsToPositionClassTitle,
              param: reportsParam, setParam: setReportsParam as any, isLoading: classesLoading,
              data: (classes?.data ?? []).filter((c) => c.id !== formData.id).map((c) => ({ id: c.id, name: c.title })) as never,
            },
            {
              name: "workLocationId", label: "Work Location", type: "dropDown", onSelect: selectHandler,
              value: formData.workLocationId, displayValue: formData.workLocationName,
              param: locationParam, setParam: setLocationParam as any, isLoading: locationsLoading,
              data: (locations?.data ?? []).map((l) => ({ id: l.id, name: l.name })) as never,
            },
            { name: "allocatedHeadcount", label: "Allocated Headcount", value: formData.allocatedHeadcount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "minExperienceYears", label: "Min Experience (yrs)", value: formData.minExperienceYears, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "minimumAge", label: "Minimum Age", value: formData.minimumAge, onChange: changeHandler, error: formState?.zodErrors?.minimumAge, inputType: "number", type: "text" },
            { name: "maximumAge", label: "Maximum Age", value: formData.maximumAge, onChange: changeHandler, error: formState?.zodErrors?.maximumAge, inputType: "number", type: "text" },
            { name: "weeklyWorkingHours", label: "Weekly Working Hours", value: formData.weeklyWorkingHours, onChange: changeHandler, error: formState?.zodErrors?.weeklyWorkingHours, inputType: "number", type: "text" },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "minQualifications", label: "Min Qualifications", value: formData.minQualifications, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "skills", label: "Skills", value: formData.skills, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default PositionClassForm;
