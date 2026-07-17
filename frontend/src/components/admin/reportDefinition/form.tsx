"use client";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save, Layers } from "lucide-react";
import type { ReportDefinitionModel, ReportFieldModel, FormComponentModel } from "@/models";
import { getReport, saveReport } from "@/services/admin/report";
import { reportFieldDataTypeOptions } from "@/constants/reports";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

interface EditableField extends ReportFieldModel {
  _key: number;
}

/** Grouping / pivot configuration (reference GridConfig) edited in the form; serialized to the report's
 * `gridConfig` JSON on save. Enabling it makes the report a PIVOT report — the viewer then shows the
 * top-right "Grouping" button (supportsGrouping) and can group/subtotal the results. */
interface GroupingState {
  enabled: boolean;
  groupBy: string[];
  allowUserCustomize: boolean;
  maxGroupLevels: number;
  showGroupSummary: boolean;
}
const emptyGrouping: GroupingState = { enabled: false, groupBy: [], allowUserCustomize: true, maxGroupLevels: 3, showGroupSummary: true };

/** Parse a report's stored `gridConfig` JSON back into the form's grouping state (defaults on error). */
const parseGrouping = (gridConfig?: string): GroupingState => {
  if (!gridConfig) return emptyGrouping;
  try {
    const gc = JSON.parse(gridConfig);
    return {
      enabled: true,
      groupBy: Array.isArray(gc.groupBy) ? gc.groupBy.filter((s: unknown) => typeof s === "string" && s) : [],
      allowUserCustomize: gc.allowUserCustomize !== false,
      maxGroupLevels: Number(gc.maxGroupLevels) > 0 ? Number(gc.maxGroupLevels) : 3,
      showGroupSummary: gc.showGroupSummary !== false,
    };
  } catch { return emptyGrouping; }
};

/** Every metadata input uses the project's STANDARD form controls (FormUtility → ui/*) with a floating
 * label (the field name acts as the placeholder and rises above the control on focus/value). */
const Field = (component: FormComponentModel) => (
  <FormUtility component={{ layout: "auth", floatingLabel: true, ...component }} />
);

function ReportDefinitionForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<ReportDefinitionModel>({ reportGrouping: "General", isActive: true, sortOrder: 0 });
  const [fields, setFields] = useState<EditableField[]>([]);
  const [outputs, setOutputs] = useState<{ _key: number; field: string; label: string }[]>([]);
  const [grouping, setGrouping] = useState<GroupingState>(emptyGrouping);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  // Data-type options for the parameter "Type" dropdown (labels translated for the standard select).
  const dataTypeData = useMemo(
    () => reportFieldDataTypeOptions.map((o) => ({ id: o.id, name: t(o.name) })),
    [t],
  );

  const { data: record, isLoading } = useQuery({
    queryKey: ["reportDefinition", id],
    queryFn: () => getReport(id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (record) {
      setMeta(record);
      setFields((record.fields ?? []).map((f) => ({ ...f, _key: nextKey() })));
      setOutputs((record.fieldOutputs ?? []).map((o) => ({ _key: nextKey(), field: o.field, label: o.label })));
      setGrouping(parseGrouping(record.gridConfig));
    }
  }, [record]);

  // Output columns are the fields the report may be grouped by (the pivot dimensions).
  const groupableCols = useMemo(
    () => outputs.filter((o) => o.field.trim()).map((o) => ({ field: o.field.trim(), label: o.label.trim() || o.field.trim() })),
    [outputs],
  );
  const toggleGroup = (f: string) =>
    setGrouping((g) => g.groupBy.includes(f)
      ? { ...g, groupBy: g.groupBy.filter((x) => x !== f) }
      : (g.groupBy.length < g.maxGroupLevels ? { ...g, groupBy: [...g.groupBy, f] } : g));
  const moveGroup = (f: string, dir: -1 | 1) =>
    setGrouping((g) => {
      const i = g.groupBy.indexOf(f), j = i + dir;
      if (i < 0 || j < 0 || j >= g.groupBy.length) return g;
      const next = [...g.groupBy];
      [next[i], next[j]] = [next[j], next[i]];
      return { ...g, groupBy: next };
    });

  const setMetaField = (name: keyof ReportDefinitionModel, value: unknown) =>
    setMeta((p) => ({ ...p, [name]: value }));

  const addField = () =>
    setFields((p) => [...p, { _key: nextKey(), field: "", label: "", dataType: "Text" }]);
  const updateField = (key: number, patch: Partial<EditableField>) =>
    setFields((p) => p.map((f) => (f._key === key ? { ...f, ...patch } : f)));
  const removeField = (key: number) => setFields((p) => p.filter((f) => f._key !== key));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    // Grouping/pivot → the report's gridConfig JSON. Only the group-by columns that still exist as
    // output columns are kept. Disabled ⇒ gridConfig cleared (a flat report).
    const validGroupBy = grouping.groupBy.filter((f) => groupableCols.some((c) => c.field === f));
    const gridConfig = grouping.enabled
      ? JSON.stringify({
          mode: validGroupBy.length ? "grouped" : "normal",
          groupBy: validGroupBy,
          allowUserCustomize: grouping.allowUserCustomize,
          maxGroupLevels: grouping.maxGroupLevels,
          showGroupSummary: grouping.showGroupSummary,
        })
      : undefined;
    const payload: ReportDefinitionModel = {
      ...meta,
      gridConfig,
      fields: fields.map(({ _key, ...f }, i) => ({ ...f, fieldOrder: i + 1 })),
      fieldOutputs: outputs.map((o, i) => ({ field: o.field, label: o.label, fieldOrder: i + 1 })),
    };
    const result = await saveReport(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["reportDefinitions"] });
      queryClient.invalidateQueries({ queryKey: ["reportCatalog"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  return (
    <form onSubmit={submit} className="space-y-5 text-foreground">
      {/* Report registry row */}
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Report")}</h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {Field({
            type: "text", name: "reportKey", label: "Report Key", required: true,
            value: meta.reportKey ?? "", placeholder: "e.g. EmployeeDirectory",
            onChange: (e) => setMetaField("reportKey", e.target.value), error: formState?.zodErrors?.reportKey,
          })}
          {Field({
            type: "text", name: "reportName", label: "Report Name", required: true,
            value: meta.reportName ?? "", placeholder: "e.g. Employee Directory",
            onChange: (e) => setMetaField("reportName", e.target.value), error: formState?.zodErrors?.reportName,
          })}
          {Field({
            type: "text", name: "reportGrouping", label: "Category (menu group)",
            value: meta.reportGrouping ?? "", placeholder: "e.g. Personnel",
            onChange: (e) => setMetaField("reportGrouping", e.target.value),
          })}
          {Field({
            type: "text", name: "storedProc", label: "Stored Procedure", required: true,
            value: meta.storedProc ?? "", placeholder: "e.g. Core.hrms_Report_EmployeeDirectory",  // SP name (procedures keep the Core.hrms_ name; only tables were renamed to dbo.hrms*)
            onChange: (e) => setMetaField("storedProc", e.target.value), error: formState?.zodErrors?.storedProc,
          })}
          {Field({
            type: "text", inputType: "number", name: "sortOrder", label: "Sort Order",
            value: String(meta.sortOrder ?? 0), onChange: (e) => setMetaField("sortOrder", e.target.value),
          })}
          {Field({
            type: "checkbox", name: "isActive", label: "Active",
            value: meta.isActive ?? true ? "true" : "", onChange: (e) => setMetaField("isActive", e.target.checked),
          })}
          {Field({
            type: "text", name: "description", label: "Description", colSpan: "full",
            value: meta.description ?? "", onChange: (e) => setMetaField("description", e.target.value),
          })}
        </div>
      </section>

      {/* Parameters (ReportField metadata) */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold">{t("Parameters")}</h3>
            <p className="text-xs text-muted">
              {t("Field = key inside @Criteria. A '#' declares a From/To range (e.g. HireDate#). Dropdown options come from Core.hrms_ReportFieldValues by field name.")}
            </p>
          </div>
          <button type="button" onClick={addField} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
            <Plus className="h-3.5 w-3.5" /> {t("Add Parameter")}
          </button>
        </div>

        {fields.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No parameters — the report runs without filters.")}</p>
        ) : (
          <div className="space-y-2">
            {fields.map((f) => (
              <div key={f._key} className="grid grid-cols-1 items-end gap-3 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_1fr_190px_170px_auto]">
                {Field({
                  type: "text", name: `param-field-${f._key}`, label: "Field", required: true,
                  value: f.field ?? "", placeholder: "e.g. OrganizationUnitId",
                  onChange: (e) => updateField(f._key, { field: e.target.value }),
                })}
                {Field({
                  type: "text", name: `param-label-${f._key}`, label: "Label",
                  value: f.label ?? "", placeholder: "e.g. Organization Unit",
                  onChange: (e) => updateField(f._key, { label: e.target.value }),
                })}
                {Field({
                  type: "select", name: `param-type-${f._key}`, label: "Type",
                  value: f.dataType ?? "Text", data: dataTypeData,
                  onChange: (e) => updateField(f._key, { dataType: e.target.value }),
                })}
                {Field({
                  type: "text", name: `param-dep-${f._key}`, label: "Depends On (field)",
                  value: f.dependencyField ?? "", placeholder: t("cascades from…") ?? "",
                  onChange: (e) => updateField(f._key, { dependencyField: e.target.value }),
                })}
                <div className="flex items-center pb-2">
                  <button type="button" onClick={() => removeField(f._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Output columns offered in the viewer's column chooser (reference ReportFieldOutput) */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold">{t("Output Columns")}</h3>
            <p className="text-xs text-muted">{t("Optional: the columns users may pick. Field names must match the stored procedure's column metadata. Empty = no chooser (SP returns its full set).")}</p>
          </div>
          <button type="button" onClick={() => setOutputs((pv) => [...pv, { _key: nextKey(), field: "", label: "" }])} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
            <Plus className="h-3.5 w-3.5" /> {t("Add Column")}
          </button>
        </div>
        {outputs.length === 0 ? (
          <p className="py-4 text-center text-sm text-muted">{t("No selectable columns configured.")}</p>
        ) : (
          <div className="space-y-2">
            {outputs.map((o) => (
              <div key={o._key} className="grid grid-cols-1 items-end gap-3 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_1fr_auto]">
                {Field({
                  type: "text", name: `out-field-${o._key}`, label: "Field", required: true,
                  value: o.field, onChange: (e) => setOutputs((pv) => pv.map((x) => x._key === o._key ? { ...x, field: e.target.value } : x)),
                })}
                {Field({
                  type: "text", name: `out-label-${o._key}`, label: "Label",
                  value: o.label, onChange: (e) => setOutputs((pv) => pv.map((x) => x._key === o._key ? { ...x, label: e.target.value } : x)),
                })}
                <div className="flex items-center pb-2">
                  <button type="button" onClick={() => setOutputs((pv) => pv.filter((x) => x._key !== o._key))} className="rounded p-1 text-error hover:bg-error/10">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Grouping / pivot configuration (reference GridConfig). Enabling makes this a PIVOT report. */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-start justify-between gap-3">
          <div>
            <h3 className="flex items-center gap-1.5 text-sm font-semibold"><Layers className="h-4 w-4 text-primary" /> {t("Grouping (Pivot)")}</h3>
            <p className="text-xs text-muted">
              {t("Turn this report into a pivot/grouping report. The viewer then shows a top-right “Grouping” button and can group rows and show per-group subtotals.")}
            </p>
          </div>
          <label className="flex shrink-0 items-center gap-2 text-sm font-medium">
            <input type="checkbox" className="accent-primary" checked={grouping.enabled}
              onChange={(e) => setGrouping((g) => ({ ...g, enabled: e.target.checked }))} />
            {t("Enable grouping")}
          </label>
        </div>

        {grouping.enabled && (
          <div className="space-y-4 border-t border-border pt-3">
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              {Field({
                type: "checkbox", name: "grp-customize", label: "Let users choose groups at run time",
                value: grouping.allowUserCustomize ? "true" : "",
                onChange: (e) => setGrouping((g) => ({ ...g, allowUserCustomize: e.target.checked })),
              })}
              {Field({
                type: "checkbox", name: "grp-summary", label: "Show per-group subtotals",
                value: grouping.showGroupSummary ? "true" : "",
                onChange: (e) => setGrouping((g) => ({ ...g, showGroupSummary: e.target.checked })),
              })}
              {Field({
                type: "text", inputType: "number", name: "grp-maxlevels", label: "Max grouping levels",
                value: String(grouping.maxGroupLevels),
                onChange: (e) => setGrouping((g) => ({ ...g, maxGroupLevels: Math.max(1, Math.min(6, Number(e.target.value) || 1)) })),
              })}
            </div>

            <div>
              <p className="mb-1.5 text-xs font-medium text-muted">
                {t("Default grouping — tick output columns and order them (level 1 = outermost). Users can still re-group at run time when the option above is on.")}
              </p>
              {groupableCols.length === 0 ? (
                <p className="rounded-md border border-dashed border-border/70 bg-secondary/20 py-4 text-center text-xs text-muted">
                  {t("Add Output Columns above first — those are the columns this report can be grouped by.")}
                </p>
              ) : (
                <div className="space-y-1.5">
                  {groupableCols.map((c) => {
                    const lvl = grouping.groupBy.indexOf(c.field);
                    const on = lvl >= 0;
                    const atCap = !on && grouping.groupBy.length >= grouping.maxGroupLevels;
                    return (
                      <div key={c.field}
                        className={`flex items-center gap-2.5 rounded-md border px-2.5 py-1.5 transition-colors ${
                          on ? "border-primary/40 bg-primary/5" : "border-border/70 bg-secondary/20"}`}>
                        <label className={`flex min-w-0 flex-1 items-center gap-2 text-sm ${atCap ? "opacity-40" : ""}`}>
                          <input type="checkbox" className="accent-primary" checked={on} disabled={atCap}
                            onChange={() => toggleGroup(c.field)} />
                          <span className="truncate">{c.label}</span>
                          <span className="truncate text-xs text-muted">({c.field})</span>
                        </label>
                        {on && (
                          <div className="flex items-center gap-2">
                            <span className="rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">{t("Level")} {lvl + 1}</span>
                            <div className="flex items-center overflow-hidden rounded-md border border-border">
                              <button type="button" disabled={lvl === 0} onClick={() => moveGroup(c.field, -1)}
                                className="px-2 py-1 text-xs leading-none text-muted hover:bg-secondary hover:text-primary disabled:opacity-30">▲</button>
                              <button type="button" disabled={lvl === grouping.groupBy.length - 1} onClick={() => moveGroup(c.field, 1)}
                                className="border-l border-border px-2 py-1 text-xs leading-none text-muted hover:bg-secondary hover:text-primary disabled:opacity-30">▼</button>
                            </div>
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
              {!grouping.allowUserCustomize && grouping.groupBy.length === 0 && (
                <p className="mt-1.5 text-xs text-warning">
                  {t("Pick at least one default grouping column, or enable “Let users choose groups at run time” — otherwise the report has no grouping.")}
                </p>
              )}
            </div>
          </div>
        )}
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <button type="submit" disabled={isSaving} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
          <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Report")}
        </button>
      </div>
    </form>
  );
}

export default memo(ReportDefinitionForm);
