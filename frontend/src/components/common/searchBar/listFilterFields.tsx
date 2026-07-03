import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import type { ParameterModel } from "@/models";
import DateField from "@/components/ui/dateField";
import DropDownField from "@/components/ui/dropDownField";
import SelectField from "@/components/ui/selectField";
import type {
  ListFilterAsyncSelect,
  ListFilterDefinition,
  ListFilterSelect,
} from "./listFilterTypes";

interface ListFilterFieldsProps {
  filters: ListFilterDefinition[];
  param: ParameterModel;
  onPatch: (patch: Partial<ParameterModel>) => void;
  disabled?: boolean;
  /** Toolbar chips vs modal form layout. */
  compact?: boolean;
}

export function ListFilterFields({
  filters,
  param,
  onPatch,
  disabled = false,
  compact = false,
}: ListFilterFieldsProps) {
  const { t } = useTranslation();
  const fieldLayout = compact ? undefined : ("stack" as const);

  return (
    <div
      className={
        compact
          ? "flex flex-wrap items-end gap-2"
          : "grid grid-cols-1 gap-x-6 gap-y-4 lg:grid-cols-2"
      }
    >
      {filters.map((filter) => {
        if (filter.type === "dateRange") {
          const fromKey = filter.fromKey ?? "fromDate";
          const toKey = filter.toKey ?? "toDate";
          return (
            <div
              key={`${String(fromKey)}-${String(toKey)}`}
              className={
                compact
                  ? "flex flex-wrap items-end gap-1.5"
                  : "min-w-0 space-y-2 lg:col-span-2"
              }
            >
              {filter.label ? (
                <p
                  className={
                    compact
                      ? "pb-2 text-xs font-medium text-muted"
                      : "text-sm font-medium text-foreground"
                  }
                >
                  {filter.label}
                </p>
              ) : null}
              <div
                className={
                  compact
                    ? "flex flex-wrap items-end gap-2"
                    : "grid grid-cols-1 gap-4 sm:grid-cols-2"
                }
              >
                <DateField
                  compact={compact}
                  layout={fieldLayout}
                  name={String(fromKey)}
                  label={t("From date")}
                  type="date"
                  disabled={disabled}
                  value={String(param[fromKey] ?? "")}
                  onChange={(e) =>
                    onPatch({ [fromKey]: e.target.value } as Partial<ParameterModel>)
                  }
                />
                {compact ? (
                  <span className="pb-2 text-xs text-muted">–</span>
                ) : null}
                <DateField
                  compact={compact}
                  layout={fieldLayout}
                  name={String(toKey)}
                  label={t("To date")}
                  type="date"
                  disabled={disabled}
                  value={String(param[toKey] ?? "")}
                  onChange={(e) =>
                    onPatch({ [toKey]: e.target.value } as Partial<ParameterModel>)
                  }
                />
              </div>
            </div>
          );
        }

        if (filter.type === "asyncSelect") {
          return (
            <div key={String(filter.paramKey)} className="min-w-0">
              <AsyncFilterSelect
                filter={filter}
                value={String(param[filter.paramKey] ?? "")}
                disabled={disabled}
                compact={compact}
                layout={fieldLayout}
                onChange={(value) =>
                  onPatch({ [filter.paramKey]: value } as Partial<ParameterModel>)
                }
              />
            </div>
          );
        }

        return (
          <div key={String(filter.paramKey)} className="min-w-0">
            <StaticFilterSelect
              filter={filter}
              value={String(param[filter.paramKey] ?? "")}
              disabled={disabled}
              compact={compact}
              layout={fieldLayout}
              onChange={(value) =>
                onPatch({ [filter.paramKey]: value } as Partial<ParameterModel>)
              }
            />
          </div>
        );
      })}
    </div>
  );
}

function StaticFilterSelect({
  filter,
  value,
  disabled,
  compact,
  layout,
  onChange,
}: {
  filter: ListFilterSelect;
  value: string;
  disabled: boolean;
  compact: boolean;
  layout?: "stack";
  onChange: (value: string) => void;
}) {
  const selectData = useMemo(
    () => filter.options.map((option) => ({ id: option.value, name: option.label })),
    [filter.options],
  );

  return (
    <SelectField
      compact={compact}
      layout={layout}
      name={String(filter.paramKey)}
      label={filter.label}
      type="select"
      disabled={disabled}
      value={value}
      data={selectData}
      onChange={(e) => onChange(e.target.value)}
    />
  );
}

function AsyncFilterSelect({
  filter,
  value,
  disabled,
  compact,
  layout,
  onChange,
}: {
  filter: ListFilterAsyncSelect;
  value: string;
  disabled: boolean;
  compact: boolean;
  layout?: "stack";
  onChange: (value: string) => void;
}) {
  const valueKey = filter.valueKey ?? "id";
  const labelKey = filter.labelKey ?? "name";
  const { data, isLoading } = useQuery({
    queryKey: filter.queryKey,
    queryFn: filter.queryFn,
    staleTime: 60_000,
  });

  const dropdownData = useMemo(() => {
    const rows = data?.data ?? [];
    const mapped = rows
      .map((row) => ({
        id: String(row[valueKey] ?? ""),
        name: String(row[labelKey] ?? ""),
      }))
      .filter((row) => row.id && row.name);

    if (filter.placeholder) {
      return [{ id: "", name: filter.placeholder }, ...mapped];
    }
    return mapped;
  }, [data?.data, filter.placeholder, labelKey, valueKey]);

  const displayValue = useMemo(() => {
    if (!value) return filter.placeholder ?? "";
    return dropdownData.find((row) => row.id === value)?.name ?? "";
  }, [dropdownData, filter.placeholder, value]);

  return (
    <DropDownField
      compact={compact}
      layout={layout}
      name={String(filter.paramKey)}
      label={filter.label}
      placeholder={filter.placeholder}
      type="dropDown"
      disabled={disabled}
      value={value}
      displayValue={displayValue}
      data={dropdownData}
      isLoading={isLoading}
      onSelect={(_name, record) => onChange(record?.id ?? "")}
    />
  );
}
