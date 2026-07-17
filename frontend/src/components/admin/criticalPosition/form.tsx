"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { CriticalPositionModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveCriticalPosition from "@/services/admin/criticalPosition/save";
import getCriticalPosition from "@/services/admin/criticalPosition/get";
import getAllPosition from "@/services/admin/position/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { riskLevelOptions } from "@/constants/careerDevelopment";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);

function CriticalPositionForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as CriticalPositionModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["criticalPosition", id],
    queryFn: () => getCriticalPosition(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: positions } = useQuery({
    queryKey: ["positions", "criticalPositionLookup"],
    queryFn: () => getAllPosition({ ...parameterInitialData, take: 200 }),
    staleTime: 60_000,
  });
  const positionOptions = useMemo(
    () => (positions?.data ?? []).map((p) => ({
      id: p.id,
      name: `${p.code} — ${p.positionClassTitle ?? ""}${p.organizationUnitName ? ` · ${p.organizationUnitName}` : ""}`,
    })),
    [positions],
  );

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    setFormState(await saveCriticalPosition(fd));
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
    if (formState.status == "success") {
      setFormData({} as CriticalPositionModel);
      formRef.current?.reset();
      queryClient.invalidateQueries({ queryKey: ["criticalPositions"] });
      setId("");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <div>
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
            { name: "positionId", label: "Role / Position", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.positionId, displayValue: formData.positionTitle ?? formData.positionCode,
              error: formState?.zodErrors?.positionId, data: positionOptions as never },
            { name: "riskLevel", label: "Risk Level", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.riskLevel ?? "Medium", displayValue: formData.riskLevel ?? "Medium",
              error: formState?.zodErrors?.riskLevel, data: riskLevelOptions as never },
            { name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive), data: activeStatusOptions as never },
            { name: "reason", label: "Reason", placeholder: "Why is this role critical?", value: formData.reason, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "criteria", label: "Criteria", placeholder: "Criteria used to flag it", value: formData.criteria, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(CriticalPositionForm);
