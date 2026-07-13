import type { MouseEventHandler, ReactNode } from "react";
import type FormComponentModel from "./FormComponentModel";

export default interface FormModel {
  components?: FormComponentModel[];
  initialValues?: {};
  columnsNo?: number;
  labelWidth?: string;
  /** Applies a field layout to every field (e.g. "auth" = a clean label-above-input style). */
  fieldLayout?: FormComponentModel["layout"];
  submitHandler?: any;
  otherComponent?: ReactNode;
  method?: any;
  submitBtnTitle?: string;
  submitBtnClassName?: string;
  disableSubmitBtn?: boolean;
  children?: ReactNode;
  ref?: any;
  submitBtnType?: "Add" | "Submit";
  addHandler?: MouseEventHandler<HTMLAnchorElement>;
  formId?: string;
  isPending?: boolean;
  status?: string;
  step?: number;
  showValidate?: boolean;
  showCancel?: boolean;
  validateLabel?: string;
  cancelLabel?: string;
  onValidate?: () => void;
  onCancel?: () => void;
  validating?: boolean;
  SubmitButton?: "top" | "bottom";
  voucherType?: string;
  /** Tighter layout for auth and other small forms */
  compact?: boolean;
  /** ERP-style login/register layout (card fields + full-width submit) */
  authMode?: boolean;
  showLockOnSubmit?: boolean;
  /** Optional form header inside the card */
  title?: string;
  description?: string;
  /** When true, form renders inside a modal overlay instead of inline page content */
  showModal?: boolean;
  /** Controls modal open state (defaults to true when the form is mounted) */
  modalVisible?: boolean;
  /** Modal title; falls back to `title` */
  modalTitle?: string;
  /** Modal width preset */
  modalSize?: "sm" | "md" | "lg" | "xl" | "fullscreen";
  /** Called when the modal is closed (backdrop, Escape, or close button) */
  onModalClose?: () => void;
}
