import { useState } from "react";
import { Eye, EyeOff } from "lucide-react";
import { useTranslation } from "react-i18next";
import type { FormComponentModel } from "../../models";
import { FieldShell } from "./fieldShell";
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
