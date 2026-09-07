import { api } from "@/utils/apiClient";
import {
  EmployeeEducationSchema,
  EmployeeExperienceSchema,
  EmployeeDependentSchema,
} from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
import { createDeleteService } from "@/template/createDeleteService";
import type {
  EmployeeEducationModel,
  EmployeeExperienceModel,
  EmployeeDependentModel,
} from "@/models";

/* Education (HC017) */
export const getEducations = (employeeId: string) =>
  api.get<EmployeeEducationModel[]>(`EmployeeEducation?employeeId=${employeeId}`);
export const saveEducation = createSaveService("EmployeeEducation", EmployeeEducationSchema, {
  customFields: true,
});
export const deleteEducation = createDeleteService("EmployeeEducation");

/* Experience (HC018) */
export const getExperiences = (employeeId: string) =>
  api.get<EmployeeExperienceModel[]>(`EmployeeExperience?employeeId=${employeeId}`);
export const saveExperience = createSaveService("EmployeeExperience", EmployeeExperienceSchema, {
  booleanFields: ["isExternal", "isGovernmental"],
  // Sent as a real JSON number so the nullable decimal binds. A BLANK box never reaches this
  // coercion — createSaveService drops empty values first — so "not recorded" stays null rather
  // than becoming 0, which would claim the role was unpaid.
  numberFields: ["salary"],
  customFields: true,
});
export const deleteExperience = createDeleteService("EmployeeExperience");

/* Family / dependents (HC019-HC020) */
export const getDependents = (employeeId: string) =>
  api.get<EmployeeDependentModel[]>(`EmployeeDependent?employeeId=${employeeId}`);
export const saveDependent = createSaveService("EmployeeDependent", EmployeeDependentSchema, {
  booleanFields: ["isDependent"],
  customFields: true,
});
export const deleteDependent = createDeleteService("EmployeeDependent");
