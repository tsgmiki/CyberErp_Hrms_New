import type AbstractModel from "../AbstractModel";

/**
 * A moment the system can notify on (Hrms.NotificationEvent). SEEDED, never user-created — an
 * administrator picks from this list rather than typing an event key the code never raises.
 */
export interface NotificationEventModel {
  id: string;
  eventKey: string;
  name: string;
  category: string;
  description?: string | null;
  /** Merge tokens this event publishes — rendered as the editor's clickable token palette. */
  tokens: string[];
  /** True when the event fires inside a workflow, so step scoping is meaningful. */
  isWorkflowEvent: boolean;
}

/**
 * One recipient RULE. Stores INTENT, not an address, so the routing keeps working when people
 * change role, manager or team.
 */
export interface NotificationRecipientModel {
  id?: string;
  /** Requester | CurrentApprover | RequesterManager | Role | OrganizationUnit | Employee | AllEmployees | Address | EventSubject */
  kind: string;
  /** The role / unit / employee this rule points at — only for those kinds. */
  targetId?: string | null;
  /** A literal address, only for the Address kind. */
  address?: string | null;
  /** To | Cc | Bcc */
  delivery: string;
  isActive: boolean;
}

/** The administrator's subject + body for one event, with its recipient rules. */
export default interface NotificationTemplateModel extends AbstractModel {
  notificationEventId?: string;
  eventKey?: string;
  /** Read-only display name resolved from the catalogue. */
  eventName?: string;
  name?: string;
  subject?: string;
  /** HTML with {{Token}} merge fields — same syntax as a document template. */
  body?: string;
  /** Email | Portal | Both */
  channel?: string;
  /** Narrows the template to one workflow; blank = every workflow. */
  workflowDefinitionId?: string | null;
  /** Narrows it to one step of that workflow; blank = every step. */
  stepOrder?: number | null;
  isActive?: boolean;
  recipients?: NotificationRecipientModel[];
}
