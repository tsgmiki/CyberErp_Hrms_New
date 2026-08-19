import type AbstractModel from '../AbstractModel'
import type OperationModel from './OperationModel'

export default interface ModuleModel extends AbstractModel {
  name?:string
  icon?:string
  /** FK to the subsystem master list (dbo.coreSubsystem). */
  subsystemId?:string
  /** Subsystem display NAME (resolved server-side) -- a renameable LABEL, do NOT scope on it. */
  subSystem?:string
  /**
   * The owning subsystem's ABBREVIATION (Core.Subsystem.Abbreviation), resolved server-side.
   * THE key to scope the sidebar on -- `subSystem` is a display name and gets renamed.
   */
  subSystemAbbreviation?:string
  sortOrder?:number
  operations?:OperationModel[]
}
