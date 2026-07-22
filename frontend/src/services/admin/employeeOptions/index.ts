import { api } from "@/utils/apiClient";

export interface EmployeeOptionModel {
  id: string;
  name: string;
  employeeNumber: string;
}

/** Role-scoped employee options: "All" (HR admin) | "Unit" (manager subtree) | "Self" (locked). */
export interface EmployeeOptionsModel {
  scope: "All" | "Unit" | "Self";
  self: EmployeeOptionModel | null;
  options: EmployeeOptionModel[];
}

export const getEmployeeOptions = (search?: string, excludeId?: string) => {
  const params = new URLSearchParams();
  if (search) params.set("search", search);
  if (excludeId) params.set("exclude", excludeId);
  const qs = params.toString();
  return api.get<EmployeeOptionsModel>(`EmployeeOptions${qs ? `?${qs}` : ""}`);
};
