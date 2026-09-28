/**
 * An allowance or benefit that belongs to a POST rather than to whoever currently holds it.
 *
 * Read when somebody ACTS in the post: a deputy covering it receives these alongside its salary.
 * Defining one does not enrol the post's current holder — it is a statement about the job.
 */
export default interface PositionEntitlementModel {
  id?: string;
  positionClassId?: string;
  positionClassTitle?: string;
  /** "Allowance" | "BenefitPlan" — decides which reference below is set. */
  kind?: string;
  allowanceTypeId?: string | null;
  benefitPlanId?: string | null;
  /** The catalogue row's name, whichever side it is on. */
  referenceName?: string;
  /** The post's own figure. null = fall back to the catalogue's default rate. */
  value?: number | null;
  /** Shown when the entitlement names no value of its own. */
  defaultRate?: number | null;
  /** "Fixed" | "PercentOfBase" — decides how the value reads. */
  calcMethod?: string | null;
  /** Whether somebody ACTING in the post receives this. */
  grantedWhenActing?: boolean;
  isActive?: boolean;
  notes?: string | null;
}
