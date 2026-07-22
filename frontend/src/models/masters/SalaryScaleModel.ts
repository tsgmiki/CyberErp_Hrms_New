import type AbstractModel from "../AbstractModel";

export default interface SalaryScaleModel extends AbstractModel {
  jobGradeId?: string;
  jobGrade?: string;
  stepId?: string;
  step?: string;
  salary?: number;
}
