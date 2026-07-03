import type { FormComponentModel } from "@/models";
import { FieldShell } from "./fieldShell";

const LabelField = ({
  required = defaultProps.required,
  name,
  label,
  className = defaultProps.className,
  labelWidth,
  value = defaultProps.value,
  colSpan,
}: FormComponentModel) => {
  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      hideLabel={!label}
    >
      <p
        id={name}
        className={`min-h-10 rounded-lg border border-transparent bg-muted/20 px-3 py-2 text-sm text-foreground ${className ?? ""}`}
      >
        {value}
      </p>
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

export default LabelField;
