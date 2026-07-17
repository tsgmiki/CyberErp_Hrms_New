import type { FormComponentModel } from "@/models";
import { formatBackendDate } from "@/components/util/dateFormater";
import { FieldShell } from "./fieldShell";
import { FloatingLabel } from "./floatingLabel";
import { FORM_COMPACT_INPUT_CLASS, FORM_INPUT_CLASS, LIST_FILTER_CONTROL_CLASS } from "./fieldStyles";
import DualDateField from "./dualDateField";

const DateField = (props: FormComponentModel & { compact?: boolean }) => {
  const {
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
    floatingLabel,
  } = props;

  // Floating-label variant → a single native date input with a floating label. Native date inputs
  // always show a mm/dd/yyyy hint, so the label stays in the floated position (above) to avoid overlap.
  if (floatingLabel && label?.trim() && !compact) {
    return (
      <FloatingLabel htmlFor={name} label={label} required={required} active>
        <input
          className={FORM_INPUT_CLASS}
          name={name}
          id={name}
          type="date"
          disabled={disabled}
          maxLength={maxLength}
          onChange={onChange}
          value={value ? formatBackendDate(value) : ""}
          onKeyDown={onKeyDown}
        />
      </FloatingLabel>
    );
  }

  // Full form fields get the dual Gregorian + Ethiopian picker; compact list-filter
  // controls stay as a single lightweight native date input.
  if (!compact) {
    return <DualDateField {...props} />;
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
