"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useMemo, useState } from "react";
import { keepPreviousData, useMutation, useQueries, useQuery, useQueryClient } from "@tanstack/react-query";
import { Paperclip } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { DynamicFormModel, DynamicFormFieldModel, DynamicFormRecordModel } from "@/models";
import { getRecords, saveRecord, deleteRecord } from "@/services/admin/dynamicForm";
import { getLookup } from "@/services/admin/lookup";
import ChildManager, { type ChildColumn } from "@/components/admin/employee/childManager";
import DocumentAttachments from "@/components/admin/employee/documentAttachments";
import { buildCustomFieldComponents } from "@/components/admin/employee/customFieldConfigs";
import { StatusMessage } from "@/components/common/statusMessage/status";
import { parameterInitialData } from "@/constants/initialization";
import type ParameterModel from "@/models/ParameterModel";

const FormProvider = memo(FormProviders);
const DEFAULT_TAKE = 15;

/** Renders one grid cell value for a field by its data type. */
function formatCell(field: DynamicFormFieldModel, raw: unknown) {
  const v = raw == null ? "" : String(raw);
  if (!v) return "—";
  if (field.dataType === "Boolean") return v === "true" ? "Yes" : "No";
  if (field.dataType === "Date") return v.slice(0, 10);
  return v;
}

/** Paperclip + count cell for an Attachment column (same look as the Education/Experience grids). */
function docCountCell(n?: number) {
  return n && n > 0 ? (
    <span className="inline-flex items-center gap-1 text-xs text-foreground">
      <Paperclip size={12} /> {n}
    </span>
  ) : (
    "—"
  );
}

/**
 * Metadata-driven, module-agnostic child collection. Rendered with the SAME building blocks as the
 * hardcoded employee child tabs (Education/Experience/…): the shared `ChildManager` table + modal
 * `FormProvider`, so custom tabs look identical to their siblings. Records are **server-paged**
 * (the fetch + JSON parse is bounded to one page); the standard `Pagination` appears only when a form
 * accumulates more than one page, so small collections look exactly like the fixed tabs.
 */
function DynamicFormSection({
  form,
  ownerType,
  ownerId,
}: {
  form: DynamicFormModel;
  ownerType: string;
  ownerId: string;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [values, setValues] = useState<Record<string, string>>({});
  const [isSaving, setIsSaving] = useState(false);
  const [param, setParam] = useState<ParameterModel>(
    () => ({ ...parameterInitialData, take: DEFAULT_TAKE }) as ParameterModel,
  );

  const fields = form.fields ?? [];
  // Attachment fields hold files (EmployeeDocument), not a JSON value — each renders as its OWN
  // Documents panel (edit mode) rather than a form input, exactly like the Education/Experience tabs.
  const rawValueFields = fields.filter((f) => f.dataType !== "Attachment");
  const attachmentFields = fields.filter((f) => f.dataType === "Attachment");

  // Lookup-bound Select fields: fetch each bound category's values (cached 30 min, shared app-wide
  // via the ["lookup", code] key) and pre-resolve them into the renderer's optionsList.
  const lookupCodes = useMemo(
    () => [...new Set(rawValueFields.filter((f) => f.dataType === "Select" && f.lookupCategory).map((f) => f.lookupCategory!))],
    [rawValueFields],
  );
  const lookupQueries = useQueries({
    queries: lookupCodes.map((code) => ({
      queryKey: ["lookup", code],
      queryFn: () => getLookup(code),
      staleTime: 30 * 60 * 1000,
    })),
  });
  // No manual useMemo here: a spread dependency array isn't compiler-legal, and the React
  // Compiler memoizes this derivation automatically with precise per-query dependencies.
  const valueFields = rawValueFields.map((f) => {
    if (f.dataType !== "Select" || !f.lookupCategory) return f;
    const idx = lookupCodes.indexOf(f.lookupCategory);
    const items = idx >= 0 ? (lookupQueries[idx].data ?? []) : [];
    // Values are stored as the item NAME (same convention as static options → readable grids).
    return { ...f, optionsList: items.map((i) => ({ id: i.name, name: i.name })) };
  });
  // Base key (no paging) — invalidating it refreshes every loaded page after a write.
  const baseKey = ["dynamicRecords", form.id, ownerType, ownerId];
  const { data: page, isLoading } = useQuery({
    queryKey: [...baseKey, param.skip, param.take, param.searchText],
    queryFn: () => getRecords(form.id!, ownerType, ownerId, param),
    enabled: !!form.id && !!ownerId,
    placeholderData: keepPreviousData, // smooth page-to-page transitions
  });
  const rows = page?.data ?? [];
  const total = page?.total ?? 0;

  const invalidate = () => queryClient.invalidateQueries({ queryKey: baseKey });

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteRecord(id),
    onSuccess: (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      invalidate();
    },
  });

  const columns = useMemo<ChildColumn<DynamicFormRecordModel>[]>(
    () =>
      fields
        .filter((f) => f.showInList)
        .map((f) => ({
          name: (f.name ?? "") as keyof DynamicFormRecordModel & string,
          label: f.label ?? f.name ?? "",
          render:
            f.dataType === "Attachment"
              ? (_v, r) => docCountCell(r.documentCounts?.[f.name ?? ""])
              : (_v, r) => formatCell(f, r.data?.[f.name ?? ""]),
        })),
    [fields],
  );

  const open = (record: DynamicFormRecordModel | null) => {
    setEditingId(record?.id ?? null);
    const next: Record<string, string> = {};
    for (const [k, v] of Object.entries(record?.data ?? {})) next[k] = v ?? "";
    setValues(next);
    setFormState({});
    setShowForm(true);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setValues((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setValues((p) => ({ ...p, [name]: r.id }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    const result = await saveRecord({
      id: editingId ?? undefined,
      dynamicFormId: form.id,
      ownerType,
      ownerId,
      data: values,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      invalidate();
      setShowForm(false);
    }
  };

  // The record modal's inputs ARE the form's value fields — bind directly to `values` (prefix "").
  const components = useMemo(
    () => buildCustomFieldComponents(valueFields, values, changeHandler, selectHandler, "", formState?.zodErrors),
    [valueFields, values, changeHandler, selectHandler, formState],
  );

  return (
    <>
      <ChildManager
        title={form.label ?? form.name ?? "Records"}
        addLabel="Add Record"
        columns={columns}
        rows={rows}
        isLoading={isLoading}
        error={error}
        onAdd={() => open(null)}
        onEdit={open}
        onDelete={(id) => remove(id)}
        paging={{ param, setParam, total }}
        formOpen={showForm}
        formTitle={editingId ? `Edit ${form.label}` : `Add ${form.label}`}
        onBack={() => setShowForm(false)}
        formView={
        <FormProvider
          form={{
            columnsNo: 2,
            submitHandler,
            labelWidth: "w-[35%]",
            isPending: isSaving,
            SubmitButton: "top",
            // Unique per form: hosts (e.g. the OrganizationUnit/Position edit modals) are themselves
            // FormProviders — with the default formId the inline record form's Save would submit the
            // HOST form instead (default-id collision).
            formId: `dynRecord-${form.id}`,
            submitBtnTitle: "Save",
            components,
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
          {attachmentFields.length > 0 &&
            (editingId ? (
              // One separate file pool per Attachment field (scoped by ownerField = the field name).
              attachmentFields.map((f) => (
                <DocumentAttachments
                  key={f.id ?? f.name}
                  employeeId={ownerId}
                  ownerType="DynamicFormRecord"
                  ownerId={editingId}
                  ownerField={f.name}
                  title={f.label}
                />
              ))
            ) : (
              <p className="mt-3 text-xs text-muted">{t("Save the record first to attach documents.")}</p>
            ))}
        </FormProvider>
        }
      />
    </>
  );
}

export default DynamicFormSection;
