"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Paperclip } from "lucide-react";
import type { CandidateEducationModel } from "@/models";
import {
  getCandidateEducations,
  saveCandidateEducation,
  deleteCandidateEducation,
} from "@/services/admin/recruitment";
import ChildManager, { type ChildColumn } from "../employee/childManager";
import BackgroundAttachments from "./backgroundAttachments";
import { StatusMessage } from "../../common/statusMessage/status";

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

const COLUMNS: ChildColumn<CandidateEducationModel>[] = [
  { name: "educationLevel", label: "Level" },
  { name: "institution", label: "Institution" },
  { name: "fieldOfStudy", label: "Field of Study" },
  { name: "qualification", label: "Qualification" },
  { name: "graduationYear", label: "Graduation Year" },
  { name: "documentCount", label: "Documents", render: docCountCell },
];

/**
 * Structured candidate education. Writes the SAME person-owned rows the employee profile
 * uses, so at hire they appear on the employee's Education tab with no re-entry. For an
 * internal candidate the rows belong to the employee master, so the section is read-only.
 */
function CandidateEducationSection({ candidateId, readOnly }: { candidateId: string; readOnly?: boolean }) {
  const { t } = useTranslation();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<CandidateEducationModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<CandidateEducationModel>({});
  const [isSaving, setIsSaving] = useState(false);

  const queryKey = ["candidateEducations", candidateId];
  const { data: rows, isLoading, isError, error: queryError, refetch } = useQuery({
    queryKey,
    queryFn: () => getCandidateEducations(candidateId),
    enabled: !!candidateId,
  });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteCandidateEducation(id),
    onSuccess: async (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      await refetch();
    },
  });

  const open = (record: CandidateEducationModel | null) => {
    setEditing(record);
    setFormData(record ?? {});
    setFormState({});
    setShowForm(true);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    const result = await saveCandidateEducation(candidateId, formData);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      await refetch();
      setShowForm(false);
    }
  };

  return (
    <>
      <ChildManager
        title="Educational Background"
        addLabel="Add Education"
        columns={COLUMNS}
        rows={rows}
        isLoading={isLoading}
        error={error ?? (isError ? ((queryError as Error)?.message || "Failed to load records") : null)}
        readOnly={readOnly}
        hint={readOnly ? "Maintained on the employee record — read-only for internal applicants." : undefined}
        onAdd={() => open(null)}
        onEdit={open}
        onDelete={(id) => remove(id)}
      />
      {showForm && (
        <FormProvider
          form={{
            // Unique id is REQUIRED: the modal's Save button lives in the modal footer (outside the
            // <form>) and targets it via form="<id>". With the default "formProvider" id it would
            // submit the page's main candidate form instead (duplicate DOM ids resolve to the first).
            formId: "candidateEducationForm",
            columnsNo: 2,
            submitHandler,
            labelWidth: "w-[35%]",
            isPending: isSaving,
            SubmitButton: "top",
            showModal: true,
            modalVisible: true,
            modalTitle: editing ? "Edit Education" : "Add Education",
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
            ],
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
          {formData.id ? (
            <BackgroundAttachments
              candidateId={candidateId}
              ownerType="Education"
              ownerId={formData.id}
              readOnly={readOnly}
            />
          ) : (
            <p className="mt-3 text-xs text-muted">{t("Save the record first to attach documents.")}</p>
          )}
        </FormProvider>
      )}
    </>
  );
}

export default CandidateEducationSection;
