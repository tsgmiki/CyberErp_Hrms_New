"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { EmployeeDependentModel } from "@/models";
import { getDependents, saveDependent, deleteDependent } from "@/services/admin/employee/children";
import getAllEmployee from "@/services/admin/employee/getAll";
import ChildManager, { type ChildColumn } from "./childManager";
import { useCustomFields } from "./customFieldsHook";
import { StatusMessage } from "../../common/statusMessage/status";
import { parameterInitialData } from "@/constants/initialization";
import { relationshipOptions, yesNoOptions } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const fmtDate = (v: unknown) => (typeof v === "string" && v ? v.slice(0, 10) : "");

const COLUMNS: ChildColumn<EmployeeDependentModel>[] = [
  { name: "fullName", label: "Full Name" },
  { name: "relationship", label: "Relationship" },
  { name: "dateOfBirth", label: "Date of Birth", render: fmtDate },
  { name: "isDependent", label: "Dependent", render: (v) => (v ? "Yes" : "No") },
  {
    name: "relatedEmployeeName",
    label: "Related Employee",
    render: (v) => (v ? String(v) : "—"),
  },
];

function FamilySection({ employeeId }: { employeeId: string }) {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<EmployeeDependentModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<EmployeeDependentModel>({});
  const [isSaving, setIsSaving] = useState(false);
  const customFields = useCustomFields("Dependent");

  const queryKey = ["employeeDependents", employeeId];
  const { data: rows, isLoading } = useQuery({
    queryKey,
    queryFn: () => getDependents(employeeId),
  });

  // Internal-relationship lookup (HC020) — other employees in the organization.
  const [employeeParam, setEmployeeParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: employees, isLoading: employeesLoading } = useQuery({
    queryKey: ["employees", employeeParam],
    queryFn: () => getAllEmployee(employeeParam),
  });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteDependent(id),
    onSuccess: (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const open = (record: EmployeeDependentModel | null) => {
    setEditing(record);
    setFormData(record ? { ...record, dateOfBirth: fmtDate(record.dateOfBirth) } : {});
    customFields.hydrate(record?.customFields);
    setFormState({});
    setShowForm(true);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsSaving(true);
    const result = await saveDependent(fd);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey });
      setShowForm(false);
    }
  };

  return (
    <>
      <ChildManager
        title="Family & Internal Relationships"
        addLabel="Add Family Member"
        columns={COLUMNS}
        rows={rows}
        isLoading={isLoading}
        error={error}
        onAdd={() => open(null)}
        onEdit={open}
        onDelete={(id) => remove(id)}
      />
      {showForm && (
        <FormProvider
          form={{
            columnsNo: 2,
            submitHandler,
            fieldLayout: "auth",
            isPending: isSaving,
            SubmitButton: "top",
            showModal: true,
            modalVisible: true,
            modalTitle: editing ? "Edit Family Member" : "Add Family Member",
            description: "Dependents and next of kin.",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Save",
            components: [
              { name: "fullName", label: "Full Name", required: true, value: formData.fullName, onChange: changeHandler, error: formState?.zodErrors?.fullName, type: "text" },
              {
                name: "relationship", label: "Relationship", required: true, type: "dropDown", onSelect: selectHandler,
                value: formData.relationship, displayValue: formData.relationship,
                error: formState?.zodErrors?.relationship, data: relationshipOptions as never,
              },
              { name: "dateOfBirth", label: "Date of Birth", value: formData.dateOfBirth, onChange: changeHandler, type: "date" },
              { name: "phoneNumber", label: "Phone Number", value: formData.phoneNumber, onChange: changeHandler, type: "text" },
              {
                name: "isDependent", label: "Dependent", type: "dropDown", onSelect: selectHandler,
                value: formData.isDependent === true ? "true" : "false",
                displayValue: formData.isDependent === true ? "Yes" : "No",
                data: yesNoOptions as never,
              },
              {
                name: "relatedEmployeeId", label: "Related Employee (optional)", type: "dropDown", onSelect: selectHandler,
                value: formData.relatedEmployeeId, displayValue: formData.relatedEmployeeName,
                param: employeeParam, setParam: setEmployeeParam as any, isLoading: employeesLoading,
                data: (employees?.data ?? [])
                  .filter((e) => e.id !== employeeId)
                  .map((e) => ({ id: e.id, name: `${e.fullName} (${e.employeeNumber})` })) as never,
              },
              { name: "address", label: "Address", value: formData.address, onChange: changeHandler, type: "textarea", colSpan: "full" },
              { name: "remark", label: "Remark", value: formData.remark, onChange: changeHandler, type: "textarea", colSpan: "full" },
              ...customFields.components,
              { name: "employeeId", value: employeeId, type: "hidden" },
              { name: "id", value: formData.id, type: "hidden" },
            ],
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
        </FormProvider>
      )}
    </>
  );
}

export default FamilySection;
