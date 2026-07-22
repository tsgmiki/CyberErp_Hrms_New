import { api } from "@/utils/apiClient";

export interface ProbationEmployee {
  id: string;
  fullName: string;
  employeeNumber: string;
  positionTitle?: string;
  hireDate?: string;
  probationEndDate?: string;
  daysRemaining?: number;
}

export default async function getEmployeesOnProbation() {
  return api.get<ProbationEmployee[]>("Employee/on-probation");
}
