import type AbstractModel from './AbstractModel'

export default interface CriteriaModel extends AbstractModel {
  field?:string
  operator?:string
  value?:string
  group?:number
  connector?:string 
}
