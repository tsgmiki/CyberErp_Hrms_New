import type { ReactNode } from "react";
import type { SaveResult } from "@/template/createSaveService";

/**
 * Module-specific plumbing for a shared person-background section (Education / Experience). The
 * Employee and Candidate modules render the SAME section component and only differ in this adapter,
 * so the form fields, columns, custom fields and UI can never drift between the two.
 */
export interface BackgroundDataSource<T> {
  /** Employee id or candidate id — used as the react-query owner + passed to the attachments panel. */
  ownerId: string;
  /** React-query key for the record list (invalidated after a save/delete). */
  queryKey: unknown[];
  /** Fetch the records for this owner. */
  list: () => Promise<T[]>;
  /** Persist a record from the form's native FormData (create/update). */
  save: (fd: FormData) => Promise<SaveResult>;
  /** Delete a record by id. */
  remove: (id: string) => Promise<unknown>;
  /** Hidden owner-id field injected into the form (Employee needs `employeeId`; Candidate: none). */
  ownerIdField?: { name: string; value: string };
  /** Module-specific attachments panel for one saved record (edit mode). */
  renderAttachments: (recordId: string) => ReactNode;
  /** View-only (e.g. an internal candidate's records live on the employee master). */
  readOnly?: boolean;
  hint?: string;
}
