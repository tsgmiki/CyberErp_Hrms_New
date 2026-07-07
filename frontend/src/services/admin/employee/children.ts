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
export const saveEducation = createSaveService("EmployeeEducation", EmployeeEducationSchema);
export const deleteEducation = createDeleteService("EmployeeEducation");

/* Experience (HC018) */
export const getExperiences = (employeeId: string) =>
  api.get<EmployeeExperienceModel[]>(`EmployeeExperience?employeeId=${employeeId}`);
export const saveExperience = createSaveService("EmployeeExperience", EmployeeExperienceSchema);
export const deleteExperience = createDeleteService("EmployeeExperience");

/* Family / dependents (HC019-HC020) */
export const getDependents = (employeeId: string) =>
  api.get<EmployeeDependentModel[]>(`EmployeeDependent?employeeId=${employeeId}`);
export const saveDependent = createSaveService("EmployeeDependent", EmployeeDependentSchema, {
  booleanFields: ["isDependent"],
});
export const deleteDependent = createDeleteService("EmployeeDependent");
