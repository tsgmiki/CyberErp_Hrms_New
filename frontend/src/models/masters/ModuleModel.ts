import type AbstractModel from '../AbstractModel'
import type OperationModel from './OperationModel'

export default interface ModuleModel extends AbstractModel {
  name?:string
  icon?:string
  subSystem?:string
  operations?:OperationModel[]
}
