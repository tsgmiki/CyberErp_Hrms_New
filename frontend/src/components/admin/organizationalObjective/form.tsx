"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { OrganizationalObjectiveModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveOrganizationalObjective from "@/services/admin/organizationalObjective/save";
import getOrganizationalObjective from "@/services/admin/organizationalObjective/get";
import getAllOrganizationalObjective from "@/services/admin/organizationalObjective/getAll";
import getAllReviewCycle from "@/services/admin/reviewCycle/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { objectiveStatusOptions } from "@/constants/performance";

const FormProvider = memo(FormProviders);
const NEW_DEFAULTS: OrganizationalObjectiveModel = { status: "Active", weight: 0 };

function OrganizationalObjectiveForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<OrganizationalObjectiveModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["organizationalObjective", id],
    queryFn: () => getOrganizationalObjective(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [cycleParam, setCycleParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: cycles, isLoading: isCyclesLoading } = useQuery({
    queryKey: ["reviewCycles", cycleParam],
    queryFn: () => getAllReviewCycle(cycleParam),
  });

  const [unitParam, setUnitParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: units, isLoading: isUnitsLoading } = useQuery({
    queryKey: ["organizationUnits", unitParam],
    queryFn: () => getAllOrganizationUnit(unitParam),
  });

  const [parentParam, setParentParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: parents, isLoading: isParentsLoading } = useQuery({
    queryKey: ["organizationalObjectives", parentParam],
    queryFn: () => getAllOrganizationalObjective(parentParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveOrganizationalObjective(fd);
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
      queryClient.invalidateQueries({ queryKey: ["organizationalObjectives"] });
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
            { name: "title", label: "Title", placeholder: "e.g. Grow Revenue", required: true, value: formData.title, onChange: changeHandler, error: formState?.zodErrors?.title, type: "text" },
            {
              name: "reviewCycleId", label: "Review Cycle", placeholder: "Select cycle", required: true, type: "dropDown",
              value: formData.reviewCycleId, displayValue: formData.reviewCycleName,
              error: formState?.zodErrors?.reviewCycleId,
              param: cycleParam, setParam: setCycleParam as any, isLoading: isCyclesLoading,
              onSelect: selectHandler,
              data: cycles?.data?.map((c) => ({ id: c.id, name: c.name })) as never,
            },
            {
              name: "organizationUnitId", label: "Organization Unit", placeholder: "Optional (directorate/team)", type: "dropDown",
              value: formData.organizationUnitId, displayValue: formData.organizationUnitName,
              param: unitParam, setParam: setUnitParam as any, isLoading: isUnitsLoading,
              onSelect: selectHandler,
              data: units?.data?.map((u) => ({ id: u.id, name: u.name })) as never,
            },
            {
              name: "parentObjectiveId", label: "Parent Objective", placeholder: "Optional (cascade)", type: "dropDown",
              value: formData.parentObjectiveId, displayValue: formData.parentObjectiveTitle,
              param: parentParam, setParam: setParentParam as any, isLoading: isParentsLoading,
              onSelect: selectHandler,
              data: parents?.data?.filter((o) => o.id !== formData.id).map((o) => ({ id: o.id, name: o.title })) as never,
            },
            { name: "weight", label: "Weight (%)", placeholder: "0", value: formData.weight, onChange: changeHandler, error: formState?.zodErrors?.weight, inputType: "number", type: "text" },
            {
              name: "status", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: formData.status, displayValue: formData.status,
              data: objectiveStatusOptions as never,
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
export default OrganizationalObjectiveForm;
