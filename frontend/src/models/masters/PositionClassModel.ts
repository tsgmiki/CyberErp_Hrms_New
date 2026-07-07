import type AbstractModel from "../AbstractModel";

export default interface PositionClassModel extends AbstractModel {
  code?: string;
  title?: string;
  allocatedHeadcount?: number;
  minQualifications?: string;
  minExperienceYears?: number;
  skills?: string;
  description?: string;
  isActive?: boolean;

  minimumAge?: number;
  maximumAge?: number;
  weeklyWorkingHours?: number;

  // Pay point: salary scale (grade + step + salary). jobGradeId is a derived UI filter.
  salaryScaleId?: string;
  salaryStep?: string;
  salary?: number;
  jobGradeId?: string;
  jobGradeName?: string;
  jobCategoryId?: string;
  jobCategoryName?: string;
  workLocationId?: string;
  workLocationName?: string;
  reportsToPositionClassId?: string;
  reportsToPositionClassTitle?: string;
}
