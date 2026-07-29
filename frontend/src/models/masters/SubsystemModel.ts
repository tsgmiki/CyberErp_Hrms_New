import type AbstractModel from "../AbstractModel";

/** Master list of ERP subsystems (dbo.coreSubsystem); modules reference one by name. */
export default interface SubsystemModel extends AbstractModel {
  name?: string;
  code?: string;
  sortOrder?: number;
  /** Where the subsystem's application lives — launchers/landing pages deep-link here. */
  url?: string;
}
