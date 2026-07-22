"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { DisciplinaryMeasureModel } from "@/models";
import {
  getDisciplinaryMeasures,
  saveDisciplinaryMeasure,
  deleteDisciplinaryMeasure,
} from "@/services/admin/employee/personnelActions";
import ChildManager, { type ChildColumn } from "./childManager";
import { useCustomFields } from "./customFieldsHook";
import { StatusMessage } from "../../common/statusMessage/status";
import {
  measureTypeOptions,
  measureTypeLabel,
  disciplinaryStatusOptions,
  disciplinaryStatusLabel,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const fmtDate = (v: unknown) => (typeof v === "string" && v ? v.slice(0, 10) : "");

const STATUS_TONE: Record<string, string> = {
  Open: "bg-warning/15 text-warning",
  UnderReview: "bg-info/15 text-info",
  Resolved: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

const COLUMNS: ChildColumn<DisciplinaryMeasureModel>[] = [
  { name: "violationDate", label: "Violation Date", render: fmtDate },
  { name: "violationType", label: "Violation" },
  { name: "measureType", label: "Measure", render: (v) => measureTypeLabel(String(v ?? "")) },
  {
    name: "status",
    label: "Status",
    render: (v) => (
      <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[String(v)] ?? "bg-muted/30 text-muted"}`}>
        {disciplinaryStatusLabel(String(v ?? ""))}
      </span>
    ),
  },
  {
    name: "validUntil",
    label: "Lifetime / Impact",
    render: (v, r) => (
      <span className="inline-flex items-center gap-1.5">
        <span className="text-xs text-muted">{v ? `until ${fmtDate(v)}` : "open-ended"}</span>
        {r.affectsPromotion && <span className="rounded bg-error/10 px-1 py-0.5 text-[10px] font-semibold text-error">Promo</span>}
        {r.affectsReward && <span className="rounded bg-error/10 px-1 py-0.5 text-[10px] font-semibold text-error">Reward</span>}
      </span>
    ),
  },
];

function DisciplineSection({ employeeId }: { employeeId: string }) {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<DisciplinaryMeasureModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<DisciplinaryMeasureModel>({});
  const [isSaving, setIsSaving] = useState(false);
  const customFields = useCustomFields("Discipline");

  const queryKey = ["employeeDiscipline", employeeId];
  const { data: rows, isLoading } = useQuery({
    queryKey,
    queryFn: () => getDisciplinaryMeasures(employeeId),
  });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteDisciplinaryMeasure(id),
    onSuccess: (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const open = (record: DisciplinaryMeasureModel | null) => {
    setEditing(record);
    setFormData(
      record
        ? {
            ...record,
            violationDate: fmtDate(record.violationDate),
            effectiveDate: fmtDate(record.effectiveDate),
            validUntil: fmtDate(record.validUntil),
          }
        : { measureType: "VerbalWarning", status: "Open" },
    );
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
  const checkHandler = useCallback((e: any) => {
    const { name, checked } = e.target;
    setFormData((p) => ({ ...p, [name]: checked }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsSaving(true);
    const result = await saveDisciplinaryMeasure(fd);
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
        title="Disciplinary Measures"
        addLabel="Add Case"
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
            modalTitle: editing ? "Edit Disciplinary Case" : "Add Disciplinary Case",
            description: "Record a disciplinary action and its outcome.",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Save",
            components: [
              {
                name: "violationDate", label: "Violation Date", required: true, type: "date",
                value: formData.violationDate, onChange: changeHandler,
                error: formState?.zodErrors?.violationDate,
              },
              {
                name: "violationType", label: "Violation Type", required: true, type: "text",
                placeholder: "e.g. Absenteeism, Misconduct",
                value: formData.violationType, onChange: changeHandler,
                error: formState?.zodErrors?.violationType,
              },
              {
                name: "measureType", label: "Measure", required: true, type: "dropDown",
                onSelect: selectHandler, value: formData.measureType,
                displayValue: measureTypeLabel(formData.measureType),
                error: formState?.zodErrors?.measureType, data: measureTypeOptions as never,
              },
              {
                name: "status", label: "Status", type: "dropDown", onSelect: selectHandler,
                value: formData.status, displayValue: disciplinaryStatusLabel(formData.status),
                data: disciplinaryStatusOptions as never,
              },
              {
                name: "effectiveDate", label: "Effective Date", type: "date",
                value: formData.effectiveDate, onChange: changeHandler,
              },
              {
                name: "validUntil", label: "Valid Until (lifetime)", type: "date",
                value: formData.validUntil, onChange: changeHandler,
              },
              {
                name: "affectsPromotion", label: "Blocks promotion", type: "checkbox",
                value: formData.affectsPromotion ? "true" : "", onChange: checkHandler,
              },
              {
                name: "affectsReward", label: "Blocks reward", type: "checkbox",
                value: formData.affectsReward ? "true" : "", onChange: checkHandler,
              },
              { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
              { name: "resolution", label: "Resolution", value: formData.resolution, onChange: changeHandler, type: "textarea", colSpan: "full" },
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

export default DisciplineSection;
