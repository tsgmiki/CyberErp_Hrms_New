import BreakField from "@/components/ui/breakField";
import DropDownField from "@/components/ui/dropDownField";
import InputField from "@/components/ui/inputField";
import RadioField from "@/components/ui/radioField";
import SelectField from "@/components/ui/selectField";
import TextreaField from "@/components/ui/textreaField";
import type { ReactNode } from "react";
import type { FormComponentModel } from "@/models";
import FileUploadField from "../../ui/fileUploadField";
import CheckboxDropDownField from "../../ui/checkboxListField";
import MultiSelectField from "../../ui/multiSelectField";
import CheckBoxField from "../../ui/checkBoxField";
import DateField from "../../ui/dateField";
import CustomField from "@/components/ui/customField";
import HtmlEditorField from "@/components/ui/htmlEditorField";
import FormSection from "./formSection";
import { FORM_INPUT_CLASS, getFieldCellClass } from "./formLayout";

function mergeClass(...parts: Array<string | false | undefined>) {
  return parts.filter(Boolean).join(" ");
}

function withInputClass(component: FormComponentModel) {
  return mergeClass(
    FORM_INPUT_CLASS,
    component.labelWidth === "full" ? "w-full" : "",
    component.className,
  );
}

function FormFieldRenderer({ component }: { component: FormComponentModel }) {
  const cellClass = getFieldCellClass(component);
  const fieldClass = withInputClass(component);

  if (component.type === "break") {
    return (
      <div className={cellClass}>
        {component.sectionDescription ? (
          <FormSection
            title={component.label ?? ""}
            description={component.sectionDescription}
          />
        ) : (
          <BreakField
            type="break"
            label={component.label}
            className={component.className}
            labelWidth={component.labelWidth}
          />
        )}
      </div>
    );
  }

  if (component.type === "empty") {
    return <div className={cellClass} />;
  }

  if (component.type === "hidden" || component.type === "label") {
    return (
      <div className={cellClass}>
        <input
          hidden
          name={component.name}
          defaultValue={component.value}
          value={component.value}
        />
      </div>
    );
  }

  const common = {
    key: component.name,
    name: component.name,
    label: component.label,
    maxLength: component.maxLength,
    error: component.error,
    className: fieldClass,
    placeholder: component.placeholder,
    disabled: component.disabled,
    inputType: component.inputType,
    required: component.required,
    labelWidth: component.labelWidth,
    colSpan: component.colSpan,
    layout: component.layout,
    floatingLabel: component.floatingLabel,
    value: component.value,
    onChange: component.onChange,
    onBlur: component.onBlur,
  };

  let field: ReactNode;

  switch (component.type) {
    case "text":
    case "password":
      field = (
        <InputField
          {...common}
          type={component.type}
          showPasswordToggle={component.showPasswordToggle}
        />
      );
      break;
    case "date":
      field = <DateField {...common} type="date" />;
      break;
    case "select":
      field = (
        <SelectField
          {...common}
          type="select"
          data={component.data}
          param={component.param}
          setParam={component.setParam}
        />
      );
      break;
    case "radio":
      field = (
        <RadioField {...common} type="radio" data={component.data} />
      );
      break;
    case "dropDown":
      field = (
        <DropDownField
          {...common}
          type="dropDown"
          data={component.data}
          displayValue={component.displayValue}
          param={component.param}
          isLoading={component.isLoading}
          showAdd={component.showAdd}
          setParam={component.setParam}
          onSelect={component.onSelect}
          onAdd={component.onAdd}
        />
      );
      break;
    case "checkboxListField":
      field = (
        <CheckboxDropDownField
          {...common}
          type="checkboxListField"
          data={component.data}
          displayValue={component.displayValue}
          param={component.param}
          setParam={component.setParam}
          onSelect={component.onSelect}
        />
      );
      break;
    case "multiSelectField":
      // The standard multi-select COMBOBOX (searchable dropdown + chips). Same comma-id value
      // contract as checkboxListField, so it's a drop-in for multi-select parameters.
      field = (
        <MultiSelectField
          {...common}
          type="multiSelectField"
          data={component.data}
          onSelect={component.onSelect}
        />
      );
      break;
    case "checkbox":
      field = (
        <CheckBoxField {...common} type="checkbox" data={component.data} />
      );
      break;
    case "textarea":
      field = (
        <TextreaField
          {...common}
          type="textarea"
          rowNo={component.rowNo}
        />
      );
      break;
    case "file":
      field = <FileUploadField {...common} type="file" />;
      break;
    case "custom":
      field = (
        <CustomField
          {...common}
          type="custom"
          customChildren={component.customChildren}
          showAdd={component.showAdd}
          onAdd={component.onAdd}
          addTitle={component.addTitle}
        />
      );
      break;
    case "editor":
      field = (
        <HtmlEditorField
          {...common}
          type="editor"
          onHtmlChange={component.onHtmlChange}
        />
      );
      break;
    default:
      return null;
  }

  return <div className={cellClass}>{field}</div>;
}

export default FormFieldRenderer;

export { FormFieldRenderer as FormUtility };
