"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { PositionModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import savePosition from "@/services/admin/position/save";
import getPosition from "@/services/admin/position/get";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

interface Props {
  id: string;
  presetOrganizationUnitId?: string;
  presetOrganizationUnitName?: string;
  onClose: () => void;
  onSaved: () => void;
}

function PositionForm({
  id,
  presetOrganizationUnitId,
  presetOrganizationUnitName,
  onClose,
  onSaved,
}: Props) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<PositionModel>(() =>
    presetOrganizationUnitId
      ? { organizationUnitId: presetOrganizationUnitId, organizationUnitName: presetOrganizationUnitName }
      : {},
  );
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record } = useQuery({
    queryKey: ["position", id],
    queryFn: () => getPosition(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [classParam, setClassParam] = useState({ ...lookupParam });
  const { data: classes, isLoading: classesLoading } = useQuery({
    queryKey: ["positionClasses", classParam],
    queryFn: () => getAllPositionClass(classParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await savePosition(fd);
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
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["positions"] });
      onSaved();
      onClose();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  // Organization unit is captured behind the scenes — shown in the modal title, not as a field.
  const orgName = formData.organizationUnitName ?? presetOrganizationUnitName;
  const baseTitle = id ? "Edit Position" : "Add Position";
  const modalTitle = orgName ? `${baseTitle} · ${orgName}` : baseTitle;

  return (
    <FormProvider
      ref={formRef}
      form={{
        columnsNo: 1,
        submitHandler,
        labelWidth: "w-[35%]",
        isPending: isLoading,
        SubmitButton: "top",
        showModal: true,
        modalVisible: true,
        modalTitle,
        modalSize: "lg",
        onModalClose: onClose,
        submitBtnTitle: "Save",
        components: [
          {
            name: "positionClassId", label: "Position Class", required: true, type: "dropDown", onSelect: selectHandler,
            value: formData.positionClassId, displayValue: formData.positionClassTitle,
            error: formState?.zodErrors?.positionClassId,
            param: classParam, setParam: setClassParam as any, isLoading: classesLoading,
            data: (classes?.data ?? []).map((c) => ({ id: c.id, name: `${c.title} (${c.code})` })) as never,
          },
          { name: "code", label: "Code", placeholder: "Position code", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
          // Organization unit — hidden, captured from the selected tree node (or the record on edit).
          { name: "organizationUnitId", value: formData.organizationUnitId, type: "hidden" },
          { name: "id", value: formData.id, type: "hidden" },
        ],
      }}
    >
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </FormProvider>
  );
}
export default memo(PositionForm);
