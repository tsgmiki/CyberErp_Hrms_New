import type { FormComponentModel } from "@/models";
import { useTranslation } from "react-i18next";
import { FieldShell } from "./fieldShell";
import { ZodErrors } from "../common/statusMessage/status";
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
  floatingLabel,
}: FormComponentModel) => {
  const { t } = useTranslation();

  // Floating-label variant: the label sits at the top-left as a placeholder and floats above the
  // textarea on focus/value (peer / :placeholder-shown technique).
  if (floatingLabel && label?.trim()) {
    return (
      <div className="w-full">
        <div className="relative">
          <textarea
            className={`peer ${FORM_TEXTAREA_CLASS}`}
            {...error?.register?.(name, {
              required: { value: required, message: `${name} Required` },
            })}
            rows={rowNo ?? 4}
            name={name}
            id={name}
            disabled={disabled}
            onChange={onChange}
            value={value == null ? "" : value}
            placeholder=" "
          />
          <label
            htmlFor={name}
            className="pointer-events-none absolute left-2.5 top-2 z-10 origin-[0] -translate-y-4 scale-90 bg-background px-1 text-primary transition-all duration-150
              peer-placeholder-shown:top-4 peer-placeholder-shown:translate-y-0 peer-placeholder-shown:scale-100 peer-placeholder-shown:text-sm peer-placeholder-shown:text-muted
              peer-focus:top-2 peer-focus:-translate-y-4 peer-focus:scale-90 peer-focus:text-primary
              peer-disabled:opacity-60"
          >
            {t(label)}
            {required ? <span className="text-error"> *</span> : null}
          </label>
        </div>
        {error ? (
          <span className="mt-1 block">
            <ZodErrors error={error} />
          </span>
        ) : null}
      </div>
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
