import type AbstractModel from "../AbstractModel";

/** Master list of ERP subsystems (dbo.coreSubsystem); modules reference one by name. */
export default interface SubsystemModel extends AbstractModel {
  name?: string;
  /**
   * The subsystem's stable short identifier (SSMS, HRMS, SAMS, SRMS) — Core.Subsystem.Abbreviation.
   * THE key to match on: the launcher's HOME exclusion and the app-URL registry.
   *
   * Replaced `code` on 2026-08-19, matching the Home portal — Code is not dependable as a key
   * (the catalogue holds 'HOME', '002', 'srms' and 'Finance', and it gets re-typed by hand).
   */
  abbreviation?: string;
  /** lucide-react icon name (Core.Subsystem.Icon) — resolved through lucideIconMap. */
  icon?: string | null;
  /** Launcher ordering (Core.Subsystem.DisplayOrder). */
  displayOrder?: number;
  // ⚠️ NO url. Core.Subsystem.Url was dropped on 2026-08-16 for SRMS parity — resolve a
  // subsystem's application address with `appUrlFor(abbreviation)` from @/config/appConfig instead.
}
