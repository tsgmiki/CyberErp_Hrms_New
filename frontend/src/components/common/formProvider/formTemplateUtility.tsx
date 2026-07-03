import BreakField from "@/components/ui/breakField";
import type { FormComponentModel } from "@/models";
import FileUploadField from "../../ui/fileUploadField";
import LabelField from "@/components/ui/labelField";

function FormTemplateUtility(props: { component: FormComponentModel }) {
  const { component } = props;

  return (
    <div
      key={component.name}
      className={`${component.type == "break" && "col-span-full"} ${
        component.type == "hidden" && "w-0"
      } ${component.colSpan == "full" && "col-span-full"} bg-secondary`}
    >
      {component.type == "text" && (
        <LabelField
          type={component.type}
          key={component.name}
          label={component.label}
          name={component.name}
          className={component.className}
          placeholder={component.placeholder}
          labelWidth={component.labelWidth}
          value={component.value}
        ></LabelField>
      )}

      {component.type == "break" && (
        <BreakField
          key={component.label}
          type={component.type}
          label={component.label}
          className={component.className}
          labelWidth={component.labelWidth}
        ></BreakField>
      )}

      {component.type == "empty" && <></>}
      {component.type == "hidden" && (
        <input
          key={component.name}
          hidden
          name={component.name}
          defaultValue={component.value}
        />
      )}
      {component.type == "file" && (
        <FileUploadField
          type={component.type}
          key={component.name}
          label={component.label}
          maxLength={component.maxLength}
          error={component.error}
          name={component.name}
          className={component.className}
          placeholder={component.placeholder}
          disabled={component.disabled}
          inputType={component.inputType}
          required={component.required}
          labelWidth={component.labelWidth}
          value={component.value}
          onChange={component.onChange}
        ></FileUploadField>
      )}
    </div>
  );
}
export default FormTemplateUtility;
