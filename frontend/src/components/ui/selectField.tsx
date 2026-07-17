import type { FormComponentModel } from "@/models";
import { FieldShell } from "./fieldShell";
import { FloatingLabel } from "./floatingLabel";
import {
  FORM_COMPACT_SELECT_CLASS,
  FORM_SELECT_CLASS,
  LIST_FILTER_CONTROL_CLASS,
} from "./fieldStyles";

const SelectField = ({
  error,
  required = defaultProps.required,
  name,
  label,
  inputType,
  disabled = defaultProps.disabled,
  labelWidth,
  value = defaultProps.value,
  onChange,
  data,
  colSpan,
  compact,
  layout,
  floatingLabel,
}: FormComponentModel & { compact?: boolean }) => {
  // Floating-label variant: the label floats above the select once a value is chosen.
  if (floatingLabel && label && !compact) {
    return (
      <FloatingLabel htmlFor={name} label={label} required={required} active={!!value}>
        <select
          className={FORM_SELECT_CLASS}
          {...error?.register?.(name, {
            required: { value: required, message: `${name} Required` },
          })}
          name={name}
          id={name}
          type={inputType}
          disabled={disabled}
          onChange={onChange}
          value={value ?? ""}
        >
          {data?.map((item: { id: string | number; name: string }) => (
            <option key={String(item.id)} value={item.id}>
              {item.name}
            </option>
          ))}
        </select>
      </FloatingLabel>
    );
  }
  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      error={error}
      layout={compact ? "toolbar" : layout ?? "horizontal"}
      hideLabel={compact ? true : !label}
      controlClassName={compact ? LIST_FILTER_CONTROL_CLASS : undefined}
    >
      <select
        className={compact ? FORM_COMPACT_SELECT_CLASS : FORM_SELECT_CLASS}
        {...error?.register?.(name, {
          required: { value: required, message: `${name} Required` },
        })}
        name={name}
        id={name}
        type={inputType}
        disabled={disabled}
        onChange={onChange}
        value={value ?? ""}
      >
        {data?.map((item: { id: string | number; name: string }) => (
          <option key={String(item.id)} value={item.id}>
            {item.name}
          </option>
        ))}
      </select>
    </FieldShell>
  );
};

const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  className: "",
};

export default SelectField;
