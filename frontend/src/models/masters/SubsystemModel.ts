import type AbstractModel from "../AbstractModel";

/** Master list of ERP subsystems (dbo.coreSubsystem); modules reference one by name. */
export default interface SubsystemModel extends AbstractModel {
  name?: string;
  code?: string;
  /** lucide-react icon name (Core.Subsystem.Icon) — resolved through lucideIconMap. */
  icon?: string | null;
  /** Launcher ordering (Core.Subsystem.DisplayOrder). */
  displayOrder?: number;
  // ⚠️ NO url. Core.Subsystem.Url was dropped on 2026-08-16 for SRMS parity — resolve a
  // subsystem's application address with `appUrlFor(code)` from @/config/appConfig instead.
}
