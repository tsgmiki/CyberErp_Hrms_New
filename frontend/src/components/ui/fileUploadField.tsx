import type { FormComponentModel } from "@/models";
import { FieldShell } from "./fieldShell";
import { FORM_FILE_CLASS } from "./fieldStyles";

const FileUploadField = ({
  error,
  maxLength,
  required = defaultProps.required,
  name,
  label,
  disabled = defaultProps.disabled,
  labelWidth,
  value = defaultProps.value,
  onChange,
  colSpan,
}: FormComponentModel) => {
  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      error={error}
    >
      <input
        className={FORM_FILE_CLASS}
        name={name}
        id={name}
        type="file"
        disabled={disabled}
        maxLength={maxLength}
        onChange={onChange}
        defaultValue={value}
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

export default FileUploadField;
