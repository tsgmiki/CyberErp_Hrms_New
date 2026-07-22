"use client";
import type { FormComponentModel } from "@/models";
import { useTranslation } from "react-i18next";
import { X } from "lucide-react";
import DropDownField from "./dropDownField";

type Option = { id: string | number; name: string };

/**
 * The project's standard MULTI-SELECT combobox: the same searchable dropdown as the single-select
 * `DropDownField` (type to filter, pick an option), but you can pick MANY — each pick becomes a
 * removable chip and is dropped from the list. The value is a comma-joined id string and it emits
 * `onSelect(name, commaIds)`, so it is a drop-in replacement for the inline `CheckboxListField`
 * anywhere a multi-select parameter is rendered (e.g. report `MultiSelect` criteria).
 */
const MultiSelectField = (props: FormComponentModel) => {
  const {
    name, label, data = [], value = "", onSelect,
    disabled, required, placeholder, floatingLabel, layout, colSpan, error,
  } = props;
  const { t } = useTranslation();

  const ids = String(value ?? "").split(",").map((s) => s.trim()).filter(Boolean);
  const options = data as Option[];
  const nameOf = (id: string) => options.find((o) => String(o.id) === id)?.name ?? id;
  // Only offer options that aren't already selected.
  const available = options.filter((o) => !ids.includes(String(o.id)));

  const emit = (nextIds: string[]) => onSelect?.(name as string, nextIds.join(","));
  const add = (id: string) => { if (id && !ids.includes(id)) emit([...ids, id]); };
  const remove = (id: string) => emit(ids.filter((x) => x !== id));

  return (
    <div className={colSpan === "full" ? "w-full" : "w-full"}>
      {/* Searchable add-picker — reuses the standard combobox. Keyed on the selection count so it
          resets (clears its search/label) after each pick. */}
      <DropDownField
        key={`${name}-ms-${ids.length}`}
        type="dropDown"
        name={`${name}__add`}
        label={label}
        required={required && ids.length === 0}
        disabled={disabled}
        floatingLabel={floatingLabel}
        layout={layout}
        colSpan={colSpan}
        placeholder={placeholder ?? (t("Select…") as string)}
        value=""
        displayValue=""
        data={available}
        error={error}
        onSelect={(_n: string, item: Option) => add(String(item.id))}
      />

      {ids.length > 0 && (
        <div className="mt-1.5 flex flex-wrap gap-1.5">
          {ids.map((id) => (
            <span key={id} className="inline-flex items-center gap-1 rounded-full border border-primary/40 bg-primary/10 px-2 py-0.5 text-xs text-primary">
              <span className="max-w-[180px] truncate">{nameOf(id)}</span>
              <button type="button" disabled={disabled} onClick={() => remove(id)}
                className="ml-0.5 rounded-full hover:opacity-70 disabled:opacity-40" aria-label={t("Remove") ?? "Remove"}>
                <X size={11} />
              </button>
            </span>
          ))}
        </div>
      )}
    </div>
  );
};

export default MultiSelectField;
