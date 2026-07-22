"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save, GripVertical } from "lucide-react";
import type { DynamicFormModel, DynamicFormFieldModel } from "@/models";
import { getForm, saveForm } from "@/services/admin/dynamicForm";
import { dynamicFormFieldTypeOptions } from "@/constants/orgStructure";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableField extends DynamicFormFieldModel {
  _key: number;
}

const MODULE = "Employee";

function FormBuilderForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<DynamicFormModel>({ module: MODULE, isActive: true, sortOrder: 0 });
  const [fields, setFields] = useState<EditableField[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["dynamicForm", id],
    queryFn: () => getForm(id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (record) {
      setMeta(record);
      setFields((record.fields ?? []).map((f) => ({ ...f, _key: nextKey() })));
    }
  }, [record]);

  const setMetaField = (name: keyof DynamicFormModel, value: unknown) =>
    setMeta((p) => ({ ...p, [name]: value }));

  const addField = () =>
    setFields((p) => [
      ...p,
      { _key: nextKey(), name: "", label: "", dataType: "Text", isRequired: false, isActive: true, showInList: true, sortOrder: p.length },
    ]);
  const updateField = (key: number, patch: Partial<EditableField>) =>
    setFields((p) => p.map((f) => (f._key === key ? { ...f, ...patch } : f)));
  const removeField = (key: number) => setFields((p) => p.filter((f) => f._key !== key));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const payload: DynamicFormModel = {
      ...meta,
      module: MODULE,
      fields: fields.map(({ _key, ...f }, i) => ({ ...f, sortOrder: i })),
    };
    const result = await saveForm(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      // Refresh both the admin list and the active-forms metadata that drives the profile tabs.
      queryClient.invalidateQueries({ queryKey: ["dynamicFormsList"] });
      queryClient.invalidateQueries({ queryKey: ["dynamicForms"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  return (
    <form onSubmit={submit} className="space-y-5 text-foreground">
      {/* Tab metadata */}
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Tab Details")}</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Tab Label")} *</label>
            <input className={INPUT} value={meta.label ?? ""} onChange={(e) => setMetaField("label", e.target.value)} placeholder="Certifications" required />
          </div>
          <div>
            <label className={LABEL}>{t("Key")} *</label>
            <input className={INPUT} value={meta.name ?? ""} onChange={(e) => setMetaField("name", e.target.value)} placeholder="certifications" required />
          </div>
          <div>
            <label className={LABEL}>{t("Sort Order")}</label>
            <input type="number" className={INPUT} value={meta.sortOrder ?? 0} onChange={(e) => setMetaField("sortOrder", Number(e.target.value))} />
          </div>
          <div className="flex items-end gap-2 pb-1">
            <input id="frm-active" type="checkbox" className="h-4 w-4 accent-primary" checked={meta.isActive ?? true} onChange={(e) => setMetaField("isActive", e.target.checked)} />
            <label htmlFor="frm-active" className="text-sm">{t("Active (tab is visible)")}</label>
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Description")}</label>
            <input className={INPUT} value={meta.description ?? ""} onChange={(e) => setMetaField("description", e.target.value)} placeholder={t("Shown under the modal title") ?? ""} />
          </div>
        </div>
      </section>

      {/* Field schema */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("Fields")}</h3>
          <button type="button" onClick={addField} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
            <Plus className="h-3.5 w-3.5" /> {t("Add Field")}
          </button>
        </div>

        {fields.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No fields yet. Add at least one field.")}</p>
        ) : (
          <div className="space-y-2">
            {fields.map((f) => (
              <div key={f._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[16px_1fr_1fr_130px_1fr_auto]">
                <GripVertical className="hidden h-4 w-4 self-center text-muted md:block" />
                <div>
                  <label className={LABEL}>{t("Label")} *</label>
                  <input className={INPUT} value={f.label ?? ""} onChange={(e) => updateField(f._key, { label: e.target.value })} required />
                </div>
                <div>
                  <label className={LABEL}>{t("Key")} *</label>
                  <input className={INPUT} value={f.name ?? ""} onChange={(e) => updateField(f._key, { name: e.target.value })} required />
                </div>
                <div>
                  <label className={LABEL}>{t("Type")}</label>
                  <select className={INPUT} value={f.dataType ?? "Text"} onChange={(e) => updateField(f._key, { dataType: e.target.value })}>
                    {dynamicFormFieldTypeOptions.map((o) => (
                      <option key={o.id} value={o.id}>{o.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={LABEL}>{t("Options (comma-sep)")}</label>
                  <input className={INPUT} value={f.options ?? ""} onChange={(e) => updateField(f._key, { options: e.target.value })} disabled={f.dataType !== "Select"} placeholder={f.dataType === "Select" ? "A,B,C" : "—"} />
                </div>
                <div className="flex items-center gap-3 pb-2">
                  <label className="flex items-center gap-1 text-xs" title={t("Required") ?? ""}>
                    <input type="checkbox" className="h-4 w-4 accent-primary" checked={!!f.isRequired} onChange={(e) => updateField(f._key, { isRequired: e.target.checked })} /> {t("Req")}
                  </label>
                  <label className="flex items-center gap-1 text-xs" title={t("Show as a column in the list") ?? ""}>
                    <input type="checkbox" className="h-4 w-4 accent-primary" checked={f.showInList ?? true} onChange={(e) => updateField(f._key, { showInList: e.target.checked })} /> {t("List")}
                  </label>
                  <button type="button" onClick={() => removeField(f._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <button type="submit" disabled={isSaving} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
          <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Form")}
        </button>
      </div>
    </form>
  );
}

export default memo(FormBuilderForm);
