"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save, GripVertical, Settings2, ListChecks } from "lucide-react";
import type { DynamicFormModel, DynamicFormFieldModel } from "@/models";
import { getForm, saveForm } from "@/services/admin/dynamicForm";
import { useLookupCategories } from "@/services/admin/lookup";
import { dynamicFormFieldTypeOptions, dynamicFormModuleOptions } from "@/constants/orgStructure";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import ButtonField from "@/components/ui/buttonField";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableField extends DynamicFormFieldModel {
  _key: number;
}

function FormBuilderForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<DynamicFormModel>({ module: "Employee", isActive: true, sortOrder: 0 });
  const [fields, setFields] = useState<EditableField[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["dynamicForm", id],
    queryFn: () => getForm(id),
    enabled: id !== "",
  });

  // Lookup categories for binding Select fields to the centralized lookup system.
  const { data: lookupCategories } = useLookupCategories();

  // Attachment fields ride the employee-document subsystem — offered for the Employee module only.
  const typeOptions =
    meta.module === "Employee"
      ? dynamicFormFieldTypeOptions
      : dynamicFormFieldTypeOptions.filter((o) => o.id !== "Attachment");

  useEffect(() => {
    if (record) {
      setMeta(record);
      setFields((record.fields ?? []).map((f) => ({ ...f, _key: nextKey() })));
    }
  }, [record]);
  // stale-form guard: when the id is cleared (back / Add-new) while this form stays
  // mounted, drop the previously loaded record so Add never shows stale values.
  useEffect(() => {
    if (!id) {
      setMeta({ module: "Employee", isActive: true, sortOrder: 0 });
      setFields([]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);


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
      /* route-id fallback: an unloaded record must not degrade this update into a create */
      id: meta.id || id || undefined,
      module: meta.module || "Employee",
      fields: fields.map(({ _key, ...f }, i) => ({ ...f, sortOrder: i })),
    };
    const result = await saveForm(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      // Refresh both the admin list and the active-forms metadata that drives the profile tabs.
      queryClient.invalidateQueries({ queryKey: ["dynamicFormsList"] });
      queryClient.invalidateQueries({ queryKey: ["dynamicForms"] });
      // The record's OWN cache entry, not just the list: without this the detail query
      // ["dynamicForm", id] kept the pre-save copy and the client's 30 s staleTime served it to the
      // next Edit WITHOUT refetching -- grid fresh, form stale until a full page reload.
      queryClient.invalidateQueries({ queryKey: ["dynamicForm"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  // Native `required` is intentionally absent: both tab panels stay mounted (hidden) so their state
  // survives switches, and a hidden required control would block submit invisibly. The backend
  // validators enforce the * fields and surface through StatusMessage below the tabs.
  const detailsTab = (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Module")} *</label>
            {/* Module is fixed once created — records are keyed by it (backend Update never changes it). */}
            <select
              className={INPUT}
              value={meta.module ?? "Employee"}
              onChange={(e) => {
                setMetaField("module", e.target.value);
                // Attachment fields are Employee-only — drop them when leaving that module.
                if (e.target.value !== "Employee")
                  setFields((p) => p.filter((f) => f.dataType !== "Attachment"));
              }}
              disabled={id !== ""}
            >
              {dynamicFormModuleOptions.map((o) => (
                <option key={o.id} value={o.id}>{t(o.name)}</option>
              ))}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Tab Label")} *</label>
            <input className={INPUT} value={meta.label ?? ""} onChange={(e) => setMetaField("label", e.target.value)} placeholder="Certifications" />
          </div>
          <div>
            <label className={LABEL}>{t("Key")} *</label>
            <input className={INPUT} value={meta.name ?? ""} onChange={(e) => setMetaField("name", e.target.value)} placeholder="certifications" />
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
    </div>
  );

  const fieldsTab = (
    <div className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-end">
          <ButtonField
            value={t("Add Field")}
            icon={<Plus className="h-4 w-4" />}
            htmlType="button"
            variant="primary"
            onClick={addField}
          />
        </div>

        {fields.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No fields yet. Add at least one field.")}</p>
        ) : (
          <div className="space-y-2">
            {fields.map((f) => (
              <div key={f._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[16px_1fr_1fr_120px_1.5fr_auto]">
                <GripVertical className="hidden h-4 w-4 self-center text-muted md:block" />
                <div>
                  <label className={LABEL}>{t("Label")} *</label>
                  <input className={INPUT} value={f.label ?? ""} onChange={(e) => updateField(f._key, { label: e.target.value })} />
                </div>
                <div>
                  <label className={LABEL}>{t("Key")} *</label>
                  <input className={INPUT} value={f.name ?? ""} onChange={(e) => updateField(f._key, { name: e.target.value })} />
                </div>
                <div>
                  <label className={LABEL}>{t("Type")}</label>
                  <select className={INPUT} value={f.dataType ?? "Text"} onChange={(e) => updateField(f._key, { dataType: e.target.value })}>
                    {typeOptions.map((o) => (
                      <option key={o.id} value={o.id}>{o.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  {f.dataType === "Select" ? (
                    <div className="space-y-1">
                      <label className={LABEL}>{t("Options source")}</label>
                      <div className="flex gap-1.5">
                        {/* `!w-28` — the shared INPUT class carries w-full, which would otherwise win
                            and crush the sibling input/combo to zero width. */}
                        <select
                          className={`${INPUT} !w-28 shrink-0`}
                          value={f.lookupCategory != null ? "lookup" : "static"}
                          onChange={(e) =>
                            e.target.value === "lookup"
                              ? updateField(f._key, { lookupCategory: "", options: undefined })
                              : updateField(f._key, { lookupCategory: undefined })
                          }
                        >
                          <option value="static">{t("Static")}</option>
                          <option value="lookup">{t("Lookup")}</option>
                        </select>
                        {f.lookupCategory != null ? (
                          <select
                            className={INPUT}
                            value={f.lookupCategory}
                            onChange={(e) => updateField(f._key, { lookupCategory: e.target.value })}
                          >
                            <option value="">{t("— pick a lookup category —")}</option>
                            {(lookupCategories ?? []).map((c) => (
                              <option key={c.id} value={c.code}>{c.name}</option>
                            ))}
                          </select>
                        ) : (
                          <input className={INPUT} value={f.options ?? ""} onChange={(e) => updateField(f._key, { options: e.target.value })} placeholder="A,B,C" />
                        )}
                      </div>
                    </div>
                  ) : (
                    <div>
                      <label className={LABEL}>{t("Options (comma-sep)")}</label>
                      <input className={INPUT} value="" disabled placeholder="—" readOnly />
                    </div>
                  )}
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
    </div>
  );

  return (
    <form onSubmit={submit} noValidate className="space-y-4 text-foreground">
      {/* Standard tabbed record editor (same EntityFormTabs as the rest of the app). Both panels
          keepMounted so the schema being drafted survives tab switches. */}
      <EntityFormTabs
        hasId
        tabs={[
          { key: "details", label: "Tab Details", Icon: Settings2, keepMounted: true, content: detailsTab },
          {
            key: "fields", label: "Fields", Icon: ListChecks, keepMounted: true,
            description: t("Define the columns of the custom tab — use Add Field to append rows.") ?? undefined,
            content: fieldsTab,
          },
        ]}
      />

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <ButtonField
          value={isSaving ? t("Saving…") : t("Save Form")}
          icon={<Save className="h-4 w-4" />}
          htmlType="submit"
          variant="primary"
          disabled={isSaving}
        />
      </div>
    </form>
  );
}

export default memo(FormBuilderForm);
