import InputField from "@/components/ui/inputField";
import DropDownField from "@/components/ui/dropDownField";
import type { FormComponentModel } from "@/models";

function FormUtility(props: { component: FormComponentModel }) {
  const { component } = props;
  return (
    <>
      {component.type == "text" && (
        <InputField
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
        ></InputField>
      )}
      {component.type == "dropDown" && (
        <DropDownField
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
          data={component.data}
          displayValue={component.displayValue}
          param={component.param}
          isLoading={component.isLoading}
          showAdd={component.showAdd}
          setParam={component.setParam}
          onChange={component.onChange}
          onSelect={component.onSelect}
          onAdd={component.onAdd}
        ></DropDownField>
      )}
    </>
  );
}
export default FormUtility;
