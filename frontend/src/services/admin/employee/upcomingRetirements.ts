import { api } from "@/utils/apiClient";

export interface RetiringEmployee {
  id: string;
  fullName: string;
  employeeNumber: string;
  dateOfBirth?: string;
  retirementDate: string;
  daysRemaining: number;
}

export default async function getUpcomingRetirements() {
  return api.get<RetiringEmployee[]>("Employee/upcoming-retirements");
}
