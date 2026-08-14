import type AbstractModel from "../AbstractModel";

/** Master list of ERP subsystems (dbo.coreSubsystem); modules reference one by name. */
export default interface SubsystemModel extends AbstractModel {
  name?: string;
  code?: string;
  /** lucide-react icon name (Core.Subsystem.Icon) — resolved through lucideIconMap. */
  icon?: string | null;
  sortOrder?: number;
  /** Where the subsystem's application lives — launchers/landing pages deep-link here. */
  url?: string;
}
