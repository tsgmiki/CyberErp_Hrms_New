import type AbstractModel from "../AbstractModel";

/** A field (column) of a user-defined dynamic form (reuses the HC021 data types). */
export interface DynamicFormFieldModel {
  id?: string;
  name?: string;
  label?: string;
  dataType?: string; // Text | Number | Date | Boolean | Select
  options?: string; // comma-separated for Select
  isRequired?: boolean;
  isActive?: boolean;
  sortOrder?: number;
  /** Whether the field appears as a column in the tab's list grid. */
  showInList?: boolean;
}

/** A user-defined dynamic form / custom tab (the "Form Builder" output). */
export default interface DynamicFormModel extends AbstractModel {
  module?: string; // e.g. "Employee"
  name?: string;
  label?: string;
  description?: string;
  icon?: string;
  isActive?: boolean;
  sortOrder?: number;
  fields?: DynamicFormFieldModel[];
}

/** One data row of a dynamic form for an owner (e.g. an employee). Values live in `data`. */
export interface DynamicFormRecordModel {
  id?: string;
  dynamicFormId?: string;
  ownerType?: string;
  ownerId?: string;
  data?: Record<string, string | null>;
  /** Attached-file counts keyed by Attachment field name (each field is a separate pool). */
  documentCounts?: Record<string, number>;
}
