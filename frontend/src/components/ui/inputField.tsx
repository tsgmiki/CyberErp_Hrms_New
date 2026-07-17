import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { FormComponentModel } from "../../models";
import { FieldShell } from "./fieldShell";
import { ZodErrors } from "../common/statusMessage/status";
import { FORM_INPUT_CLASS } from "./fieldStyles";

const InputField = ({
  error,
  maxLength,
  required = defaultProps.required,
  name,
  label,
  type,
  inputType,
  disabled = defaultProps.disabled,
  className = defaultProps.className,
  labelWidth,
  value = defaultProps.value,
  placeholder,
  colSpan,
  layout,
  showPasswordToggle,
  onKeyDown,
  onChange,
  onBlur,
  floatingLabel,
}: FormComponentModel) => {
  const { t } = useTranslation();
  const [showPassword, setShowPassword] = useState(false);
  const isAuthLayout = layout === "auth";
  const isPasswordField =
    type === "password" || inputType === "password" || showPasswordToggle;

  const resolvedType = () => {
    if (inputType === "number") return "number";
    if (isPasswordField && isAuthLayout && showPasswordToggle) {
      return showPassword ? "text" : "password";
    }
    if (inputType === "password") return "password";
    return type;
  };

  // Floating-label variant (reusable app-wide): the label sits INSIDE the control as a placeholder
  // and floats above it on focus or when the field has a value. Uses the peer/:placeholder-shown
  // technique — the input's real placeholder is a single space so it toggles as the field fills.
  if (floatingLabel && label?.trim()) {
    return (
      <div className="w-full">
        <div className="relative">
          <input
            className={`peer ${FORM_INPUT_CLASS} ${className}`}
            name={name}
            id={name}
            type={resolvedType()}
            disabled={disabled}
            maxLength={maxLength}
            onChange={onChange}
            onBlur={onBlur}
            onKeyDown={onKeyDown}
            value={value ?? ""}
            placeholder={placeholder ? t(placeholder) : " "}
            step={inputType === "number" ? "0.0001" : undefined}
          />
          <label
            htmlFor={name}
            className="pointer-events-none absolute left-2.5 top-2 z-10 origin-[0] -translate-y-4 scale-90 bg-background px-1 text-sm text-muted transition-all duration-150
              peer-placeholder-shown:top-1/2 peer-placeholder-shown:-translate-y-1/2 peer-placeholder-shown:scale-100 peer-placeholder-shown:text-muted
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

  const input = (
    <div className={isPasswordField && showPasswordToggle ? "relative" : undefined}>
      <input
        className={`${FORM_INPUT_CLASS} ${className} ${isPasswordField && showPasswordToggle ? "pr-10" : ""}`}
        name={name}
        id={name}
        type={resolvedType()}
        disabled={disabled}
        maxLength={maxLength}
        onChange={onChange}
        onBlur={onBlur}
        value={value ?? ""}
        placeholder={placeholder ? t(placeholder) : undefined}
        onKeyDown={onKeyDown}
        step={inputType === "number" ? "0.0001" : undefined}
        autoComplete={
          name === "password" || name === "confirmPassword"
            ? "current-password"
            : name === "userName"
              ? "username"
              : undefined
        }
      />
      {isPasswordField && showPasswordToggle ? (
        <button
          type="button"
          onClick={() => setShowPassword((prev) => !prev)}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-foreground"
          aria-label={showPassword ? "Hide password" : "Show password"}
        >
          {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
        </button>
      ) : null}
    </div>
  );

  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      layout={isAuthLayout ? "auth" : "horizontal"}
      error={error}
      hideLabel={!label?.trim()}
    >
      {input}
    </FieldShell>
  );
};

const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  type: "text" as const,
  className: "",
};

export default InputField;
