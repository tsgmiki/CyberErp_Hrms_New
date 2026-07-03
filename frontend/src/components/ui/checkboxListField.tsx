import type { FormComponentModel } from "@/models";
import { useEffect, useRef, useState } from "react";
import { ZodErrors } from "../common/statusMessage/status";

const CheckboxListField = ({
  error,
  maxLength,
  required = defaultProps.required,
  name,
  label,
  inputType,
  disabled,
  labelWidth,
  value = defaultProps.value,
  data,
  onSelect,
}: FormComponentModel) => {
  const [currentValues, setCurrentValues] = useState(
    [] as {
      id: string;
      name: string;
    }[]
  );

  const input = useRef<HTMLInputElement>(null);

  // Unified theme classes
  const labelClass = "text-foreground";
  const checkboxItemClass = "border border-border hover:bg-secondary text-foreground";
  const checkboxClass = "accent-primary rounded border-border";

  useEffect(() => {
    if (typeof value == "undefined" || value == "" || value == null)
      setCurrentValues([]);
    else {
      const idList = (value as string)?.split(",");
      const valueList = data
        ?.filter((a: any) => idList.filter((b) => b == a.id).length > 0)
        .map((item: any) => {
          return { id: item.id, name: item.name };
        });
      setCurrentValues(valueList as never);
    }
  }, [value, data]);

  return (
    <div className="md:inline-flex gap-1 w-full">
      <label
        className={`${labelClass} font-medium text-sm col-span-1 max-md:w-full text-end flex items-center justify-end gap-0.5 ${
          typeof labelWidth != "undefined" ? labelWidth : "w-[20%]"
        }`}
      >
        {label}
        <span className={required ? "text-error text-lg leading-none" : "text-transparent text-lg leading-none"}>*</span>
      </label>
      <div className="w-full">
          <input
            ref={input}
            className="w-0"
            name={name}
            id={name}
            type={inputType}
            disabled={disabled}
            maxLength={maxLength}
            defaultValue={value}
          />

        {data?.map((item: any) => (
          <div
            key={item.id}
            className={`inline-flex gap-2 border-b m-1 pl-2 transition-colors ${checkboxItemClass}`}
          >
            <input
              type="checkbox"
              className={checkboxClass}
              checked={currentValues?.filter((a) => a.id == item.id).length > 0}
              onChange={(e) => {
                let valueList = currentValues?.filter((a) => a.id != item.id);
                if (e.target.checked == true)
                  valueList.push({ id: item.id, name: item.name });
                if(valueList.filter(a=>a.name=='None')?.length>0)
                {
                  valueList=[{ id: item.id, name: item.name }];
                }
                const newvalues = valueList.map((a) => a.id).join(",");
                if (input.current != null) input.current.value = newvalues;
                setCurrentValues(valueList);
                onSelect?.(name as string, newvalues);
              }}
            />

            <span className="text-sm">{item.name}</span>
          </div>
        ))}
        <span className="pb-1 block flex-none">
          <ZodErrors error={error} />
        </span>
      </div>
    </div>
  );
};
const defaultProps = {
  value: "",
  disabled: false,
  required: false,
  className: "",
};

export default CheckboxListField;
