"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { lazy, memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { ClipboardList, ClipboardCheck, Grid3x3 } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { TalentReviewModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import saveTalentReview from "@/services/admin/talentReview/save";
import getTalentReview from "@/services/admin/talentReview/get";
import { talentReviewStatusOptions } from "@/constants/careerDevelopment";

const FormProvider = memo(FormProviders);
const NineBoxGrid = memo(lazy(() => import("./nineBoxGrid")));
const Assessments = memo(lazy(() => import("./assessments")));

function TalentReviewForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as TalentReviewModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["talentReview", id],
    queryFn: () => getTalentReview(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveTalentReview(fd);
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

  useEffect(() => { if (record) setFormData(record); }, [record]);

  useEffect(() => {
    // On save, keep the record open (so its 9-box / assessments become editable) — refresh the id.
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["talentReviews"] });
      if (formState.id && !id) setId(formState.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <div className="space-y-4">
      {pending && <Loading />}
      <EntityFormTabs
        hasId={!!id}
        disabledHint="Save the review to place employees on the 9-box grid."
        tabs={[
          {
            key: "review",
            label: "Talent Review",
            Icon: ClipboardList,
            keepMounted: true,
            content: (
              <div className="space-y-4">
                <FormProvider
                  ref={formRef}
                  form={{
                    columnsNo: 2,
                    submitHandler,
                    labelWidth: "w-[30%]",
                    isPending: isLoading,
                    SubmitButton: "top",
                    formId: "talentReviewForm",
                    components: [
                      { name: "name", label: "Name", required: true, placeholder: "e.g. 2026 Talent Review", value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
                      { name: "cycle", label: "Cycle", placeholder: "e.g. 2026", value: formData.cycle, onChange: changeHandler, type: "text" },
                      { name: "status", label: "Status", required: true, type: "dropDown", onSelect: selectHandler,
                        value: formData.status ?? "Draft", displayValue: talentReviewStatusOptions.find((o) => o.id === (formData.status ?? "Draft"))?.name, data: talentReviewStatusOptions as never },
                      { name: "notes", label: "Notes", value: formData.notes, onChange: changeHandler, type: "textarea", colSpan: "full" },
                      { name: "id", value: formData.id, type: "hidden" },
                    ],
                  }}
                />
                <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
              </div>
            ),
          },
          { key: "assessments", label: "Assessments", Icon: ClipboardCheck, needsId: true, content: id ? <Assessments reviewId={id} /> : null },
          { key: "nineBox", label: "9-Box Grid", Icon: Grid3x3, needsId: true, content: id ? <NineBoxGrid reviewId={id} /> : null },
        ]}
      />
    </div>
  );
}

export default memo(TalentReviewForm);
