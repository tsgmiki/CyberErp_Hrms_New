"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Paperclip } from "lucide-react";
import type { CandidateExperienceModel } from "@/models";
import {
  getCandidateExperiences,
  saveCandidateExperience,
  deleteCandidateExperience,
} from "@/services/admin/recruitment";
import ChildManager, { type ChildColumn } from "../employee/childManager";
import BackgroundAttachments from "./backgroundAttachments";
import { StatusMessage } from "../../common/statusMessage/status";

const FormProvider = memo(FormProviders);
const fmtDate = (v: unknown) => (typeof v === "string" && v ? v.slice(0, 10) : "");

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

const COLUMNS: ChildColumn<CandidateExperienceModel>[] = [
  { name: "organization", label: "Organization" },
  { name: "jobTitle", label: "Role / Job Title" },
  { name: "startDate", label: "From", render: fmtDate },
  { name: "endDate", label: "To", render: fmtDate },
  { name: "documentCount", label: "Documents", render: docCountCell },
];

/**
 * Structured candidate work history. Writes the SAME person-owned rows the employee profile
 * uses (shared PersonId → automatic hand-off at hire). Read-only for internal applicants.
 */
function CandidateExperienceSection({ candidateId, readOnly }: { candidateId: string; readOnly?: boolean }) {
  const { t } = useTranslation();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<CandidateExperienceModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<CandidateExperienceModel>({});
  const [isSaving, setIsSaving] = useState(false);

  const queryKey = ["candidateExperiences", candidateId];
  const { data: rows, isLoading, isError, error: queryError, refetch } = useQuery({
    queryKey,
    queryFn: () => getCandidateExperiences(candidateId),
    enabled: !!candidateId,
  });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteCandidateExperience(id),
    onSuccess: async (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      await refetch();
    },
  });

  const open = (record: CandidateExperienceModel | null) => {
    setEditing(record);
    setFormData(
      record
        ? { ...record, startDate: fmtDate(record.startDate), endDate: fmtDate(record.endDate) }
        : {},
    );
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
    const result = await saveCandidateExperience(candidateId, formData);
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
        title="Employment History"
        addLabel="Add Experience"
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
            // Unique id is REQUIRED — see educationSection: the modal footer's Save button targets
            // the form via form="<id>", and the default "formProvider" collides with the main form.
            formId: "candidateExperienceForm",
            columnsNo: 2,
            submitHandler,
            labelWidth: "w-[35%]",
            isPending: isSaving,
            SubmitButton: "top",
            showModal: true,
            modalVisible: true,
            modalTitle: editing ? "Edit Experience" : "Add Experience",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Save",
            components: [
              { name: "organization", label: "Organization", required: true, value: formData.organization, onChange: changeHandler, error: formState?.zodErrors?.organization, type: "text" },
              { name: "jobTitle", label: "Role / Job Title", required: true, value: formData.jobTitle, onChange: changeHandler, error: formState?.zodErrors?.jobTitle, type: "text" },
              { name: "startDate", label: "From", value: formData.startDate, onChange: changeHandler, type: "date" },
              { name: "endDate", label: "To", value: formData.endDate, onChange: changeHandler, type: "date" },
              { name: "responsibilities", label: "Responsibilities", value: formData.responsibilities, onChange: changeHandler, type: "textarea", colSpan: "full" },
            ],
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
          {formData.id ? (
            <BackgroundAttachments
              candidateId={candidateId}
              ownerType="Experience"
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

export default CandidateExperienceSection;
