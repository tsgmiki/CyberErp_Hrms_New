"use client";
import { memo, useState, useEffect } from "react";
import DropDownField from "@/components/ui/dropDownField";
import type { ParameterModel } from "@/models";
import Tooltip from "@/components/common/toolTip/tooltip";

interface Option {
  id: string | number;
  name: string;
}

interface EditableInputProps {
  value: any;
  type?: "text" | "number" | "currency" | "date" | "dropdown";
  displayValue?: string;
  options?: Option[];
  onChange: (value: any) => void;
  onSelect?: (item: any, value: any, record?: any) => void;
  onFocus?: () => void;
  param?: ParameterModel;
  paramValue?: (param: ParameterModel | undefined, item: any) => any;
  item?: any;
  setParam?: React.Dispatch<React.SetStateAction<ParameterModel>>;
  isLoading?: boolean;
  error?: string;
}

const EditableInput = memo(function EditableInput({
  value,
  type = "text",
  options = [],
  displayValue,
  param,
  paramValue,
  item,
  setParam,
  isLoading,
  onChange,
  onSelect,
  onFocus,
  error,
}: EditableInputProps) {
  // Local state for text inputs to prevent vibration during typing
  const [localValue, setLocalValue] = useState(value ?? "");

  const inputBaseClass = " h-10 mt-[-3px] bg-background border-b border-border text-foreground placeholder:text-muted focus:border-primary focus:ring-0";
  
  // Sync local state when external value changes
  useEffect(() => {
    setLocalValue(value ?? "");
  }, [value]);

  // Compute effective param using paramValue callback if provided
  const effectiveParam = paramValue && item ? paramValue(param, item) : param;

  // Update parent state with computed param only when item changes
  useEffect(() => {
    if (paramValue && item && effectiveParam) {
      const currentCategoryId = param?.categoryId;
      const newCategoryId = effectiveParam?.categoryId;
      // Only update if categoryId changed
      if (currentCategoryId !== newCategoryId) {
        setParam?.(effectiveParam);
      }
    }
  }, [item?.id]);

  if (type === "dropdown") {
    const InputComponent = (
      <DropDownField
        name="editable_dropdown"
        label=""
        type="dropDown"
        value={value || ""}
        displayValue={displayValue}
        data={options}
        onSelect={(_name: string, record: any) => {
          const selectedValue = record?.id || record;
          onChange(selectedValue);
          onSelect?.(selectedValue, record);
        }}
        onFocus={onFocus}
        param={effectiveParam}
        setParam={setParam}
        isLoading={isLoading}
      />
    );
    if (error) {
      return (
        <Tooltip message={error}>
          <div className="border-2 border-red-500 rounded">{InputComponent}</div>
        </Tooltip>
      );
    }
    return InputComponent;
  }

  if (type === "number") {
    const errorClass = error ? "!border-red-500" : "";
    const borderThemeClass = error ? "" : "border-border";
    const InputComponent = (
      <input
        type="number"
        min="0"
        step="1"
        value={localValue}
        onChange={(e) => {
          setLocalValue(e.target.value);
        }}
        onBlur={() => {
          const newValue = parseInt(localValue) || 0;
          onChange(newValue);
        }}
        className={`${inputBaseClass} ${errorClass} w-full px-2 py-1.5 border rounded text-center text-sm ${borderThemeClass} focus:border-primary focus:ring-2 focus:ring-primary/50 transition-colors duration-200`}
      />
    );
    if (error) {
      return (
        <Tooltip message={error}>
          {InputComponent}
        </Tooltip>
      );
    }
    return InputComponent;
  }

  if (type === "currency") {
    const errorClass = error ? "!border-red-500" : "";
    const borderThemeClass = error ? "" : "border-border";
    const InputComponent = (
      <input
        type="number"
        min="0"
        step="0.01"
        value={localValue}
        onChange={(e) => {
          setLocalValue(e.target.value);
        }}
        onBlur={() => {
          const newValue = parseFloat(localValue) || 0;
          onChange(newValue);
        }}
        className={`${inputBaseClass} ${errorClass} w-full px-2 py-1.5 border rounded text-center text-sm ${borderThemeClass} focus:border-primary focus:ring-2 focus:ring-primary/50 transition-colors duration-200`}
      />
    );
    if (error) {
      return (
        <Tooltip message={error}>
          {InputComponent}
        </Tooltip>
      );
    }
    return InputComponent;
  }

  if (type === "date") {
    const errorClass = error ? "!border-red-500" : "";
    const borderThemeClass = error ? "" : "border-border";
    const InputComponent = (
      <input
        type="date"
        value={localValue}
        onChange={(e) => {
          setLocalValue(e.target.value);
        }}
        onBlur={() => {
          onChange(localValue);
        }}
        className={`${inputBaseClass} ${errorClass} w-full px-2 py-1.5 border rounded text-center text-sm ${borderThemeClass} focus:border-primary focus:ring-2 focus:ring-primary/50 transition-colors duration-200`}
      />
    );
    if (error) {
      return (
        <Tooltip message={error}>
          {InputComponent}
        </Tooltip>
      );
    }
    return InputComponent;
  }

  // Default text input - use local state to prevent vibration
  const errorClass = error ? "!border-red-500" : "";
  const borderThemeClass = error ? "" : "border-border";
  const InputComponent = (
    <input
      type="text"
      value={localValue}
      onChange={(e) => {
        setLocalValue(e.target.value);
      }}
      onBlur={() => {
        // Only trigger parent update on blur
        onChange(localValue);
      }}
      className={`${inputBaseClass} ${errorClass} w-full px-2 py-1.5 border rounded text-center text-sm ${borderThemeClass} focus:border-primary focus:ring-2 focus:ring-primary/50 transition-colors duration-200`}
    />
  );
  if (error) {
    return (
      <Tooltip message={error}>
        {InputComponent}
      </Tooltip>
    );
  }
  return InputComponent;
});

export default EditableInput;
