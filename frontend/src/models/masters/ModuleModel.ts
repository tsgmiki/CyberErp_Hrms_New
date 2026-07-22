import type AbstractModel from '../AbstractModel'
import type OperationModel from './OperationModel'

export default interface ModuleModel extends AbstractModel {
  name?:string
  icon?:string
  /** FK to the subsystem master list (dbo.coreSubsystem). */
  subsystemId?:string
  /** Subsystem display name (resolved server-side from subsystemId). */
  subSystem?:string
  sortOrder?:number
  operations?:OperationModel[]
}
