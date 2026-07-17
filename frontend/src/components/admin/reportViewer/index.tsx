"use client";
import { memo, useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  BarChart3, FileBarChart, History as HistoryIcon,
  CalendarClock, ListChecks, Plus, Shield, Trash2, X, Play, Pencil, Power, UserRound, GripVertical, Layers,
} from "lucide-react";
import InputField from "@/components/ui/inputField";
import CheckBoxField from "@/components/ui/checkBoxField";
import { FloatingLabel } from "@/components/ui/floatingLabel";
import { FORM_INPUT_CLASS } from "@/components/ui/fieldStyles";
import {
  getReportCatalog, getReportSchema, getReportFieldValues, saveReportFilter, getReportFilter,
  deleteReportFilter, getReportHistory, saveReportSchedule, emailReport, getReportSchedules,
  deleteReportSchedule, getReportScheduleDetail, setReportScheduleEnabled, runReportScheduleNow,
  setReportRestrictions, getReport,
  type ReportRunModel, type ReportScheduleItem, type ScheduleOutputFieldInput,
} from "@/services/admin/report";
import getAllRole from "@/services/admin/role/getAll";
import getAllUser from "@/services/admin/user/getAll";
import type { ReportCatalogItemModel, ReportSchemaFieldModel, ReportLookupOptionModel, FormComponentModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import Modal from "@/components/common/modal";
import DialogModal from "@/components/common/dialog";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";
import EmptyState from "@/components/common/emptyState";
import InventoryLayout from "@/components/common/inventoryLayout";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import { parameterInitialData } from "@/constants/initialization";
import ReportCatalogTree from "./reportCatalogTree";
import ReportCriteria from "./reportCriteria";

const INPUT =
  "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

/** '#' range convention (ported): "HireDate#" expands to HireDate1 (From) / HireDate2 (To). */
const rangeNames = (field: string): [string, string] => [field.replace("#", "1"), field.replace("#", "2")];

/** Report lookup options → the {id,name} shape the standard select/lookup components expect. */
const toOptionData = (opts: ReportLookupOptionModel[] | undefined) =>
  (opts ?? []).map((o) => ({ id: o.value, name: o.label }));

/**
 * A lookup parameter rendered with the project's STANDARD components: single = DropDownField
 * (searchable select), multi = CheckboxListField (multi-select lookup). Options come preloaded on the
 * schema field, or are fetched on the fly for a cascading (dependency) field when its parent changes.
 */
function LookupField({ reportKey, field, values, setValue, fields, multiple }: {
  reportKey: string;
  field: ReportSchemaFieldModel;
  values: Record<string, string>;
  setValue: (name: string, v: string, fields?: ReportSchemaFieldModel[]) => void;
  fields?: ReportSchemaFieldModel[];
  multiple?: boolean;
}) {
  const { t } = useTranslation();
  const dep = field.dependencyField;
  const parentValue = dep ? values[dep] ?? "" : "";
  const { data: fetched } = useQuery({
    queryKey: ["reportFieldValues", reportKey, field.field, parentValue],
    queryFn: () => getReportFieldValues(reportKey, field.field, parentValue || undefined),
    enabled: !!dep,
  });
  const data = toOptionData(dep ? fetched : field.options);
  const value = values[field.field] ?? "";

  if (multiple) {
    // Standard MULTI-SELECT combobox (searchable dropdown + chips) — replaces the inline checkbox list.
    const component: FormComponentModel = {
      name: field.field, label: field.label, type: "multiSelectField", floatingLabel: true,
      value, data,
      onSelect: (_n: string, commaIds: unknown) => setValue(field.field, String(commaIds ?? ""), fields),
    };
    return <FormUtility component={component} />;
  }

  // Prepend an "All" entry so a single-select filter can be cleared (the native <select> did this).
  const singleData = [{ id: "", name: t("All") }, ...data];
  const component: FormComponentModel = {
    name: field.field, label: field.label, type: "dropDown", floatingLabel: true,
    value, displayValue: value ? data.find((d) => String(d.id) === String(value))?.name ?? "" : "",
    data: singleData,
    onSelect: (_n: string, item: { id: string | number }) => setValue(field.field, String(item.id), fields),
  };
  return <FormUtility component={component} />;
}

/** Legacy three-tab workspace: Report / Schedule / History. */
const TABS = [
  { key: "report", label: "Report", Icon: FileBarChart },
  { key: "schedule", label: "Schedule", Icon: CalendarClock },
  { key: "history", label: "History", Icon: HistoryIcon },
] as const;

// Reference cadence model. Weekly bitmask: Sun=64 … Sat=1 (see GenerateCronExpression / DecodeWeeklyDays).
const WEEKDAYS: { bit: number; label: string }[] = [
  { bit: 64, label: "Sun" }, { bit: 32, label: "Mon" }, { bit: 16, label: "Tue" },
  { bit: 8, label: "Wed" }, { bit: 4, label: "Thu" }, { bit: 2, label: "Fri" }, { bit: 1, label: "Sat" },
];
const FREQUENCIES = ["Daily", "Weekly", "Monthly", "Quarterly", "Yearly"];
const to24 = (h: number, ampm: string) => (ampm === "PM" ? (h % 12) + 12 : h % 12);
const from24 = (h24: number) => ({ hour: ((h24 + 11) % 12) + 1, ampm: h24 >= 12 ? "PM" : "AM" });
const fmtTime = (m: number) => `${String(Math.floor(m / 60)).padStart(2, "0")}:${String(m % 60).padStart(2, "0")}`;

const emptyScheduleForm = {
  frequency: "Daily", hour: 8, ampm: "AM", startDate: "", weekly: new Set<number>(),
  hideRecipients: false, subject: "", body: "",
  userIds: new Set<string>(), roleIds: new Set<string>(),
};
type ScheduleForm = typeof emptyScheduleForm;

// Ad-hoc e-mail form (reference _SendReportByEmail.cshtml).
const emptyEmailForm = {
  userIds: new Set<string>(), roleIds: new Set<string>(), emails: "",
  isCc: true, hide: false, subject: "", body: "", outputFormat: 1,
};
type EmailForm = typeof emptyEmailForm;

// One row of the pre-generation column customizer (reference _ReportFieldsEnableDisable.cshtml):
// display order = array order; SortOrder 0–9 = ORDER BY priority; only shown columns are generated.
type FieldRow = { field: string; label: string; sortOrder: number; show: boolean };
type OutputSel = { field: string; label?: string; sortOrder?: number };

// ---- Pure field-row transforms (shared by the Report-tab Fields popup + the schedule Fields tab) ----
const colsToRows = (cols: { field: string; label: string }[] | undefined): FieldRow[] =>
  (cols ?? []).map((c) => ({ field: c.field, label: c.label, sortOrder: 0, show: true }));

const rowsToOutputFields = (rows: FieldRow[]) =>
  rows.filter((r) => r.show).map((r, i) => ({ field: r.field, label: r.label, order: i + 1, sortOrder: r.sortOrder }));

/** Rebuild the customizer rows from a saved selection (shown-in-order first, then the rest hidden). */
const hydrateRows = (cols: { field: string; label: string }[] | undefined, selected: OutputSel[]): FieldRow[] => {
  const list = cols ?? [];
  const chosen = new Set(selected.map((s) => s.field));
  const shown = selected
    .map((s) => { const c = list.find((x) => x.field === s.field); return c ? { field: s.field, label: s.label ?? c.label, sortOrder: s.sortOrder ?? 0, show: true } : null; })
    .filter((r): r is FieldRow => r !== null);
  const hidden = list.filter((c) => !chosen.has(c.field)).map((c) => ({ field: c.field, label: c.label, sortOrder: 0, show: false }));
  return [...shown, ...hidden];
};

// Toggle Show: unchecking drops sort priority and sinks the row to the bottom (reference report.js).
const toggleShowRows = (rows: FieldRow[], field: string): FieldRow[] => {
  const i = rows.findIndex((r) => r.field === field);
  if (i < 0) return rows;
  const row = { ...rows[i] };
  const next = [...rows];
  if (row.show) { row.show = false; row.sortOrder = 0; next.splice(i, 1); next.push(row); }
  else { row.show = true; row.sortOrder = 0; next[i] = row; }
  return next;
};
const bumpSortRows = (rows: FieldRow[], field: string, delta: number): FieldRow[] =>
  rows.map((r) => (r.field === field && r.show ? { ...r, sortOrder: Math.max(0, Math.min(9, r.sortOrder + delta)) } : r));
const setLabelRows = (rows: FieldRow[], field: string, label: string): FieldRow[] =>
  rows.map((r) => (r.field === field ? { ...r, label } : r));
const reorderRows = (rows: FieldRow[], dragField: string | null, target: string): FieldRow[] => {
  if (!dragField || dragField === target) return rows;
  const from = rows.findIndex((r) => r.field === dragField);
  const to = rows.findIndex((r) => r.field === target);
  if (from < 0 || to < 0) return rows;
  const next = [...rows];
  const [moved] = next.splice(from, 1);
  next.splice(to, 0, moved);
  return next;
};

/**
 * The column customizer table (reference _ReportFieldsEnableDisable.cshtml + report.js):
 * [☰ drag | hidden Field | Label (editable) | Sorting −/+ | Show]. Shared by the Report-tab "Report
 * Fields" popup and the schedule popup's "Fields" tab.
 */
function FieldsTable({ rows, setRows, dragField, setDragField }: {
  rows: FieldRow[];
  setRows: (updater: (r: FieldRow[]) => FieldRow[]) => void;
  dragField: string | null;
  setDragField: (f: string | null) => void;
}) {
  const { t } = useTranslation();
  if (rows.length === 0)
    return <p className="py-8 text-center text-sm text-muted">{t("This report has no configurable output columns — all of the stored procedure's columns are shown.")}</p>;
  return (
    <div className="space-y-2.5">
      <p className="text-xs text-muted">
        {t("Drag rows to reorder columns. Set a sort priority (1 = sorted first). Untick to hide a column.")}
      </p>
      <div className="max-h-[48vh] space-y-1.5 overflow-y-auto pr-0.5">
        {rows.map((r) => (
          <div key={r.field}
            draggable={r.show}
            onDragStart={() => setDragField(r.field)}
            onDragOver={(e) => e.preventDefault()}
            onDrop={() => { setRows((p) => reorderRows(p, dragField, r.field)); setDragField(null); }}
            onDragEnd={() => setDragField(null)}
            className={`flex items-center gap-2.5 rounded-lg border px-2.5 py-1.5 transition-colors ${
              r.show ? "border-border bg-card hover:border-primary/40" : "border-dashed border-border/70 bg-secondary/20"} ${
              dragField === r.field ? "opacity-50" : ""}`}>
            <span className={`shrink-0 ${r.show ? "cursor-grab text-muted hover:text-foreground" : "cursor-not-allowed text-muted/30"}`} title={t("Reorder") ?? "Reorder"}>
              <GripVertical size={16} />
            </span>
            <div className="min-w-0 flex-1">
              {/* Floating label: the column's field name floats above as the user edits its display label. */}
              <InputField floatingLabel name={`fld-${r.field}`} type="text" label={r.field} value={r.label} disabled={!r.show}
                onChange={(e) => setRows((p) => setLabelRows(p, r.field, e.target.value))} />
            </div>
            <div className="flex shrink-0 items-center overflow-hidden rounded-md border border-border">
              <button type="button" disabled={!r.show} onClick={() => setRows((p) => bumpSortRows(p, r.field, -1))}
                className="px-2 py-1.5 text-sm font-bold leading-none text-muted transition-colors hover:bg-secondary hover:text-primary disabled:opacity-30" title={t("Lower priority") ?? ""}>−</button>
              <span className="w-6 border-x border-border py-1.5 text-center text-xs tabular-nums text-foreground">{r.show && r.sortOrder ? r.sortOrder : "–"}</span>
              <button type="button" disabled={!r.show} onClick={() => setRows((p) => bumpSortRows(p, r.field, 1))}
                className="px-2 py-1.5 text-sm font-bold leading-none text-muted transition-colors hover:bg-secondary hover:text-primary disabled:opacity-30" title={t("Higher priority") ?? ""}>+</button>
            </div>
            <div className="shrink-0">
              <CheckBoxField name={`show-${r.field}`} type="checkbox" label="Show" value={r.show ? "true" : ""}
                onChange={() => setRows((p) => toggleShowRows(p, r.field))} />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

const SCHED_TABS = [
  { key: "criteria", label: "Criteria" },
  { key: "grouping", label: "Grouping" },
  { key: "fields", label: "Fields" },
  { key: "schedule", label: "Schedule" },
] as const;

/**
 * Pivot / grouping customizer (reference _ReportGroupingCustomize.cshtml): tick the columns to group
 * the report by, and order them into grouping levels (1 = outermost). Built from the STANDARD
 * CheckBoxField. Shared by the Report-tab "Grouping" popup and the schedule popup's Grouping tab.
 */
function GroupingCustomizer({ fields, maxLevels, value, onChange }: {
  fields: { field: string; label: string }[];
  maxLevels: number;
  value: string[];
  onChange: (next: string[]) => void;
}) {
  const { t } = useTranslation();
  const toggle = (f: string) => {
    if (value.includes(f)) onChange(value.filter((x) => x !== f));
    else if (value.length < maxLevels) onChange([...value, f]);
  };
  const move = (f: string, dir: -1 | 1) => {
    const i = value.indexOf(f);
    const j = i + dir;
    if (i < 0 || j < 0 || j >= value.length) return;
    const next = [...value];
    [next[i], next[j]] = [next[j], next[i]];
    onChange(next);
  };
  if (fields.length === 0)
    return <p className="py-8 text-center text-sm text-muted">{t("This report has no groupable columns.")}</p>;
  return (
    <div className="space-y-2.5">
      <p className="text-xs text-muted">
        {t("Tick up to {{n}} columns to group the report; use the arrows to set the grouping level (1 = outermost).", { n: maxLevels })}
      </p>
      <div className="max-h-[46vh] space-y-1.5 overflow-y-auto pr-0.5">
        {fields.map((c) => {
          const lvl = value.indexOf(c.field);
          const on = lvl >= 0;
          return (
            <div key={c.field}
              className={`flex items-center gap-2.5 rounded-lg border px-2.5 py-2 transition-colors ${
                on ? "border-primary/40 bg-primary/5" : "border-border bg-card hover:border-primary/40"}`}>
              <CheckBoxField name={`grp-${c.field}`} type="checkbox" label={c.label} value={on ? "true" : ""}
                onChange={() => toggle(c.field)} disabled={!on && value.length >= maxLevels} />
              {on && (
                <div className="ml-auto flex items-center gap-2">
                  <span className="rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary">{t("Level")} {lvl + 1}</span>
                  <div className="flex items-center overflow-hidden rounded-md border border-border">
                    <button type="button" disabled={lvl === 0} onClick={() => move(c.field, -1)}
                      className="px-2 py-1 text-xs leading-none text-muted transition-colors hover:bg-secondary hover:text-primary disabled:opacity-30">▲</button>
                    <button type="button" disabled={lvl === value.length - 1} onClick={() => move(c.field, 1)}
                      className="border-l border-border px-2 py-1 text-xs leading-none text-muted transition-colors hover:bg-secondary hover:text-primary disabled:opacity-30">▼</button>
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

/**
 * Standard multi-select combobox for entities (Users / Roles): a searchable DropDownField to add +
 * removable chips for the current selection — the app's convention (clearanceDepartment / workflow
 * approver picker). Shared by the Email popup and the Schedule form.
 */
function ChipMultiSelect({ label, addLabel, options, selected, onChange, icon }: {
  label: string;
  addLabel: string;
  options: { id: string; name: string }[];
  selected: Set<string>;
  onChange: (next: Set<string>) => void;
  icon?: ReactNode;
}) {
  const { t } = useTranslation();
  const nameOf = (id: string) => options.find((o) => o.id === id)?.name ?? id;
  const available = options.filter((o) => !selected.has(o.id));
  const add = (id: string) => { if (!id) return; const n = new Set(selected); n.add(id); onChange(n); };
  const remove = (id: string) => { const n = new Set(selected); n.delete(id); onChange(n); };
  return (
    <div>
      <FormUtility component={{
        name: `chip-${label}`, label, type: "dropDown", floatingLabel: true,
        value: "", displayValue: "", placeholder: addLabel, data: available,
        onSelect: (_n: string, item: { id: string | number }) => add(String(item.id)),
      }} />
      {selected.size > 0 && (
        <div className="mt-1.5 flex flex-wrap gap-1.5">
          {[...selected].map((id) => (
            <span key={id} className="inline-flex items-center gap-1 rounded-full border border-primary/40 bg-primary/10 px-2 py-0.5 text-xs text-primary">
              {icon}<span className="max-w-[160px] truncate">{nameOf(id)}</span>
              <button type="button" onClick={() => remove(id)} className="ml-0.5 rounded-full hover:opacity-70" aria-label={t("Remove") ?? "Remove"}><X size={11} /></button>
            </span>
          ))}
        </div>
      )}
    </div>
  );
}

function ReportViewer() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [tab, setTab] = useState<"report" | "schedule" | "history">("report");
  const [selected, setSelected] = useState<ReportCatalogItemModel | null>(null);
  const [values, setValues] = useState<Record<string, string>>({});
  // Pre-generation column customizer: fieldRows = COMMITTED selection (used by generate/email/schedule);
  // draftRows = the working copy edited inside the "Report Fields" popup (Close commits, Cancel discards).
  const [fieldRows, setFieldRows] = useState<FieldRow[]>([]);
  const [draftRows, setDraftRows] = useState<FieldRow[]>([]);
  const [dragField, setDragField] = useState<string | null>(null);
  // Pivot / grouping (reference GridConfig): effective group-by columns + the popup's working draft.
  const [groupBy, setGroupBy] = useState<string[]>([]);
  const [groupDraft, setGroupDraft] = useState<string[]>([]);

  // Dialogs: report-fields / grouping / schedule-add / email / save-filter / restricted-roles.
  const [dialog, setDialog] = useState<"fields" | "grouping" | "schedule" | "email" | "save" | "restrict" | null>(null);
  const [dlgName, setDlgName] = useState("");
  const [dlgRoles, setDlgRoles] = useState<Set<string>>(new Set());
  const [dlgBusy, setDlgBusy] = useState(false);
  const [dlgMsg, setDlgMsg] = useState<string | null>(null);
  const [historySearch, setHistorySearch] = useState("");
  // Schedule cadence form (reference _ScheduleForm.cshtml) + e-mail form (_SendReportByEmail.cshtml).
  const [editId, setEditId] = useState<string | undefined>(undefined);
  const [sForm, setSForm] = useState<ScheduleForm>(emptyScheduleForm);
  const [eForm, setEForm] = useState<EmailForm>(emptyEmailForm);
  // The four-tab schedule popup (reference _ScheduleReportTop.cshtml): Criteria / Grouping / Fields / Schedule.
  const [schedTab, setSchedTab] = useState<"criteria" | "grouping" | "fields" | "schedule">("criteria");
  const [schedReportKey, setSchedReportKey] = useState("");   // the chosen Template (Criteria tab)
  const [schedValues, setSchedValues] = useState<Record<string, string>>({}); // its filter criteria
  const [schedRows, setSchedRows] = useState<FieldRow[]>([]); // its column customization (Fields tab)
  const [schedGroupBy, setSchedGroupBy] = useState<string[]>([]); // its grouping (Grouping tab)
  const [schedFormat, setSchedFormat] = useState(1);          // output format (1=CSV, 0=Tab)
  // Standard destructive-confirm popup (project DialogModal) — replaces native window.confirm.
  const [confirm, setConfirm] = useState<{ message: string; onOk: () => void } | null>(null);
  const askConfirm = useCallback((message: string, onOk: () => void) => setConfirm({ message, onOk }), []);

  const { data: catalog, isLoading: catalogLoading } = useQuery({
    queryKey: ["reportCatalog"], queryFn: getReportCatalog,
  });
  const { data: schema, isFetching: schemaLoading } = useQuery({
    queryKey: ["reportSchema", selected?.reportKey],
    queryFn: () => getReportSchema(selected!.reportKey),
    enabled: !!selected,
  });
  // The Criteria-tab template's schema (its filter fields); shares the reportSchema cache.
  const { data: schedSchema, isFetching: schedSchemaLoading } = useQuery({
    queryKey: ["reportSchema", schedReportKey],
    queryFn: () => getReportSchema(schedReportKey),
    enabled: dialog === "schedule" && !!schedReportKey,
  });
  // Dynamic relative-date catalog (reference _x_ReportFieldValues '@DynamicDate') — date criteria on a
  // schedule save a token (e.g. "StartOfMonth"), resolved to a real date by the engine at run time.
  const { data: dynamicDateOptions } = useQuery({
    queryKey: ["reportFieldValues", schedReportKey, "@DynamicDate"],
    queryFn: () => getReportFieldValues(schedReportKey, "@DynamicDate"),
    enabled: dialog === "schedule" && !!schedReportKey,
  });
  const { data: schedules } = useQuery({
    queryKey: ["reportSchedules", selected?.reportKey],
    queryFn: () => getReportSchedules(selected!.reportKey),
    enabled: !!selected && tab === "schedule",
  });
  const { data: history } = useQuery({
    queryKey: ["reportHistory", selected?.reportKey, tab],
    queryFn: () => getReportHistory(selected?.reportKey),
    enabled: tab === "history",
  });
  const { data: roles } = useQuery({
    queryKey: ["roles", "reportRestrict"],
    queryFn: () => getAllRole({ ...parameterInitialData, take: 100 }),
    enabled: dialog === "restrict",
  });
  // Recipient pickers (users + roles) shared by the schedule + e-mail forms.
  const pickersOpen = dialog === "schedule" || dialog === "email";
  const { data: schedUsers } = useQuery({
    queryKey: ["users", "reportRecipients"],
    queryFn: () => getAllUser({ ...parameterInitialData, take: 500 }),
    enabled: pickersOpen,
  });
  const { data: schedRoles } = useQuery({
    queryKey: ["roles", "reportRecipients"],
    queryFn: () => getAllRole({ ...parameterInitialData, take: 500 }),
    enabled: pickersOpen,
  });

  // Derived option lists — memoized so typing in any field doesn't re-map the (up to 500) users/roles
  // or re-flatten the catalog on every keystroke.
  const allReports = useMemo(
    () => (catalog ?? []).flatMap((g) => g.reports).map((r) => ({ id: r.reportKey, name: r.reportName })),
    [catalog],
  );
  const userOptions = useMemo(
    () => (schedUsers?.data ?? []).map((u) => ({ id: u.id!, name: u.fullName ?? u.userName ?? u.id! })),
    [schedUsers],
  );
  const roleOptions = useMemo(
    () => (schedRoles?.data ?? []).map((r) => ({ id: r.id!, name: r.name ?? r.id! })),
    [schedRoles],
  );
  // Roles for the "Restricted Roles" popup (its own query, enabled only for that dialog).
  const restrictRoleOptions = useMemo(
    () => (roles?.data ?? []).map((r) => ({ id: r.id!, name: r.name ?? r.id! })),
    [roles],
  );

  useEffect(() => {
    setValues({});
  }, [selected?.reportKey]);

  useEffect(() => {
    setFieldRows((schema?.outputColumns ?? []).map((c) => ({ field: c.field, label: c.label, sortOrder: 0, show: true })));
    setGroupBy(schema?.grouping?.groupBy ?? []); // seed the effective grouping from the report's default
  }, [schema?.reportKey]);

  // ---- Pre-generation column customizer (reference _ReportFieldsEnableDisable.cshtml + report.js) ----
  /** The chosen columns → the structured OutputFields payload (visible rows, in display order). */
  const buildOutputFields = () => rowsToOutputFields(fieldRows);
  const defaultRows = (): FieldRow[] => colsToRows(schema?.outputColumns);

  /** Open the "Report Fields" popup (reference the Fields link → popup-reportFieldEnableDisable). */
  const openFields = () => { setDraftRows(fieldRows.length ? fieldRows.map((r) => ({ ...r })) : defaultRows()); setDialog("fields"); };
  const resetFields = () => setDraftRows(defaultRows());          // reference Reset → reload the field list
  const commitFields = () => { setFieldRows(draftRows); setDialog(null); }; // reference Close → save the config

  // ---- Pivot / grouping popup (reference _ReportGroupingCustomize.cshtml) ----
  const openGrouping = () => { setGroupDraft(groupBy); setDialog("grouping"); };
  const resetGrouping = () => setGroupDraft(schema?.grouping?.groupBy ?? []); // reference Reset → the default grouping
  const commitGrouping = () => { setGroupBy(groupDraft); setDialog(null); };  // reference Close → apply the grouping

  // Value setters clear any dependent (cascading) child when their parent changes.
  const applyValue = (prev: Record<string, string>, name: string, v: string, fields?: ReportSchemaFieldModel[]) => {
    const next = { ...prev, [name]: v };
    for (const f of fields ?? []) if (f.dependencyField === name) delete next[f.field];
    return next;
  };
  const setValue = useCallback((name: string, v: string, fields?: ReportSchemaFieldModel[]) =>
    setValues((p) => applyValue(p, name, v, fields)), []);
  const setSchedValue = useCallback((name: string, v: string, fields?: ReportSchemaFieldModel[]) =>
    setSchedValues((p) => applyValue(p, name, v, fields)), []);

  /** Load a template's schema (cached) and initialise the schedule's criteria + column rows + grouping.
   * A schedule persists its grouping as a reserved "__groupBy" criteria value (comma list). */
  const initTemplate = async (reportKey: string, opts?: { values?: Record<string, string>; outputs?: OutputSel[] }) => {
    setSchedReportKey(reportKey);
    const rawValues = { ...(opts?.values ?? {}) };
    const savedGroup = (rawValues["__groupBy"] ?? "").split(",").map((s) => s.trim()).filter(Boolean);
    delete rawValues["__groupBy"];
    setSchedValues(rawValues);
    const sc = await queryClient.fetchQuery({ queryKey: ["reportSchema", reportKey], queryFn: () => getReportSchema(reportKey) });
    setSchedRows(opts?.outputs ? hydrateRows(sc.outputColumns, opts.outputs) : colsToRows(sc.outputColumns));
    setSchedGroupBy(savedGroup.length ? savedGroup : (opts ? [] : sc.grouping?.groupBy ?? []));
  };

  const loadSavedFilter = async (filterId: string, report: ReportCatalogItemModel) => {
    setSelected(report);
    setTab("report");
    const f = await getReportFilter(filterId);
    setValues(Object.fromEntries(Object.entries(f.values ?? {}).map(([k, v]) => [k, v ?? ""])));
    if (f.outputFields && f.outputFields.length > 0) {
      const sc = await queryClient.fetchQuery({ queryKey: ["reportSchema", report.reportKey], queryFn: () => getReportSchema(report.reportKey) });
      setFieldRows(hydrateRows(sc.outputColumns, f.outputFields.map((field) => ({ field }))));
    }
  };

  const removeSavedFilter = (filterId: string) =>
    askConfirm(t("Delete this saved filter?") ?? "Delete this saved filter?", async () => {
      await deleteReportFilter(filterId);
      queryClient.invalidateQueries({ queryKey: ["reportCatalog"] });
      toast.success(t("Saved filter deleted."));
    });

  // Reference behavior: the generated report opens in a NEW BROWSER TAB (/reportResult).
  const generate = () => {
    if (!selected) return;
    const hasChooser = fieldRows.length > 0;
    const grouped = groupBy.length > 0;
    // PIVOT: the grouping SP reads the chosen levels (+ whether to compute subtotals) from reserved
    // criteria values, so it can fetch & group the data server-side. Flat reports send plain values.
    const showSummary = grouped && !!schema?.grouping?.showGroupSummary;
    const runValues = grouped
      ? { ...values, __groupBy: groupBy.join(","), ...(showSummary ? { __showSummary: "true" } : {}) }
      : values;
    sessionStorage.setItem("reportRun", JSON.stringify({
      reportKey: selected.reportKey, values: runValues, outputFields: hasChooser ? buildOutputFields() : undefined,
      groupBy: grouped ? groupBy : undefined, // pivot: the effective group-by columns (client render)
    }));
    window.open("/reportResult", "_blank");
  };

  const openRestrict = async () => {
    if (!selected) return;
    setDlgMsg(null);
    try {
      const full = await getReport(selected.id);
      setDlgRoles(new Set(((full as { roleIds?: string[] }).roleIds) ?? []));
    } catch { setDlgRoles(new Set()); }
    setDialog("restrict");
  };

  // Open the four-tab schedule popup: fresh (Add) or hydrated from an existing schedule (Edit).
  const openSchedule = async (id?: string) => {
    setDlgMsg(null); setEditId(id); setSchedTab("criteria"); setDialog("schedule");
    if (!id) {
      setDlgName("");
      setSForm({ ...emptyScheduleForm, weekly: new Set(), userIds: new Set(), roleIds: new Set() });
      setSchedFormat(1);
      // Default the Template to the report the Schedule tab is under; load its criteria fields + columns.
      await initTemplate(selected?.reportKey ?? "");
      return;
    }
    try {
      const d = await getReportScheduleDetail(id);
      const { hour, ampm } = from24(d.hour24);
      setDlgName(d.name);
      setSchedFormat(d.outputFormat ?? 1);
      setSForm({
        frequency: d.frequency, hour, ampm, startDate: d.scheduleStartDate ?? "",
        weekly: new Set(WEEKDAYS.filter((w) => (d.frequencyWeekly & w.bit) !== 0).map((w) => w.bit)),
        hideRecipients: d.isHideRecipients, subject: d.mailSubject ?? "", body: d.mailBody ?? "",
        userIds: new Set(d.recipientUserIds), roleIds: new Set(d.recipientRoleIds),
      });
      // Restore the schedule's own Template + criteria + column customization for editing.
      await initTemplate(d.reportKey, {
        values: Object.fromEntries(Object.entries(d.values ?? {}).map(([k, v]) => [k, v ?? ""])),
        outputs: (d.outputFields ?? []).map((o) => ({ field: o.field, label: o.label, sortOrder: o.sortOrder })),
      });
    } catch (e) { setDlgMsg((e as Error).message); }
  };

  const openEmail = () => {
    setDlgMsg(null);
    setEForm({ ...emptyEmailForm, userIds: new Set(), roleIds: new Set(),
      subject: `${t("Report")}: ${selected?.reportName ?? ""}`, body: `${t("Please find the attached report")}: ${selected?.reportName ?? ""}.` });
    setDialog("email");
  };

  const toggleSchedule = async (r: ReportScheduleItem) => {
    await setReportScheduleEnabled(r.id, !r.isActive);
    queryClient.invalidateQueries({ queryKey: ["reportSchedules"] });
  };

  const runNow = async (id: string) => {
    try {
      const res = await runReportScheduleNow(id);
      if (res.sent) toast.success(`${t("Sent")} ${res.rows} ${t("rows")} → ${res.recipients}`);
      else toast.info(`${t("Generated")} ${res.rows} ${t("rows")} — ${t("no recipients resolved")}`);
    } catch (e) { toast.error((e as Error).message); }
  };

  const submitDialog = async () => {
    if (!selected) return;
    setDlgBusy(true); setDlgMsg(null);
    const hasChooser = fieldRows.length > 0;
    const outputFields = hasChooser ? buildOutputFields() : undefined;
    try {
      if (dialog === "schedule") {
        if (!schedReportKey) { setDlgMsg(t("Pick a report on the Criteria tab.") ?? "Pick a report."); setDlgBusy(false); return; }
        const frequencyWeekly = [...sForm.weekly].reduce((a, b) => a + b, 0);
        // Fields tab → the exact column customization Hangfire applies before sending.
        const outputFieldRows: ScheduleOutputFieldInput[] = rowsToOutputFields(schedRows).map((o) => ({
          field: o.field, label: o.label, fieldOrder: o.order, sortOrder: o.sortOrder,
        }));
        // The reference schedule has no name field — derive one from the chosen report + cadence.
        const reportName = allReports.find((r) => r.id === schedReportKey)?.name ?? schedReportKey;
        await saveReportSchedule({
          id: editId, reportKey: schedReportKey, name: `${reportName} (${sForm.frequency})`, isScheduled: true,
          mailSubject: sForm.subject || undefined, mailBody: sForm.body || undefined,
          isHideRecipients: sForm.hideRecipients, frequency: sForm.frequency, frequencyWeekly,
          hour24: to24(sForm.hour, sForm.ampm), scheduleStartDate: sForm.startDate || undefined,
          outputFormat: schedFormat, recipientUserIds: [...sForm.userIds],
          recipientRoleIds: [...sForm.roleIds], recipientEmails: [],
          // Criteria tab → Hangfire filters the report data with exactly these values. The Grouping tab
          // rides along as a reserved "__groupBy" value so scheduled CSVs are grouped-sorted.
          values: schedGroupBy.length ? { ...schedValues, __groupBy: schedGroupBy.join(",") } : schedValues,
          outputFields: outputFieldRows,
        });
        queryClient.invalidateQueries({ queryKey: ["reportSchedules"] });
        setDlgMsg(t("Schedule saved.") ?? "Saved");
      } else if (dialog === "email") {
        const emails = eForm.emails.split(/[;,]/).map((s) => s.trim()).filter(Boolean);
        const r = await emailReport({
          reportKey: selected.reportKey, recipientUserIds: [...eForm.userIds], recipientRoleIds: [...eForm.roleIds],
          recipientEmails: emails, isCc: eForm.isCc, isHideRecipients: eForm.hide,
          subject: eForm.subject, body: eForm.body, outputFormat: eForm.outputFormat,
          values: values as Record<string, string>, outputFields,
        });
        setDlgMsg(`${t("Sent")}: ${r.rows} ${t("rows")} → ${r.recipients}`);
      } else if (dialog === "save") {
        await saveReportFilter(selected.reportKey, dlgName, values as Record<string, string>, (outputFields ?? []).map((o) => o.field));
        queryClient.invalidateQueries({ queryKey: ["reportCatalog"] });
        setDlgMsg(t("Saved — it appears under the report in the catalog.") ?? "Saved");
      } else if (dialog === "restrict") {
        await setReportRestrictions(selected.reportKey, [...dlgRoles]);
        setDlgMsg(t("Restrictions saved.") ?? "Saved");
      }
    } catch (e) { setDlgMsg((e as Error).message || "Failed"); }
    setDlgBusy(false);
  };

  const removeSchedule = (id: string) =>
    askConfirm(t("Delete this schedule?") ?? "Delete this schedule?", async () => {
      await deleteReportSchedule(id);
      queryClient.invalidateQueries({ queryKey: ["reportSchedules"] });
      toast.success(t("Schedule deleted."));
    });

  // ---- One dynamic parameter control — rendered with the project's STANDARD form components -----
  // (reference _ReportFields.cshtml dynamic inputs). Every parameter type maps to the same control the
  // rest of the app uses: text/number → InputField, date → the global dual date-picker (DateField),
  // check → CheckBoxField toggle, radio → RadioField, select → DropDownField (searchable lookup),
  // multi-select → CheckboxListField — via the shared FormUtility renderer. `ctx` lets the same renderer
  // drive BOTH the Report tab (main report) and the schedule popup's Criteria tab (chosen template).
  interface ParamCtx {
    reportKey: string;
    fields?: ReportSchemaFieldModel[];
    values: Record<string, string>;
    setValue: (name: string, v: string, fields?: ReportSchemaFieldModel[]) => void;
    /** When present (schedule Criteria tab), DATE fields render as a DYNAMIC relative-date dropdown
     * (e.g. "Start of this month") instead of a calendar — the value saved is the relative token. */
    dateOptions?: ReportLookupOptionModel[];
  }
  const renderParam = (f: ReportSchemaFieldModel, ctx: ParamCtx) => {
    const { reportKey, fields, values: vals, setValue: setVal, dateOptions } = ctx;

    // A single date input: dynamic relative-date dropdown (schedule) OR the standard calendar (ad-hoc).
    const dateField = (name: string, label: string) => {
      if (dateOptions) {
        const val = vals[name] ?? "";
        return (
          <FormUtility key={name} component={{
            name, label, type: "dropDown", floatingLabel: true, value: val,
            displayValue: val ? dateOptions.find((o) => o.value === val)?.label ?? val : "",
            data: toOptionData(dateOptions),
            onSelect: (_n: string, item: { id: string | number }) => setVal(name, String(item.id), fields),
          }} />
        );
      }
      return (
        <FormUtility key={name} component={{
          name, label, type: "date", floatingLabel: true, value: vals[name] ?? "",
          onChange: (e) => setVal(name, e.target.value, fields),
        }} />
      );
    };

    // From/To range (# convention) → two inputs side by side.
    if (f.isRange) {
      const [from, to] = rangeNames(f.field);
      if (f.dataType === "Date")
        return (
          <div key={f.field} className="grid grid-cols-2 gap-2">
            {dateField(from, `${f.label} ${t("From")}`)}
            {dateField(to, `${f.label} ${t("To")}`)}
          </div>
        );
      const inputType = f.dataType === "Number" || f.dataType === "Currency" ? "number" : undefined;
      const mk = (name: string, suffix: string): FormComponentModel => ({
        name, label: `${f.label} ${suffix}`, type: "text", inputType, floatingLabel: true,
        value: vals[name] ?? "", onChange: (e) => setVal(name, e.target.value, fields),
      });
      return (
        <div key={f.field} className="grid grid-cols-2 gap-2">
          <FormUtility component={mk(from, t("From"))} />
          <FormUtility component={mk(to, t("To"))} />
        </div>
      );
    }

    let component: FormComponentModel;
    switch (f.dataType) {
      case "Check":
        component = { name: f.field, label: f.label, type: "checkbox", layout: "auth", value: vals[f.field] ?? "",
          onChange: (e) => setVal(f.field, e.target.checked ? "true" : "", fields) };
        break;
      case "Radio":
        component = { name: f.field, label: f.label, type: "radio", layout: "auth", value: vals[f.field] ?? "",
          data: toOptionData(f.options), onChange: (e) => setVal(f.field, e.target.value, fields) };
        break;
      case "Date":
        return dateField(f.field, f.label);
      case "Select":
        return <LookupField key={f.field} reportKey={reportKey} field={f} values={vals} setValue={setVal} fields={fields} />;
      case "MultiSelect":
        return <LookupField key={f.field} reportKey={reportKey} field={f} values={vals} setValue={setVal} fields={fields} multiple />;
      case "Number":
      case "Currency":
        component = { name: f.field, label: f.label, type: "text", inputType: "number", floatingLabel: true, value: vals[f.field] ?? "",
          onChange: (e) => setVal(f.field, e.target.value, fields) };
        break;
      default: // Text
        component = { name: f.field, label: f.label, type: "text", floatingLabel: true, value: vals[f.field] ?? "",
          onChange: (e) => setVal(f.field, e.target.value, fields) };
    }
    return <FormUtility key={f.field} component={component} />;
  };
  const renderField = (f: ReportSchemaFieldModel) =>
    renderParam(f, { reportKey: schema?.reportKey ?? "", fields: schema?.fields, values, setValue });

  const scheduleColumns = [
    { name: "name", label: "Name" },
    { name: "frequency", label: "Frequency" },
    { name: "time", label: "Time", render: (_x: unknown, r: ReportScheduleItem) => fmtTime(r.timeOfTheDay) },
    { name: "cronExpression", label: "Cron" },
    {
      name: "isActive", label: "Enabled",
      render: (_x: unknown, r: ReportScheduleItem) => (
        <button type="button" onClick={() => toggleSchedule(r)} title={r.isActive ? t("Disable") ?? "Disable" : t("Enable") ?? "Enable"}
          className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium ${
            r.isActive ? "bg-success/15 text-success" : "bg-secondary text-muted"}`}>
          <Power size={11} /> {r.isActive ? t("On") : t("Off")}
        </button>
      ),
    },
    {
      name: "Action", label: "Action",
      render: (_x: unknown, r: ReportScheduleItem) => (
        <div className="flex items-center gap-1">
          <button type="button" onClick={() => openSchedule(r.id)} title={t("Edit") ?? "Edit"} className="rounded p-1 text-primary hover:bg-primary/10"><Pencil size={14} /></button>
          <button type="button" onClick={() => runNow(r.id)} title={t("Run now") ?? "Run now"} className="rounded p-1 text-foreground hover:bg-secondary"><Play size={14} /></button>
          <button type="button" onClick={() => removeSchedule(r.id)} title={t("Delete") ?? "Delete"} className="rounded p-1 text-error hover:bg-error/10"><Trash2 size={14} /></button>
        </div>
      ),
    },
  ] as DataTableColumnModel[];

  const historyRows = useMemo(() => (history ?? []).filter((h: ReportRunModel) => {
    if (!historySearch.trim()) return true;
    const q = historySearch.toLowerCase();
    return h.reportKey.toLowerCase().includes(q) || (h.ranBy ?? "").toLowerCase().includes(q);
  }), [history, historySearch]);

  const historyColumns = [
    { name: "reportKey", label: "Report" },
    { name: "ranAt", label: "When", render: (_x: unknown, r: ReportRunModel) => String(r.ranAt).replace("T", " ").slice(0, 19) },
    { name: "ranBy", label: "By" },
    { name: "rowCount", label: "Rows" },
    { name: "durationMs", label: "Duration (ms)" },
  ] as DataTableColumnModel[];

  return (
    <InventoryLayout
      title={t("Reports")}
      headerDescription={t("Generic stored-procedure-driven reports")}
      headerIcon={<BarChart3 className="h-6 w-6 text-primary" />}
      showForm={false} onList={() => {}} hideAdd hideBack
    >
      <div className="flex h-full min-h-[32rem] gap-4 p-2">
        {/* ---- Report catalog tree (shared, reusable TreeView) ---- */}
        <ReportCatalogTree
          groups={catalog ?? []}
          loading={catalogLoading}
          selectedKey={selected?.reportKey}
          onSelectReport={(r) => { setSelected(r); setTab("report"); }}
          onLoadFilter={loadSavedFilter}
          onRemoveFilter={removeSavedFilter}
        />

        {/* ---- Three-tab workspace (legacy: Report / Schedule / History) ---- */}
        <section className="flex min-w-0 flex-1 flex-col">
          {!selected && (
            <EmptyState
              className="h-full"
              icon={<FileBarChart className="h-6 w-6" />}
              title={t("No report selected")}
              description={t("Pick a report from the catalog on the left to set its criteria and generate it.")}
            />
          )}
          {selected && (
            <>
              <div className="mb-3 flex shrink-0 gap-1 border-b border-border pb-0">
                {TABS.map(({ key, label, Icon }) => (
                  <button key={key} type="button" onClick={() => setTab(key)}
                    className={`-mb-px flex items-center gap-1.5 rounded-t-lg border-x border-t px-4 py-2 text-[13px] font-medium transition-all ${
                      tab === key
                        ? "border-border bg-card text-primary shadow-[0_-2px_0_0_var(--primary)_inset]"
                        : "border-transparent text-muted hover:bg-secondary/40 hover:text-foreground"}`}>
                    <Icon className="h-4 w-4" />{t(label)}
                  </button>
                ))}
              </div>

              {/* ---- TAB 1: Report — scrollable criteria body + sticky action bar ---- */}
              {tab === "report" && (
                <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
                  {/* Top-right controls (legacy order: Grouping → Fields → Restricted Roles). The
                      Grouping button appears ONLY for a pivot report (schema.grouping.supportsGrouping). */}
                  <div className="flex shrink-0 items-center justify-between border-b border-border px-5 py-3">
                    <h3 className="text-sm font-semibold">{schema?.reportName ?? selected.reportName}</h3>
                    <div className="flex items-center gap-3 text-xs font-medium">
                      {schema?.grouping?.supportsGrouping && (
                        <button type="button" onClick={openGrouping}
                          className={`inline-flex items-center gap-1.5 rounded-md border px-2.5 py-1.5 font-semibold transition-colors ${
                            groupBy.length > 0
                              ? "border-primary/40 bg-primary/10 text-primary"
                              : "border-border text-foreground hover:border-primary/40 hover:text-primary"}`}
                          title={t("Group this report — pick and order the grouping levels") ?? "Grouping"}>
                          <Layers size={13} /> {t("Grouping")}{groupBy.length > 0 ? ` · ${groupBy.length}` : ""}
                        </button>
                      )}
                      <button type="button" onClick={openFields} className="inline-flex items-center gap-1 text-primary hover:underline">
                        <ListChecks size={13} /> {t("Fields")}
                      </button>
                      <button type="button" onClick={openRestrict} className="inline-flex items-center gap-1 text-primary hover:underline">
                        <Shield size={13} /> {t("Restricted Roles")}
                      </button>
                    </div>
                  </div>

                  {/* Scrollable criteria body — reusable ReportCriteria (dense 2-col ERP layout). */}
                  <div className="min-h-0 flex-1 overflow-y-auto px-5 py-6">
                    <ReportCriteria fields={schema?.fields ?? []} loading={schemaLoading} renderField={renderField} />
                  </div>

                  {/* Sticky action bar (legacy: left = Save Report; right = Email + Generate) */}
                  <div className="flex shrink-0 items-center justify-between border-t border-border bg-card px-5 py-3">
                    <button type="button" onClick={() => { setDlgName(""); setDlgMsg(null); setDialog("save"); }}
                      className="rounded-md bg-success px-3.5 py-2 text-xs font-semibold text-on-accent shadow-sm transition-opacity hover:opacity-90">
                      {t("Save Report")}
                    </button>
                    <div className="flex items-center gap-2">
                      <button type="button" onClick={openEmail}
                        className="rounded-md bg-warning px-3.5 py-2 text-xs font-semibold text-on-accent shadow-sm transition-opacity hover:opacity-90">
                        {t("Email")}
                      </button>
                      <button type="button" onClick={generate} disabled={schemaLoading}
                        className="inline-flex items-center gap-1.5 rounded-md bg-primary px-4 py-2 text-xs font-semibold text-on-accent shadow-sm transition-opacity hover:opacity-90 disabled:opacity-50">
                        <Play size={13} /> {t("Generate")}
                      </button>
                    </div>
                  </div>
                </div>
              )}

              {/* ---- TAB 2: Schedule ---- */}
              {tab === "schedule" && (
                <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
                  <div className="flex shrink-0 items-center justify-between border-b border-border px-5 py-3">
                    <h3 className="text-sm font-semibold">{t("Schedules")} — {selected.reportName}</h3>
                    <button type="button" onClick={() => openSchedule()}
                      className="inline-flex items-center gap-1 rounded-md bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
                      <Plus className="h-3.5 w-3.5" /> {t("Add")}
                    </button>
                  </div>
                  <div className="min-h-0 flex-1 overflow-y-auto p-3">
                    {(schedules ?? []).length === 0
                      ? <p className="py-6 text-center text-sm text-muted">{t("No schedules yet — use Add.")}</p>
                      : <DataTableProvider dataTable={{ columns: scheduleColumns, data: schedules ?? [], pagination: "None", search: "None" }} />}
                  </div>
                </div>
              )}

              {/* ---- TAB 3: History (standard grid + search) ---- */}
              {tab === "history" && (
                <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-xl border border-border bg-card shadow-sm">
                  <div className="flex shrink-0 items-center justify-between border-b border-border px-5 py-3">
                    <h3 className="text-sm font-semibold">{t("History")} — {selected.reportName}</h3>
                    <input className={`${INPUT} max-w-56`} placeholder={t("Search…") ?? "Search"} value={historySearch}
                      onChange={(e) => setHistorySearch(e.target.value)} />
                  </div>
                  <div className="min-h-0 flex-1 overflow-y-auto p-3">
                    <DataTableProvider dataTable={{ columns: historyColumns, data: historyRows, pagination: "None", search: "None" }} />
                  </div>
                </div>
              )}
            </>
          )}
        </section>
      </div>

      {/* Dialogs */}
      {dialog && (
        <Modal visible size={dialog === "schedule" ? "lg" : "md"}
          title={dialog === "fields" ? t("Report Fields") : dialog === "grouping" ? t("Report Grouping") : dialog === "schedule" ? (editId ? t("Edit Schedule") : t("Add Schedule")) : dialog === "email" ? t("Email Report") : dialog === "save" ? t("Save Report") : t("Restricted Roles")}
          description={selected?.reportName}
          onClose={() => setDialog(null)}
          footer={
            dialog === "fields" || dialog === "grouping" ? (
              // reference popbuttons: Reset (yellow, left) | Cancel (grey) + Close (green, right)
              <div className="flex w-full items-center justify-between">
                <button type="button" onClick={dialog === "fields" ? resetFields : resetGrouping}
                  className="rounded-md bg-warning px-3 py-1.5 text-sm font-medium text-on-accent hover:opacity-90">{t("Reset")}</button>
                <div className="flex items-center gap-2">
                  <button type="button" onClick={() => setDialog(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary">{t("Cancel")}</button>
                  <button type="button" onClick={dialog === "fields" ? commitFields : commitGrouping} className="rounded-md bg-success px-3 py-1.5 text-sm font-medium text-on-accent hover:opacity-90">{t("Close")}</button>
                </div>
              </div>
            ) : (
              <>
                <button type="button" onClick={() => setDialog(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary">{t("Cancel")}</button>
                <button type="button" disabled={dlgBusy
                    || (dialog === "email" && (eForm.userIds.size === 0 && eForm.roleIds.size === 0 && !eForm.emails.trim() && !eForm.isCc))
                    || (dialog === "schedule" && (!schedReportKey || (sForm.userIds.size === 0 && sForm.roleIds.size === 0)))
                    || (dialog === "save" && !dlgName.trim())}
                  onClick={submitDialog}
                  className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50">
                  {dlgBusy ? t("Working…") : dialog === "email" ? t("Send") : t("Save")}
                </button>
              </>
            )
          }>
          <div className="space-y-3 text-sm text-foreground">
            {dialog === "fields" && (
              <FieldsTable rows={draftRows} setRows={setDraftRows} dragField={dragField} setDragField={setDragField} />
            )}
            {dialog === "grouping" && (
              <GroupingCustomizer fields={schema?.grouping?.groupableFields ?? []} maxLevels={schema?.grouping?.maxGroupLevels ?? 3}
                value={groupDraft} onChange={setGroupDraft} />
            )}
            {dialog === "save" && (
              <FormUtility component={{ name: "save-name", type: "text", label: t("Name"), required: true, floatingLabel: true,
                value: dlgName, onChange: (e) => setDlgName(e.target.value) }} />
            )}
            {dialog === "schedule" && (
              <div>
                {/* reference _ScheduleReportTop.cshtml — one popup, four tabs. */}
                <div className="mb-3 flex gap-1 border-b border-border">
                  {SCHED_TABS.map((st) => (
                    <button key={st.key} type="button" onClick={() => setSchedTab(st.key)}
                      className={`-mb-px rounded-t-md border-x border-t px-3 py-1.5 text-xs font-medium transition-colors ${
                        schedTab === st.key ? "border-border bg-card text-primary" : "border-transparent text-muted hover:text-foreground"}`}>
                      {t(st.label)}
                    </button>
                  ))}
                </div>

                {/* --- Criteria tab: pick a Template, choose the output format, set its filter criteria. --- */}
                {schedTab === "criteria" && (
                  <div className="space-y-3">
                    <FormUtility component={{
                      name: "schedule-template", label: "Template", type: "dropDown", floatingLabel: true,
                      value: schedReportKey, displayValue: allReports.find((r) => r.id === schedReportKey)?.name ?? "",
                      data: allReports, onSelect: (_n: string, item: { id: string | number }) => initTemplate(String(item.id)),
                    }} />
                    <div>
                      <label className={LABEL}>{t("Output Format")}</label>
                      <div className="flex gap-4 pt-0.5">
                        {[{ v: 1, label: "CSV" }, { v: 0, label: t("Tab") }].map((o) => (
                          <label key={o.v} className="flex items-center gap-1.5 text-sm">
                            <input type="radio" name="schedfmt" className="accent-primary" checked={schedFormat === o.v} onChange={() => setSchedFormat(o.v)} />
                            {o.label}
                          </label>
                        ))}
                      </div>
                    </div>
                    <div className="border-t border-border pt-3">
                      {schedSchemaLoading && <Loading />}
                      {!schedReportKey && <p className="py-4 text-center text-sm text-muted">{t("Select a report to configure its filter criteria.")}</p>}
                      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                        {(schedSchema?.fields ?? []).map((f) =>
                          renderParam(f, { reportKey: schedReportKey, fields: schedSchema?.fields, values: schedValues, setValue: setSchedValue, dateOptions: dynamicDateOptions }))}
                      </div>
                      {schedReportKey && (schedSchema?.fields ?? []).length === 0 && !schedSchemaLoading && (
                        <p className="py-2 text-center text-xs text-muted">{t("This report has no filter parameters.")}</p>
                      )}
                    </div>
                  </div>
                )}

                {/* --- Grouping tab (reference _ReportGroupingCustomize.cshtml): pivot the scheduled report. --- */}
                {schedTab === "grouping" && (
                  schedSchema?.grouping?.supportsGrouping
                    ? <GroupingCustomizer fields={schedSchema.grouping.groupableFields} maxLevels={schedSchema.grouping.maxGroupLevels}
                        value={schedGroupBy} onChange={setSchedGroupBy} />
                    : <p className="py-10 text-center text-sm text-muted">{t("This report does not support grouping.")}</p>
                )}

                {/* --- Fields tab: the same column customizer, applied to the scheduled output. --- */}
                {schedTab === "fields" && (
                  <FieldsTable rows={schedRows} setRows={setSchedRows} dragField={dragField} setDragField={setDragField} />
                )}

                {/* --- Schedule tab (reference _ScheduleForm.cshtml): cadence + recipients + subject/body. --- */}
                {schedTab === "schedule" && (
                  <div className="space-y-3">
                    <div className="grid grid-cols-4 gap-2">
                      <FormUtility component={{ name: "sched-freq", type: "select", label: t("Frequency"), floatingLabel: true,
                        value: sForm.frequency, data: FREQUENCIES.map((f) => ({ id: f, name: t(f) })),
                        onChange: (e) => setSForm((p) => ({ ...p, frequency: e.target.value })) }} />
                      <FormUtility component={{ name: "sched-hr", type: "select", label: t("Hr"), floatingLabel: true,
                        value: sForm.hour, data: Array.from({ length: 12 }, (_, i) => ({ id: i + 1, name: String(i + 1) })),
                        onChange: (e) => setSForm((p) => ({ ...p, hour: Number(e.target.value) })) }} />
                      <FormUtility component={{ name: "sched-ampm", type: "select", label: t("AM/PM"), floatingLabel: true,
                        value: sForm.ampm, data: [{ id: "AM", name: "AM" }, { id: "PM", name: "PM" }],
                        onChange: (e) => setSForm((p) => ({ ...p, ampm: e.target.value })) }} />
                      <FloatingLabel label={t("Start Date")} active>
                        <input type="date" className={FORM_INPUT_CLASS} value={sForm.startDate} onChange={(e) => setSForm((p) => ({ ...p, startDate: e.target.value }))} />
                      </FloatingLabel>
                    </div>

                    {sForm.frequency === "Weekly" && (
                      <div className="flex flex-wrap gap-4 rounded-md border border-border/70 px-3 py-2">
                        {WEEKDAYS.map((d) => (
                          <label key={d.bit} className="flex items-center gap-1.5 text-sm">
                            <span>{t(d.label)}</span>
                            <input type="checkbox" className="accent-primary" checked={sForm.weekly.has(d.bit)}
                              onChange={() => setSForm((p) => { const w = new Set(p.weekly); if (w.has(d.bit)) w.delete(d.bit); else w.add(d.bit); return { ...p, weekly: w }; })} />
                          </label>
                        ))}
                      </div>
                    )}

                    <div className="grid grid-cols-2 gap-3">
                      <ChipMultiSelect label={t("Users")} addLabel={t("+ Add user") ?? "+ Add user"} options={userOptions}
                        selected={sForm.userIds} icon={<UserRound size={11} />}
                        onChange={(next) => setSForm((p) => ({ ...p, userIds: next }))} />
                      <ChipMultiSelect label={t("Roles")} addLabel={t("+ Add role") ?? "+ Add role"} options={roleOptions}
                        selected={sForm.roleIds} icon={<Shield size={11} />}
                        onChange={(next) => setSForm((p) => ({ ...p, roleIds: next }))} />
                    </div>

                    <label className="flex items-center gap-2 text-sm">
                      <input type="checkbox" className="accent-primary" checked={sForm.hideRecipients} onChange={(e) => setSForm((p) => ({ ...p, hideRecipients: e.target.checked }))} />
                      {t("Hide all Recipient")}
                    </label>
                    <FormUtility component={{ name: "sched-subject", type: "text", label: t("Subject"), required: true, floatingLabel: true,
                      value: sForm.subject, onChange: (e) => setSForm((p) => ({ ...p, subject: e.target.value })) }} />
                    <FormUtility component={{ name: "sched-body", type: "textarea", label: t("Body"), required: true, floatingLabel: true, rowNo: 3,
                      value: sForm.body, onChange: (e) => setSForm((p) => ({ ...p, body: e.target.value })) }} />
                  </div>
                )}
              </div>
            )}
            {dialog === "email" && (
              <div className="space-y-3">
                {/* reference _SendReportByEmail.cshtml — Users + Roles + CC Me + Hide + Subject + Body + Format */}
                <div className="grid grid-cols-2 gap-3">
                  <ChipMultiSelect label={t("Users")} addLabel={t("+ Add user") ?? "+ Add user"} options={userOptions}
                    selected={eForm.userIds} icon={<UserRound size={11} />}
                    onChange={(next) => setEForm((p) => ({ ...p, userIds: next }))} />
                  <ChipMultiSelect label={t("Roles")} addLabel={t("+ Add role") ?? "+ Add role"} options={roleOptions}
                    selected={eForm.roleIds} icon={<Shield size={11} />}
                    onChange={(next) => setEForm((p) => ({ ...p, roleIds: next }))} />
                </div>
                <FormUtility component={{ name: "email-extra", type: "text", label: t("Additional e-mails (semicolon-separated)"), floatingLabel: true,
                  value: eForm.emails, onChange: (e) => setEForm((p) => ({ ...p, emails: e.target.value })) }} />
                <div className="flex flex-wrap gap-5">
                  <label className="flex items-center gap-2 text-sm">
                    <input type="checkbox" className="accent-primary" checked={eForm.isCc} onChange={(e) => setEForm((p) => ({ ...p, isCc: e.target.checked }))} />
                    {t("CC Me")}
                  </label>
                  <label className="flex items-center gap-2 text-sm">
                    <input type="checkbox" className="accent-primary" checked={eForm.hide} onChange={(e) => setEForm((p) => ({ ...p, hide: e.target.checked }))} />
                    {t("Hide all Recipient")}
                  </label>
                </div>
                <FormUtility component={{ name: "email-subject", type: "text", label: t("Subject"), required: true, floatingLabel: true,
                  value: eForm.subject, onChange: (e) => setEForm((p) => ({ ...p, subject: e.target.value })) }} />
                <FormUtility component={{ name: "email-message", type: "textarea", label: t("Message"), required: true, floatingLabel: true, rowNo: 3,
                  value: eForm.body, onChange: (e) => setEForm((p) => ({ ...p, body: e.target.value })) }} />
                <div><label className={LABEL}>{t("Output Format")}</label>
                  <div className="flex gap-4 pt-0.5">
                    {[{ v: 1, label: "CSV" }, { v: 0, label: t("Tab") }].map((o) => (
                      <label key={o.v} className="flex items-center gap-1.5 text-sm">
                        <input type="radio" name="emailfmt" className="accent-primary" checked={eForm.outputFormat === o.v} onChange={() => setEForm((p) => ({ ...p, outputFormat: o.v }))} />
                        {o.label}
                      </label>
                    ))}
                  </div></div>
              </div>
            )}
            {dialog === "restrict" && (
              <div className="space-y-3">
                <p className="text-xs text-muted">{t("Only holders of a selected role can see and run this report. Leave empty to allow everyone.")}</p>
                <ChipMultiSelect label={t("Restricted to roles")} addLabel={t("+ Add role") ?? "+ Add role"}
                  options={restrictRoleOptions} selected={dlgRoles} icon={<Shield size={11} />}
                  onChange={(next) => setDlgRoles(next)} />
              </div>
            )}
            {dlgMsg && <p className="text-xs text-primary">{dlgMsg}</p>}
          </div>
        </Modal>
      )}

      {/* Standard destructive-confirmation popup (project DialogModal) — no native window.confirm. */}
      <DialogModal
        visible={!!confirm}
        title={t("Confirm")}
        variant="destructive"
        okLabel={t("Delete")}
        onClose={() => setConfirm(null)}
        onOk={() => { confirm?.onOk(); setConfirm(null); }}
      >
        <span>{confirm?.message}</span>
      </DialogModal>
    </InventoryLayout>
  );
}

export default memo(ReportViewer);
