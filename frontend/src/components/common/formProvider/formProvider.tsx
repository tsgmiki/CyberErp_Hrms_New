import type { FormComponentModel, FormModel } from "@/models";
import FormFieldRenderer from "./formUtility";
import { forwardRef, type ReactNode, type Ref } from "react";
import Loading from "../loader/loader";
import AddButton from "./addButton";
import ActionBar from "./actionBar";
import SubmitButton from "./submitButton";
import { useFormPermissions } from "./formPermissions";
import { getFormGridClass } from "./formLayout";
import { useTranslation } from "react-i18next";
import Modal from "../modal";
import FormFooterActions, { hasFormFooterActions } from "./formFooterActions";

function FormActions({
  form,
  placement,
  finalDisable,
  disableValidate,
}: {
  form: FormModel;
  placement: "top" | "bottom";
  finalDisable: boolean;
  disableValidate: boolean;
}) {
  const showAt =
    (!form.SubmitButton && placement === "top") ||
    form.SubmitButton === placement;

  if (!showAt) return null;

  if (form.submitBtnType === "Add") {
    return (
      <AddButton
        disable={finalDisable}
        formId={form.formId ?? "formProvider"}
        title={form.submitBtnTitle as string}
      />
    );
  }

  if (placement === "top" && form.SubmitButton === "top") {
    return (
      <ActionBar
        showValidate={form.showValidate}
        showCancel={form.showCancel}
        validateLabel={form.validateLabel}
        cancelLabel={form.cancelLabel}
        onValidate={form.onValidate}
        onCancel={form.onCancel}
        validating={form.validating}
        disabled={finalDisable}
        disableValidate={disableValidate}
        status={form.status}
        className={form.submitBtnClassName}
        label={form.submitBtnTitle ?? "Save"}
        formId={form.formId ?? "formProvider"}
      />
    );
  }

  if (placement === "bottom" && form.SubmitButton === "bottom") {
    return (
      <SubmitButton
        disabled={form.disableSubmitBtn || finalDisable}
        loading={form.isPending}
        className={form.submitBtnClassName}
        label={form.submitBtnTitle ?? "Submit"}
        formId={form.formId ?? "formProvider"}
        showLockIcon={form.showLockOnSubmit}
      />
    );
  }

  return null;
}

interface FormBodyProps {
  form: FormModel;
  children?: ReactNode;
  inModal?: boolean;
  formRef?: Ref<HTMLFormElement>;
}

function FormBody({ form, children, inModal, formRef }: FormBodyProps) {
  const { t } = useTranslation();
  const { finalDisable, disableValidate } = useFormPermissions(
    form.voucherType,
    form.status,
  );

  const formId = form.formId ?? "formProvider";
  const compact = form.compact;
  const gridClass = getFormGridClass(form.columnsNo ?? 1, compact);

  if (form.authMode) {
    return (
      <>
        {form.isPending ? (
          <div className="relative">
            <Loading />
          </div>
        ) : null}
        <form
          ref={formRef}
          id={formId}
          className="w-full space-y-5"
          onSubmit={form.submitHandler}
        >
          <div
            className={`rounded-xl border border-border bg-card p-6 shadow-sm ${
              inModal ? "border-0 p-0 shadow-none" : ""
            }`}
          >
            <div
              className={`grid gap-4 ${
                form.columnsNo === 2 ? "grid-cols-1 sm:grid-cols-2" : "grid-cols-1"
              }`}
            >
              {form.components?.map((formColumn: FormComponentModel) => (
                <FormFieldRenderer
                  key={formColumn.name}
                  component={{
                    ...formColumn,
                    labelWidth: form.labelWidth,
                    layout: formColumn.layout ?? "auth",
                  }}
                />
              ))}
            </div>
          </div>

          {form.SubmitButton === "bottom" && !inModal && (
            <SubmitButton
              disabled={form.disableSubmitBtn || form.isPending}
              loading={form.isPending}
              className={form.submitBtnClassName}
              label={form.submitBtnTitle ?? t("Submit")}
              formId={formId}
              showLockIcon={form.showLockOnSubmit}
            />
          )}
        </form>
        {children}
      </>
    );
  }

  const showTopActions =
    !inModal &&
    (!form.SubmitButton || form.SubmitButton === "top") &&
    (form.submitBtnType === "Add" || form.SubmitButton === "top");

  const showHeader =
    (form.title && !inModal) ||
    (form.description && !inModal) ||
    showTopActions;

  const showInlineFooter =
    !inModal &&
    form.SubmitButton === "bottom" &&
    (form.submitBtnType === "Add" || form.SubmitButton === "bottom");

  return (
    <article
      className={`relative overflow-hidden bg-card ${
        inModal
          ? ""
          : `rounded-xl border border-border shadow-sm ${compact ? "" : "m-4"}`
      }`}
    >
      {form.isPending ? (
        <div className="absolute inset-0 z-10 flex items-center justify-center bg-card/60 backdrop-blur-[1px]">
          <Loading />
        </div>
      ) : null}

      {showHeader ? (
        <header className="flex flex-wrap items-start justify-between gap-3 border-b border-border bg-muted/20 px-4 py-3">
          <div className="min-w-0">
            {form.title && !inModal ? (
              <h2 className="text-base font-semibold text-foreground">
                {t(form.title)}
              </h2>
            ) : null}
            {form.description && !inModal ? (
              <p
                className={`text-xs text-muted ${form.title && !inModal ? "mt-0.5" : ""}`}
              >
                {t(form.description)}
              </p>
            ) : null}
          </div>
          {showTopActions ? (
            <div className="flex shrink-0 items-center gap-2">
              <FormActions
                form={form}
                placement="top"
                finalDisable={finalDisable}
                disableValidate={disableValidate}
              />
            </div>
          ) : null}
        </header>
      ) : null}

      <form ref={formRef} id={formId} onSubmit={form.submitHandler}>
        <div className={inModal ? `${gridClass} !p-0` : gridClass}>
          {form.components?.map((formColumn: FormComponentModel) => (
            <FormFieldRenderer
              key={formColumn.name}
              component={{
                ...formColumn,
                labelWidth: form.labelWidth ?? formColumn.labelWidth,
              }}
            />
          ))}
        </div>
      </form>

      {children ? (
        <section
          className={`rounded-lg border border-border bg-card ${
            inModal ? "mt-4" : "m-4 mt-0"
          }`}
        >
          {children}
        </section>
      ) : null}

      {showInlineFooter ? (
        <footer className="flex justify-end gap-2 border-t border-border bg-muted/20 px-4 py-3">
          <FormActions
            form={form}
            placement="bottom"
            finalDisable={finalDisable}
            disableValidate={disableValidate}
          />
        </footer>
      ) : null}
    </article>
  );
}

const FormProviders = forwardRef<HTMLFormElement, { form: FormModel; children?: ReactNode }>(
  (props, ref) => {
    const { t } = useTranslation();
    const { form, children } = props;

    const inModal = Boolean(form.showModal);
    const modalOpen = form.modalVisible !== false;

    if (inModal && !modalOpen) {
      return null;
    }

    const body = (
      <FormBody form={form} inModal={inModal} formRef={ref}>
        {children}
      </FormBody>
    );

    if (!inModal) {
      return body;
    }

    const modalTitle = form.modalTitle ?? form.title ?? "";
    const modalDescription = form.description ? t(form.description) : undefined;
    const modalFooter = hasFormFooterActions(form) ? (
      <FormFooterActions form={form} />
    ) : null;

    return (
      <Modal
        visible={modalOpen}
        title={modalTitle ? t(modalTitle) : ""}
        description={modalDescription}
        size={form.modalSize ?? "lg"}
        onClose={() => form.onModalClose?.()}
        footer={modalFooter}
      >
        {body}
      </Modal>
    );
  },
);

FormProviders.displayName = "FormProviders";
export default FormProviders;
