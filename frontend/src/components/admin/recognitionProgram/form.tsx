"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { RecognitionProgramModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveRecognitionProgram from "@/services/admin/recognitionProgram/save";
import getRecognitionProgram from "@/services/admin/recognitionProgram/get";
import getAllRecognitionBadge from "@/services/admin/recognitionBadge/getAll";
import Loading from "../../common/loader/loader";
import { activeStatusOptions, activeId, activeLabel, programPeriodOptions } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: RecognitionProgramModel = { isActive: true, period: "Monthly" };

function RecognitionProgramForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<RecognitionProgramModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["recognitionProgram", id],
    queryFn: () => getRecognitionProgram(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [badgeParam, setBadgeParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: badges, isLoading: isBadgeLoading } = useQuery({
    queryKey: ["recognitionBadges", badgeParam],
    queryFn: () => getAllRecognitionBadge(badgeParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveRecognitionProgram(fd);
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
      queryClient.invalidateQueries({ queryKey: ["recognitionPrograms"] });
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
              name: "period", label: "Period", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.period,
              displayValue: programPeriodOptions.find((o) => o.id === formData.period)?.name,
              error: formState?.zodErrors?.period,
              data: programPeriodOptions as never,
            },
            {
              name: "recognitionBadgeId", label: "Award", placeholder: "Optional fixed award", type: "dropDown",
              value: formData.recognitionBadgeId, displayValue: formData.badgeName,
              param: badgeParam, setParam: setBadgeParam as any, isLoading: isBadgeLoading,
              onSelect: selectHandler,
              data: badges?.data?.map((b) => ({ id: b.id, name: b.name })) as never,
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
export default RecognitionProgramForm;
