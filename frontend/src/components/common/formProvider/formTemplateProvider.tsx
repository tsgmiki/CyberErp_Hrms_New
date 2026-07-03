import type { FormComponentModel, FormModel } from "@/models";
import { forwardRef, type ReactNode } from "react";
import Loading from "../loader/loader";
import FormTemplateUtility from "./formTemplateUtility";

const FormTemplateProvider = forwardRef<
  HTMLFormElement,
  { form: FormModel; children?: ReactNode }
>((props, ref) => {
  return (
    <>
      {props.form.isPending && <Loading />}

      <form ref={ref} className="rounded-lg shadow-sm bg-card border-border m-1 p-2">
        <div
          className={`grid p-2 ${
            props.form.columnsNo == 1
              ? "grid-cols-1"
              : props.form.columnsNo == 2
              ? "grid-cols-2"
              : props.form.columnsNo == 3
              ? "grid-cols-3"
              : ""
          } max-md:grid-cols-1 gap-2`}
        >
          {props.form.components?.map((formColumn: FormComponentModel) => {
            return FormTemplateUtility({
              component: { ...formColumn, labelWidth: props.form.labelWidth },
            });
          })}
        </div>
      </form>
      {props.children}
    </>
  );
});
FormTemplateProvider.displayName = "FormProviders";
export default FormTemplateProvider;
