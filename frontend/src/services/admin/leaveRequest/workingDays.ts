import { api } from "@/utils/apiClient";

export interface WorkingDaysResult {
  startDate: string;
  endDate: string;
  workingDays: number;
  calendarDays: number;
  nonWorkingDays: string[];
}

export default async function getWorkingDays(start: string, end: string, halfDay = false) {
  const q = new URLSearchParams({ start, end, halfDay: String(halfDay) }).toString();
  return api.get<WorkingDaysResult>(`Holiday/working-days?${q}`);
}
