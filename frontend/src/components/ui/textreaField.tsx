import type { FormComponentModel } from "@/models";
import { FieldShell } from "./fieldShell";
import { FORM_TEXTAREA_CLASS } from "./fieldStyles";

const TextreaField = ({
  error,
  required = defaultProps.required,
  name,
  label,
  inputType,
  disabled,
  labelWidth,
  value = defaultProps.value,
  rowNo,
  colSpan,
  onChange,
}: FormComponentModel) => {
  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      error={error}
      hideLabel={!label}
    >
      <textarea
        className={FORM_TEXTAREA_CLASS}
        {...error?.register?.(name, {
          required: { value: required, message: `${name} Required` },
        })}
        rows={rowNo ?? 4}
        name={name}
        id={name}
        type={inputType}
        disabled={disabled}
        onChange={onChange}
        value={value == null ? undefined : value}
      />
    </FieldShell>
  );
};

const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  className: "",
};

export default TextreaField;
