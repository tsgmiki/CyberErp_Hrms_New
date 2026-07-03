import type { FormComponentModel } from "@/models";
import { useTranslation } from "react-i18next";
import { FieldShell } from "./fieldShell";
import {
  FORM_CHECKBOX_CLASS,
  FORM_OPTION_GROUP_CLASS,
  FORM_OPTION_LABEL_CLASS,
} from "./fieldStyles";

const CheckBoxField = ({
  error,
  required = defaultProps.required,
  name,
  label,
  className = defaultProps.className,
  labelWidth,
  value = defaultProps.value,
  onChange,
  data,
  colSpan,
}: FormComponentModel) => {
  const { t } = useTranslation();

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
      {!data || data.length === 0 ? (
        <label className={FORM_OPTION_LABEL_CLASS}>
          <input
            className={className || FORM_CHECKBOX_CLASS}
            type="checkbox"
            checked={!!value}
            onChange={onChange}
            name={name}
            id={name}
          />
          <span>{label ? t(label) : t("Enabled")}</span>
        </label>
      ) : (
        <div className={FORM_OPTION_GROUP_CLASS}>
          {data.map((item: { id: string | number; name: string }) => (
            <label key={String(item.id)} className={FORM_OPTION_LABEL_CLASS}>
              <input
                className={FORM_CHECKBOX_CLASS}
                type="checkbox"
                value={item.id}
                checked={value == item.id}
                onChange={onChange}
                name={name}
              />
              <span>{item.name}</span>
            </label>
          ))}
        </div>
      )}
    </FieldShell>
  );
};

const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  className: "",
};

export default CheckBoxField;
