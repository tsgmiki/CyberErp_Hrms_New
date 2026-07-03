import type { FormModel } from "@/models";
import AddButton from "./addButton";
import ActionBar from "./actionBar";
import SubmitButton from "./submitButton";
import { useFormPermissions } from "./formPermissions";

interface FormFooterActionsProps {
  form: FormModel;
}

/** Renders submit / validate / cancel controls for modal footers and inline footers. */
export default function FormFooterActions({ form }: FormFooterActionsProps) {
  const { finalDisable, disableValidate } = useFormPermissions(
    form.voucherType,
    form.status,
  );

  const formId = form.formId ?? "formProvider";
  const cancelHandler = form.onCancel ?? form.onModalClose;

  if (form.submitBtnType === "Add") {
    return (
      <AddButton
        disable={finalDisable}
        formId={formId}
        title={form.submitBtnTitle as string}
      />
    );
  }

  if (form.SubmitButton === "bottom") {
    return (
      <SubmitButton
        disabled={form.disableSubmitBtn || finalDisable}
        loading={form.isPending}
        className={form.submitBtnClassName}
        label={form.submitBtnTitle ?? "Submit"}
        formId={formId}
        showLockIcon={form.showLockOnSubmit}
      />
    );
  }

  return (
    <ActionBar
      showValidate={form.showValidate}
      showCancel={form.showCancel ?? Boolean(form.onModalClose)}
      validateLabel={form.validateLabel}
      cancelLabel={form.cancelLabel}
      onValidate={form.onValidate}
      onCancel={cancelHandler}
      validating={form.validating}
      disabled={finalDisable}
      disableValidate={disableValidate}
      status={form.status}
      className={form.submitBtnClassName}
      label={form.submitBtnTitle ?? "Save"}
      formId={formId}
    />
  );
}

export function hasFormFooterActions(form: FormModel): boolean {
  if (form.authMode) {
    return form.SubmitButton === "bottom";
  }

  return (
    form.submitBtnType === "Add" ||
    form.SubmitButton === "top" ||
    form.SubmitButton === "bottom" ||
    !form.SubmitButton
  );
}
