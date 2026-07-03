import React from "react";
import type ParameterModel from "./ParameterModel";

export default interface FormComponentModel {
  name?: string;
  label?: string;
  value?: any;
  type:
    | "text"
    | "select"
    | "break"
    | "radio"
    | "dropDown"
    | "textarea"
    | "empty"
    | "hidden"
    | "file"
    | "checkboxListField"
    | "checkbox"
    | "date"
    | "label"
    | "custom"
    | "password"
    | "editor";
  inputType?: string;
  disabled?: boolean;
  hidden?: boolean;
  placeholder?: string;
  required?: boolean;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  onFocus?: () => void;
  onSelect?: (name: string, record: any) => void;
  onKeyDown?: (event: React.KeyboardEvent<HTMLInputElement>) => void;
  onAdd?: () => void;
  onBlur?: (event: React.FocusEvent<HTMLInputElement>) => void;
  onHtmlChange?: (html: string) => void;
  onSearch?: (name: string, text: string, operator?: string) => void;

  error?: any;
  className?: string;
  maxLength?: number;
  labelWidth?: string;
  data?: any[];
  rowNo?: number;
  colSpan?: "full";
  displayValue?: string;
  param?: ParameterModel;
  setParam?: (param: ParameterModel) => void;
  isLoading?: boolean;
  showAdd?: boolean;
  customChildren?: React.ReactNode;
  addTitle?: string;
  count?: number;
  /** Stack label above input (auth screens) */
  layout?: "default" | "auth" | "toolbar" | "stack";
  showPasswordToggle?: boolean;
  /** Optional subtitle for `break` fields (FormSection layout) */
  sectionDescription?: string;
}
