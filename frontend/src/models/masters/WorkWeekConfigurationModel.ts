import type AbstractModel from "../AbstractModel";

/**
 * Per-tenant work-week pattern (weekend configuration). Each weekday is a work value —
 * Full / Half / Rest — that the leave & attendance calendar uses to count actual days.
 * Exactly one configuration is active per tenant at a time.
 */
export default interface WorkWeekConfigurationModel extends AbstractModel {
  name?: string;
  monday?: string;
  tuesday?: string;
  wednesday?: string;
  thursday?: string;
  friday?: string;
  saturday?: string;
  sunday?: string;
  isActive?: boolean;
}
