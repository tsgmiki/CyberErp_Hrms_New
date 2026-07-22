"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { OrganizationUnitModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveOrganizationUnit from "@/services/admin/organizationUnit/save";
import getOrganizationUnit from "@/services/admin/organizationUnit/get";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
import getAllBranch from "@/services/admin/branch/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { organizationUnitTypes, activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

interface Props {
  id: string;
  /** Pre-selected parent when adding under a tree node. */
  presetParentId?: string;
  presetParentName?: string;
  onClose: () => void;
  onSaved: () => void;
}

function OrganizationUnitForm({ id, presetParentId, presetParentName, onClose, onSaved }: Props) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<OrganizationUnitModel>(() =>
    presetParentId ? { parentId: presetParentId, parentName: presetParentName } : {},
  );
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record } = useQuery({
    queryKey: ["organizationUnit", id],
    queryFn: () => getOrganizationUnit(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [parentParam, setParentParam] = useState({ ...lookupParam });
  const { data: parents, isLoading: parentsLoading } = useQuery({
    queryKey: ["organizationUnits", parentParam],
    queryFn: () => getAllOrganizationUnit(parentParam),
  });
  const [locationParam, setLocationParam] = useState({ ...lookupParam });
  const { data: locations, isLoading: locationsLoading } = useQuery({
    queryKey: ["workLocations", locationParam],
    queryFn: () => getAllWorkLocation(locationParam),
  });
  const [branchParam, setBranchParam] = useState({ ...lookupParam });
  const { data: branches, isLoading: branchesLoading } = useQuery({
    queryKey: ["branches", branchParam],
    queryFn: () => getAllBranch(branchParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveOrganizationUnit(fd);
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
      queryClient.invalidateQueries({ queryKey: ["organizationUnits"] });
      queryClient.invalidateQueries({ queryKey: ["organizationTree"] });
      onSaved();
      onClose();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  const parentOptions = (parents?.data ?? [])
    .filter((o) => o.id !== formData.id)
    .map((o) => ({ id: o.id, name: `${o.name} (${o.unitType})` }));

  return (
    <FormProvider
      ref={formRef}
      form={{
        columnsNo: 2,
        submitHandler,
        labelWidth: "w-[35%]",
        isPending: isLoading,
        SubmitButton: "top",
        showModal: true,
        modalVisible: true,
        modalTitle: id ? "Edit Organization Unit" : "Add Organization Unit",
        modalSize: "xl",
        onModalClose: onClose,
        submitBtnTitle: "Save",
        components: [
          { name: "code", label: "Code", placeholder: "Code", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
          { name: "name", label: "Name", placeholder: "Name", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
          {
            name: "unitType", label: "Unit Type", required: true, type: "dropDown", onSelect: selectHandler,
            value: formData.unitType, displayValue: formData.unitType,
            error: formState?.zodErrors?.unitType, data: organizationUnitTypes as never,
          },
          {
            name: "branchId", label: "Branch", type: "dropDown", onSelect: selectHandler,
            value: formData.branchId, displayValue: formData.branchName,
            param: branchParam, setParam: setBranchParam as any, isLoading: branchesLoading,
            data: (branches?.data ?? []).map((b) => ({ id: b.id, name: b.name })) as never,
          },
          {
            name: "parentId", label: "Parent Unit", type: "dropDown", onSelect: selectHandler,
            value: formData.parentId, displayValue: formData.parentName,
            // Locked when adding under a node selected in the tree — parentId is already set.
            disabled: Boolean(presetParentId),
            param: parentParam, setParam: setParentParam as any, isLoading: parentsLoading,
            data: parentOptions as never,
          },
          {
            name: "workLocationId", label: "Work Location", type: "dropDown", onSelect: selectHandler,
            value: formData.workLocationId, displayValue: formData.workLocationName,
            param: locationParam, setParam: setLocationParam as any, isLoading: locationsLoading,
            data: (locations?.data ?? []).map((l) => ({ id: l.id, name: l.name })) as never,
          },
          { name: "allocatedHeadcount", label: "Allocated Headcount", value: formData.allocatedHeadcount, onChange: changeHandler, inputType: "number", type: "text" },
          {
            name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
            value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
            data: activeStatusOptions as never,
          },
          { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
          { name: "id", value: formData.id, type: "hidden" },
        ],
      }}
    >
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </FormProvider>
  );
}
export default memo(OrganizationUnitForm);
