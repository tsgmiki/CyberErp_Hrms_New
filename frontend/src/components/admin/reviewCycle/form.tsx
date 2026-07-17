"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { ReviewCycleModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveReviewCycle from "@/services/admin/reviewCycle/save";
import getReviewCycle from "@/services/admin/reviewCycle/get";
import { getAllRatingScales } from "@/services/admin/ratingScale";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { reviewPeriodTypeOptions, reviewPeriodTypeLabel } from "@/constants/performance";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: ReviewCycleModel = {
  periodType: "Annual",
  enableSelfAssessment: true,
  enablePeerAssessment: false,
  enableCalibration: false,
};

function ReviewCycleForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<ReviewCycleModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["reviewCycle", id],
    queryFn: () => getReviewCycle(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [scaleParam, setScaleParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: scales, isLoading: isScalesLoading } = useQuery({
    queryKey: ["ratingScales", scaleParam],
    queryFn: () => getAllRatingScales(scaleParam),
  });

  const [fyParam, setFyParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: fiscalYears, isLoading: isFyLoading } = useQuery({
    queryKey: ["fiscalYears", fyParam],
    queryFn: () => getAllFiscalYear(fyParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveReviewCycle(fd);
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
      setFormData({
        ...record,
        startDate: (record.startDate || "").slice(0, 10),
        endDate: (record.endDate || "").slice(0, 10),
        selfReviewDue: (record.selfReviewDue || "").slice(0, 10),
        managerReviewDue: (record.managerReviewDue || "").slice(0, 10),
      });
    } else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["reviewCycles"] });
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
            { name: "name", label: "Name", placeholder: "e.g. FY26 Annual Review", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "periodType", label: "Period Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.periodType, displayValue: reviewPeriodTypeLabel(formData.periodType),
              error: formState?.zodErrors?.periodType, data: reviewPeriodTypeOptions as never,
            },
            {
              name: "ratingScaleId", label: "Rating Scale", placeholder: "Select rating scale", required: true, type: "dropDown",
              value: formData.ratingScaleId, displayValue: formData.ratingScaleName,
              error: formState?.zodErrors?.ratingScaleId,
              param: scaleParam, setParam: setScaleParam as any, isLoading: isScalesLoading,
              onSelect: selectHandler,
              data: scales?.data?.map((s) => ({ id: s.id, name: s.name })) as never,
            },
            {
              name: "fiscalYearId", label: "Fiscal Year", placeholder: "Optional", type: "dropDown",
              value: formData.fiscalYearId, displayValue: formData.fiscalYearName,
              param: fyParam, setParam: setFyParam as any, isLoading: isFyLoading,
              onSelect: selectHandler,
              data: fiscalYears?.data?.map((f) => ({ id: f.id, name: f.name })) as never,
            },
            { name: "startDate", label: "Start Date", required: true, type: "date", value: formData.startDate, onChange: changeHandler, error: formState?.zodErrors?.startDate },
            { name: "endDate", label: "End Date", required: true, type: "date", value: formData.endDate, onChange: changeHandler, error: formState?.zodErrors?.endDate },
            { name: "selfReviewDue", label: "Self-Review Due", type: "date", value: formData.selfReviewDue, onChange: changeHandler },
            { name: "managerReviewDue", label: "Manager-Review Due", type: "date", value: formData.managerReviewDue, onChange: changeHandler },
            {
              name: "enableSelfAssessment", label: "Enable Self-Assessment", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.enableSelfAssessment), displayValue: yesNoLabel(formData.enableSelfAssessment),
              data: yesNoOptions as never,
            },
            {
              name: "enablePeerAssessment", label: "Enable Peer Assessment", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.enablePeerAssessment), displayValue: yesNoLabel(formData.enablePeerAssessment),
              data: yesNoOptions as never,
            },
            {
              name: "enableCalibration", label: "Enable Calibration", type: "dropDown", onSelect: selectHandler,
              value: boolId(formData.enableCalibration), displayValue: yesNoLabel(formData.enableCalibration),
              data: yesNoOptions as never,
            },
            // Probation cycles: the period end is computed as each employee's hire date + this many months.
            ...(formData.periodType === "Probation"
              ? [{
                  name: "probationDurationMonths", label: "Probation Duration (Months)", placeholder: "e.g. 3",
                  value: formData.probationDurationMonths, onChange: changeHandler,
                  error: formState?.zodErrors?.probationDurationMonths, inputType: "number", type: "text" as const,
                }]
              : []),
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default ReviewCycleForm;
