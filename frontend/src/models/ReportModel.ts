import type AbstractModel from './AbstractModel'

export interface ReportCriteriaModel extends AbstractModel {
  fieldName?: string
  operator?: string
  value?: string
  connector?: string
}

export default interface ReportModel extends AbstractModel {
  fields?: string
  reportCriterias?: ReportCriteriaModel[]
}
