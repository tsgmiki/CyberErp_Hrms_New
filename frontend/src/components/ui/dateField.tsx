import type { FormComponentModel } from "@/models";
import { formatBackendDate } from "@/components/util/dateFormater";
import { FieldShell } from "./fieldShell";
import { FORM_COMPACT_INPUT_CLASS, FORM_INPUT_CLASS, LIST_FILTER_CONTROL_CLASS } from "./fieldStyles";

const DateField = ({
  error,
  maxLength,
  required = defaultProps.required,
  name,
  label,
  disabled = defaultProps.disabled,
  labelWidth,
  value = defaultProps.value,
  onKeyDown,
  onChange,
  colSpan,
  compact,
  layout,
}: FormComponentModel & { compact?: boolean }) => {
  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      error={error}
      layout={compact ? "toolbar" : layout ?? "horizontal"}
      hideLabel={compact ? true : undefined}
      controlClassName={compact ? LIST_FILTER_CONTROL_CLASS : undefined}
    >
      <input
        className={compact ? FORM_COMPACT_INPUT_CLASS : FORM_INPUT_CLASS}
        name={name}
        id={name}
        type="date"
        disabled={disabled}
        maxLength={maxLength}
        onChange={onChange}
        value={value ? formatBackendDate(value) : ""}
        onKeyDown={onKeyDown}
      />
    </FieldShell>
  );
};

const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  inputType: "text",
  className: "",
};

export default DateField;
