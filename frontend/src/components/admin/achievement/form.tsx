"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { AchievementModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveAchievement from "@/services/admin/achievement/save";
import getAchievement from "@/services/admin/achievement/get";
import getAllEmployee from "@/services/admin/employee/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { achievementCategoryOptions } from "@/constants/performance";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: AchievementModel = { category: "Milestone" };

function AchievementForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<AchievementModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["achievement", id],
    queryFn: () => getAchievement(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [empParam, setEmpParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: employees, isLoading: isEmpLoading } = useQuery({
    queryKey: ["employees", empParam],
    queryFn: () => getAllEmployee(empParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveAchievement(fd);
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
      setFormData({ ...record, achievementDate: (record.achievementDate || "").slice(0, 10) });
    } else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["achievements"] });
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
              name: "employeeId", label: "Employee", placeholder: "Select employee", required: true, type: "dropDown",
              value: formData.employeeId, displayValue: formData.employeeName,
              error: formState?.zodErrors?.employeeId,
              param: empParam, setParam: setEmpParam as any, isLoading: isEmpLoading,
              onSelect: selectHandler,
              data: employees?.data?.map((e) => ({ id: e.id, name: `${e.employeeNumber} — ${e.fullName ?? ""}` })) as never,
            },
            {
              name: "category", label: "Category", type: "dropDown", onSelect: selectHandler,
              value: formData.category, displayValue: formData.category,
              data: achievementCategoryOptions as never,
            },
            { name: "title", label: "Title", placeholder: "e.g. Led migration project", required: true, value: formData.title, onChange: changeHandler, error: formState?.zodErrors?.title, type: "text" },
            { name: "achievementDate", label: "Date", required: true, type: "date", value: formData.achievementDate, onChange: changeHandler, error: formState?.zodErrors?.achievementDate },
            { name: "description", label: "Description", placeholder: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default AchievementForm;
