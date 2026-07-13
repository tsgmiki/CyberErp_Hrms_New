"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Paperclip, Building2, Landmark } from "lucide-react";
import type { EmployeeExperienceModel } from "@/models";
import { getExperiences, saveExperience, deleteExperience } from "@/services/admin/employee/children";
import ChildManager, { type ChildColumn } from "./childManager";
import DocumentAttachments from "./documentAttachments";
import { useCustomFields } from "./customFieldsHook";
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

const COLUMNS: ChildColumn<EmployeeExperienceModel>[] = [
  { name: "organization", label: "Organization" },
  { name: "jobTitle", label: "Role / Job Title" },
  { name: "startDate", label: "From", render: fmtDate },
  { name: "endDate", label: "To", render: fmtDate },
  {
    name: "isExternal",
    label: "Type",
    render: (_v, r) => (
      <span className="inline-flex flex-wrap items-center gap-1">
        <span
          className={`rounded px-1.5 py-0.5 text-[10px] font-semibold ${
            r.isExternal ? "bg-info/15 text-info" : "bg-secondary text-muted"
          }`}
        >
          {r.isExternal ? "External" : "Internal"}
        </span>
        {r.isGovernmental ? (
          <span className="rounded bg-primary/15 px-1.5 py-0.5 text-[10px] font-semibold text-primary">Gov</span>
        ) : null}
      </span>
    ),
  },
  { name: "documentCount", label: "Documents", render: docCountCell },
];

function ExperienceSection({ employeeId }: { employeeId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<EmployeeExperienceModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<EmployeeExperienceModel>({});
  const [isSaving, setIsSaving] = useState(false);
  const customFields = useCustomFields("Experience");

  const queryKey = ["employeeExperiences", employeeId];
  const { data: rows, isLoading } = useQuery({
    queryKey,
    queryFn: () => getExperiences(employeeId),
  });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteExperience(id),
    onSuccess: (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const open = (record: EmployeeExperienceModel | null) => {
    setEditing(record);
    setFormData(
      record
        ? { ...record, startDate: fmtDate(record.startDate), endDate: fmtDate(record.endDate) }
        : { isExternal: true }, // a manually-added job is an external employer by default
    );
    customFields.hydrate(record?.customFields);
    setFormState({});
    setShowForm(true);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const checkboxHandler = useCallback((e: any) => {
    const { name, checked } = e.target;
    setFormData((p) => ({ ...p, [name]: checked }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsSaving(true);
    const result = await saveExperience(fd);
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
        title="Employment History"
        addLabel="Add Experience"
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
            modalTitle: editing ? "Edit Experience" : "Add Experience",
            description: "Prior employment history.",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Save",
            components: [
              { name: "organization", label: "Organization", required: true, value: formData.organization, onChange: changeHandler, error: formState?.zodErrors?.organization, type: "text" },
              { name: "jobTitle", label: "Role / Job Title", required: true, value: formData.jobTitle, onChange: changeHandler, error: formState?.zodErrors?.jobTitle, type: "text" },
              { name: "startDate", label: "From", value: formData.startDate, onChange: changeHandler, type: "date" },
              { name: "endDate", label: "To", value: formData.endDate, onChange: changeHandler, type: "date" },
              { name: "responsibilities", label: "Responsibilities", value: formData.responsibilities, onChange: changeHandler, type: "textarea", colSpan: "full" },
              {
                // Employee-form styled toggle rows (border + icon + helper). The checkbox inputs
                // carry their own `name`, so they post through the form's FormData like any field.
                name: "employmentFlags", type: "custom", colSpan: "full",
                customChildren: (
                  <div className="space-y-3">
                    <label className="flex w-full cursor-pointer items-center gap-3 rounded-lg border border-border bg-secondary/30 p-3 transition hover:border-primary/40">
                      <input type="checkbox" name="isExternal" checked={!!formData.isExternal} onChange={checkboxHandler} className="h-4 w-4 shrink-0 accent-primary" />
                      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                        <Building2 size={16} />
                      </span>
                      <span className="min-w-0">
                        <span className="block text-sm font-medium text-foreground">{t("External employment")}</span>
                        <span className="block text-xs text-muted">{t("A prior job at another employer. Uncheck for an internal role.")}</span>
                      </span>
                    </label>
                    <label className="flex w-full cursor-pointer items-center gap-3 rounded-lg border border-border bg-secondary/30 p-3 transition hover:border-primary/40">
                      <input type="checkbox" name="isGovernmental" checked={!!formData.isGovernmental} onChange={checkboxHandler} className="h-4 w-4 shrink-0 accent-primary" />
                      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                        <Landmark size={16} />
                      </span>
                      <span className="min-w-0">
                        <span className="block text-sm font-medium text-foreground">{t("Governmental organization")}</span>
                        <span className="block text-xs text-muted">{t("The employer was a government body or public institution.")}</span>
                      </span>
                    </label>
                  </div>
                ),
              },
              ...customFields.components,
              { name: "employeeId", value: employeeId, type: "hidden" },
              { name: "id", value: formData.id, type: "hidden" },
            ],
          }}
        >
          <p className="mx-1 mb-1 rounded-md border border-info/25 bg-info/10 px-3 py-2 text-[11px] text-info">
            {t("New entries default to external employment. Internal roles are also added automatically when a movement is executed.")}
          </p>
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
          {formData.id ? (
            <DocumentAttachments employeeId={employeeId} ownerType="Experience" ownerId={formData.id} />
          ) : (
            <p className="mt-3 text-xs text-muted">{t("Save the record first to attach documents.")}</p>
          )}
        </FormProvider>
      )}
    </>
  );
}

export default ExperienceSection;
