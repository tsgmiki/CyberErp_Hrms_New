import type AbstractModel from "../AbstractModel";

/** Admin-configured, reusable HR document template (HC022 correspondence). */
export default interface DocumentTemplateModel extends AbstractModel {
  name?: string;
  documentType?: string; // EmploymentLetter | ExperienceLetter | IdCard | Other
  headerHtml?: string; // optional letterhead (supports {{Logo}} + tokens)
  body?: string; // HTML with {{Placeholder}} merge tokens
  footerHtml?: string; // optional footer
  description?: string;
  isActive?: boolean;
}

/** A merge token available to a template author (from GET DocumentTemplate/merge-fields). */
export interface MergeFieldModel {
  token: string; // e.g. {{FullName}}
  label: string;
  group: string;
}

/** A rendered, print-ready document for one employee (from the generate endpoint). */
export interface GeneratedDocumentModel {
  title: string;
  html: string;
}
