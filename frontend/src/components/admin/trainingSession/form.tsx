"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { TrainingSessionModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { saveTrainingSession, getTrainingSession, createTrainingSessionSeries } from "@/services/admin/trainingSession";
import getAllTrainingCourse from "@/services/admin/trainingCourse/getAll";
import Loading from "../../common/loader/loader";
import { trainerTypeOptions, sessionRecurrenceOptions } from "@/constants/orgStructure";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: TrainingSessionModel = { trainerType: "Internal" };

function TrainingSessionForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<TrainingSessionModel>({ ...NEW_DEFAULTS });
  const [recurrence, setRecurrence] = useState("");
  const [occurrences, setOccurrences] = useState(4);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["trainingSession", id],
    queryFn: () => getTrainingSession(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [courseParam, setCourseParam] = useState({ ...parameterInitialData, take: 200, status: "true" });
  const { data: courses, isLoading: isCourseLoading } = useQuery({
    queryKey: ["trainingCourses", courseParam],
    queryFn: () => getAllTrainingCourse(courseParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    // A repeat choice on a NEW session materializes the whole series instead (HC197).
    if (!id && recurrence) {
      const obj = Object.fromEntries(fd) as Record<string, string>;
      const num = (v?: string) => (v != null && v !== "" && Number.isFinite(Number(v)) ? Number(v) : undefined);
      const result = await createTrainingSessionSeries({
        trainingCourseId: obj.trainingCourseId,
        startDate: obj.startDate,
        endDate: obj.endDate,
        venue: obj.venue || undefined,
        trainerType: obj.trainerType,
        trainerName: obj.trainerName || undefined,
        providerName: obj.providerName || undefined,
        meetingUrl: obj.meetingUrl || undefined,
        maxParticipants: num(obj.maxParticipants),
        trainerCost: num(obj.trainerCost),
        materialsCost: num(obj.materialsCost),
        venueCost: num(obj.venueCost),
        notes: obj.notes || undefined,
        recurrence,
        occurrences,
      });
      setFormState({ ...result, zodErrors: {} });
    } else {
      const result = await saveTrainingSession(fd);
      setFormState(result);
    }
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    if (name === "recurrence") {
      setRecurrence(r.id);
      return;
    }
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData({ ...record, startDate: record.startDate?.slice(0, 10), endDate: record.endDate?.slice(0, 10) });
    } else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      setRecurrence("");
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["trainingSessions"] });
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
              name: "trainingCourseId", label: "Course", placeholder: "Select course", required: true, type: "dropDown",
              value: formData.trainingCourseId, displayValue: formData.courseName,
              error: formState?.zodErrors?.trainingCourseId,
              param: courseParam, setParam: setCourseParam as any, isLoading: isCourseLoading,
              onSelect: selectHandler,
              data: courses?.data?.map((c) => ({ id: c.id, name: c.name })) as never,
            },
            { name: "venue", label: "Venue", placeholder: "Room / location", value: formData.venue, onChange: changeHandler, type: "text" },
            { name: "startDate", label: "Start Date", required: true, type: "date", value: formData.startDate, onChange: changeHandler, error: formState?.zodErrors?.startDate },
            { name: "endDate", label: "End Date", required: true, type: "date", value: formData.endDate, onChange: changeHandler, error: formState?.zodErrors?.endDate },
            {
              name: "trainerType", label: "Trainer Type", type: "dropDown", onSelect: selectHandler,
              value: formData.trainerType,
              displayValue: trainerTypeOptions.find((o) => o.id === formData.trainerType)?.name,
              data: trainerTypeOptions as never,
            },
            { name: "trainerName", label: "Trainer", placeholder: "Facilitator name", value: formData.trainerName, onChange: changeHandler, type: "text" },
            { name: "providerName", label: "Provider", placeholder: "External delivery organisation", value: formData.providerName, onChange: changeHandler, type: "text" },
            { name: "meetingUrl", label: "Meeting URL", placeholder: "Online session link", value: formData.meetingUrl, onChange: changeHandler, type: "text" },
            { name: "maxParticipants", label: "Capacity", placeholder: "Blank = unlimited", value: formData.maxParticipants, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "trainerCost", label: "Trainer Cost", placeholder: "0", value: formData.trainerCost, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "materialsCost", label: "Materials Cost", placeholder: "0", value: formData.materialsCost, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "venueCost", label: "Venue Cost", placeholder: "0", value: formData.venueCost, onChange: changeHandler, inputType: "number", type: "text" },
            ...(!id
              ? [
                  {
                    name: "recurrence", label: "Repeat", type: "dropDown" as const, onSelect: selectHandler,
                    value: recurrence,
                    displayValue: sessionRecurrenceOptions.find((o) => o.id === recurrence)?.name,
                    data: sessionRecurrenceOptions as never,
                  },
                  ...(recurrence
                    ? [{
                        name: "occurrences", label: "Occurrences", placeholder: "2–26", value: occurrences,
                        onChange: (e: any) => setOccurrences(Number(e.target.value) || 2), inputType: "number", type: "text" as const,
                      }]
                    : []),
                ]
              : []),
            { name: "notes", label: "Notes", placeholder: "Notes", value: formData.notes, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default TrainingSessionForm;
