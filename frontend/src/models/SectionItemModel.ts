import type AbstractModel from './AbstractModel'


export default interface SectionItemModel extends AbstractModel {
  sectionId?:string
  sequenceNo?:number
  chartType?:string
  row?:number
  title?:string
  color?:string
  section?:string
  sectionItemField?:any,
  sectionCriteriaField?:any,
  pivotCol?:string
  summaryCol?:string
}
