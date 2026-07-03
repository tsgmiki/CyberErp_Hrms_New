import type { FormComponentModel } from "@/models";
import { FieldShell } from "./fieldShell";
import {
  FORM_OPTION_GROUP_CLASS,
  FORM_OPTION_LABEL_CLASS,
  FORM_RADIO_CLASS,
} from "./fieldStyles";

const RadioField = ({
  error,
  required = defaultProps.required,
  name,
  label,
  labelWidth,
  value = defaultProps.value,
  onChange,
  data,
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
      hideLabel={!label}
    >
      <div className={FORM_OPTION_GROUP_CLASS} role="radiogroup" aria-labelledby={name}>
        {data?.map((item: { id: string | number; name: string }) => (
          <label key={String(item.id)} className={FORM_OPTION_LABEL_CLASS}>
            <input
              className={FORM_RADIO_CLASS}
              type="radio"
              value={item.id}
              checked={value == item.id}
              onChange={onChange}
              name={name}
            />
            <span>{item.name}</span>
          </label>
        ))}
      </div>
    </FieldShell>
  );
};

const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  className: "",
};

export default RadioField;
