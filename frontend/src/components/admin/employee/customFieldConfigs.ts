import type { FormComponentModel } from "@/models";
import { yesNoOptions } from "@/constants/orgStructure";

/** Minimal field-definition shape the renderer needs — satisfied by both the HC021
 * `EmployeeFieldModel` and the dynamic-form `DynamicFormFieldModel`. */
export interface RenderableFieldDef {
  name?: string;
  label?: string;
  dataType?: string;
  options?: string;
  isRequired?: boolean;
}

/**
 * Shared custom-field engine (HC021) renderer: maps active field definitions to `FormComponentModel`s
 * by their `dataType`, used by both the Employee master form and every child form.
 *
 * The Employee master form binds values to a dedicated `customData` state, so it uses `prefix=""`.
 * The config-driven child forms (FormProvider + native FormData) bind to the form's own `formData`
 * under a `cf_` prefix, which `createSaveService({ customFields: true })` gathers back into a nested
 * `customFields` dictionary — the prefix also prevents a custom field name from clobbering a real
 * form field.
 *
 * @param defs      active field definitions for the target owner form
 * @param values    current values keyed by the (prefixed) field name
 * @param onChange  text/number/date change handler `(e) => ...`
 * @param onSelect  dropDown select handler `(name, { id }) => ...`
 * @param prefix    field-name prefix (default `"cf_"`; pass `""` for the master form)
 * @param errors    optional zodErrors map keyed by the (prefixed) field name
 */
export function buildCustomFieldComponents(
  defs: RenderableFieldDef[] | undefined,
  values: Record<string, unknown>,
  onChange: (e: any) => void,
  onSelect: (name: string, r: any) => void,
  prefix = "cf_",
  errors?: Record<string, string[] | undefined>,
): FormComponentModel[] {
  return (defs ?? []).map((def) => {
    const key = `${prefix}${def.name ?? ""}`;
    const raw = values[key];
    const value = raw == null ? "" : String(raw);
    const common = { name: key, label: def.label, required: def.isRequired, error: errors?.[key] };
    switch (def.dataType) {
      case "Number":
        return { ...common, type: "text", inputType: "number", value, onChange };
      case "Date":
        return { ...common, type: "date", value, onChange };
      case "Boolean":
        return {
          ...common, type: "dropDown", onSelect, value,
          displayValue: value === "true" ? "Yes" : value === "false" ? "No" : "",
          data: yesNoOptions as never,
        };
      case "Select":
        return {
          ...common, type: "dropDown", onSelect, value, displayValue: value,
          data: (def.options ?? "").split(",").map((o) => o.trim()).filter(Boolean).map((o) => ({ id: o, name: o })) as never,
        };
      default:
        return { ...common, type: "text", value, onChange };
    }
  }) as FormComponentModel[];
}
