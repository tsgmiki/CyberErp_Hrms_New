"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { lazy, memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { Route, Milestone, ListOrdered } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { CareerPathModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import saveCareerPath from "@/services/admin/careerPath/save";
import getCareerPath from "@/services/admin/careerPath/get";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const CareerPathSteps = memo(lazy(() => import("./steps")));
const CareerPathLadder = memo(lazy(() => import("./visualize")));

function CareerPathForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as CareerPathModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["careerPath", id],
    queryFn: () => getCareerPath(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    setFormState(await saveCareerPath(fd));
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => setFormData((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setFormData((p) => ({ ...p, [name]: r.id })), []);

  useEffect(() => { if (record) setFormData(record); }, [record]);
  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["careerPaths"] });
      if (formState.id && !id) setId(formState.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <div className="space-y-4">
      {pending && <Loading />}
      <EntityFormTabs
        hasId={!!id}
        disabledHint="Save the career path to add its steps."
        tabs={[
          {
            key: "path",
            label: "Career Path",
            Icon: Route,
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
                    formId: "careerPathForm",
                    components: [
                      { name: "name", label: "Career Path Name", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
                      { name: "code", label: "Code", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
                      { name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
                        value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive), data: activeStatusOptions as never },
                      { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
                      { name: "id", value: formData.id, type: "hidden" },
                    ],
                  }}
                />
                <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
              </div>
            ),
          },
          { key: "steps", label: "Steps", Icon: ListOrdered, needsId: true, content: id ? <CareerPathSteps pathId={id} /> : null },
          { key: "ladder", label: "Path Ladder", Icon: Milestone, needsId: true, content: id ? <CareerPathLadder pathId={id} /> : null },
        ]}
      />
    </div>
  );
}

export default memo(CareerPathForm);
