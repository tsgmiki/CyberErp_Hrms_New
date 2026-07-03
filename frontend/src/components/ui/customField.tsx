import type { FormComponentModel } from "@/models";
import { useEffect, useRef, useState } from "react";
import { SquarePlus } from "lucide-react";
import { ZodErrors } from "../common/statusMessage/status";
import { useTranslation } from "react-i18next";

const CustomField = ({
  error,
  required = defaultProps.required,
  label,
  labelWidth,
  value = defaultProps.value,
  displayValue,
  showAdd,
  onAdd,
  customChildren,
  addTitle = "Add",
}: FormComponentModel) => {
  const [open, setOpen] = useState(false);
  const [currentValue, setCurrentValue] = useState({
    id: value,
    name: displayValue,
  });
  const dropdown = useRef(null);
  const { t } = useTranslation();

  // Unified theme classes
  const labelClass = "text-foreground";
  const addButtonClass = "bg-primary text-on-accent border-primary hover:bg-primary-hover";

  useEffect(() => {
    if (typeof displayValue != "undefined")
      setCurrentValue({ id: currentValue.id, name: displayValue });
  }, [displayValue]);
  useEffect(() => {
    if (typeof value == "undefined" || value == "")
      setCurrentValue({ id: "", name: displayValue });
  }, [value]);

  useEffect(() => {
    const clickHandler = (pros: { target: any }) => {
      if (
        !open ||
        (typeof dropdown != "undefined" &&
          dropdown != null &&
          typeof dropdown?.current != "undefined" &&
          dropdown?.current != null &&
          (dropdown?.current as any).contains(pros.target))
      )
        return;

      setOpen(false);
    };
    document.addEventListener("click", clickHandler);
    return () => document.removeEventListener("click", clickHandler);
  });
  return (
    <div className="md:inline-flex gap-1 w-full">
      {label && (
        <label
          className={`${labelClass} font-medium text-sm col-span-1 max-md:w-full text-end flex items-center justify-end gap-0.5 ${
            typeof labelWidth != "undefined" ? labelWidth : "w-[20%]"
          }`}
        >
          {t(label as string)}
          <span className={required ? "text-error text-lg leading-none" : "text-transparent text-lg leading-none"}>*</span>
        </label>
      )}
      <div className="w-full gap-2">
        <div className="inline-flex w-full">
          {showAdd && (
            <a
              className={`${addButtonClass} inline-flex cursor-pointer h-[33.5px] p-1 rounded-tr-[5px] rounded-br-[5px] border-l-0 border transition-colors items-center`}
              onClick={() => onAdd?.()}
            >
              {<SquarePlus />} {addTitle}
            </a>
          )}
        </div>
        <div ref={dropdown} className="w-full">
          {customChildren}
        </div>
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

export default CustomField;
