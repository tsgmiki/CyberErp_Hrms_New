"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Paperclip } from "lucide-react";
import type { EmployeeEducationModel, FormComponentModel } from "@/models";
import ChildManager, { type ChildColumn } from "@/components/admin/employee/childManager";
import { useCustomFields } from "@/components/admin/employee/customFieldsHook";
import { StatusMessage } from "@/components/common/statusMessage/status";
import type { BackgroundDataSource } from "./types";

const FormProvider = memo(FormProviders);
const docCountCell = (v: unknown) => {
  const n = Number(v) || 0;
  return n > 0 ? (
    <span className="inline-flex items-center gap-1 text-xs text-foreground">
      <Paperclip size={12} /> {n}
    </span>
  ) : (
    "—"
  );
};

const COLUMNS: ChildColumn<EmployeeEducationModel>[] = [
  { name: "educationLevel", label: "Level" },
  { name: "institution", label: "Institution" },
  { name: "fieldOfStudy", label: "Field of Study" },
  { name: "qualification", label: "Qualification" },
  { name: "graduationYear", label: "Graduation Year" },
  { name: "documentCount", label: "Documents", render: docCountCell },
];

/**
 * Educational-background collection — shared by the Employee and Candidate modules (they differ only
 * in the `ds` adapter), so the fields, columns, dynamic custom fields (HC021, OwnerType=Education) and
 * attachments are identical in both places by construction.
 */
function EducationSection({ ds }: { ds: BackgroundDataSource<EmployeeEducationModel> }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<EmployeeEducationModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<EmployeeEducationModel>({});
  const [isSaving, setIsSaving] = useState(false);
  const customFields = useCustomFields("Education");

  const { data: rows, isLoading, isError, error: queryError } = useQuery({
    queryKey: ds.queryKey,
    queryFn: () => ds.list(),
    enabled: !!ds.ownerId,
  });
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ds.queryKey });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => ds.remove(id),
    onSuccess: (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      invalidate();
    },
  });

  const open = (record: EmployeeEducationModel | null) => {
    setEditing(record);
    setFormData(record ?? {});
    customFields.hydrate(record?.customFields);
    setFormState({});
    setShowForm(true);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsSaving(true);
    const result = await ds.save(fd);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      invalidate();
      setShowForm(false);
    }
  };

  const ownerHidden: FormComponentModel[] = ds.ownerIdField
    ? [{ name: ds.ownerIdField.name, value: ds.ownerIdField.value, type: "hidden" }]
    : [];

  return (
    <>
      <ChildManager
        title="Educational Background"
        addLabel="Add Education"
        columns={COLUMNS}
        rows={rows}
        isLoading={isLoading}
        error={error ?? (isError ? ((queryError as Error)?.message || "Failed to load records") : null)}
        readOnly={ds.readOnly}
        hint={ds.hint}
        onAdd={() => open(null)}
        onEdit={open}
        onDelete={(id) => remove(id)}
      />
      {showForm && (
        <FormProvider
          form={{
            formId: "backgroundEducationForm",
            columnsNo: 2,
            submitHandler,
            fieldLayout: "auth",
            isPending: isSaving,
            SubmitButton: "top",
            showModal: true,
            modalVisible: true,
            modalTitle: editing ? "Edit Education" : "Add Education",
            description: "Academic qualifications and certifications.",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Save",
            components: [
              { name: "educationLevel", label: "Education Level", placeholder: "e.g. BSc, MSc, Certification", required: true, value: formData.educationLevel, onChange: changeHandler, error: formState?.zodErrors?.educationLevel, type: "text" },
              { name: "institution", label: "Institution", required: true, value: formData.institution, onChange: changeHandler, error: formState?.zodErrors?.institution, type: "text" },
              { name: "fieldOfStudy", label: "Field of Study", value: formData.fieldOfStudy, onChange: changeHandler, type: "text" },
              { name: "qualification", label: "Qualification", value: formData.qualification, onChange: changeHandler, type: "text" },
              { name: "graduationYear", label: "Graduation Year", value: formData.graduationYear, onChange: changeHandler, inputType: "number", type: "text" },
              { name: "remark", label: "Remark", value: formData.remark, onChange: changeHandler, type: "textarea", colSpan: "full" },
              ...customFields.components,
              ...ownerHidden,
              { name: "id", value: formData.id, type: "hidden" },
            ],
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
          {formData.id ? (
            ds.renderAttachments(formData.id)
          ) : (
            <p className="mt-3 text-xs text-muted">{t("Save the record first to attach documents.")}</p>
          )}
        </FormProvider>
      )}
    </>
  );
}

export default EducationSection;
