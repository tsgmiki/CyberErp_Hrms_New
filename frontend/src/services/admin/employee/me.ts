import { api } from "@/utils/apiClient";

/** The signed-in user's own employee identity (self-service screens). */
export interface MyEmployeeModel {
  id: string;
  employeeNumber?: string;
  fullName?: string;
}

/** Null when the account has no linked employee (the API returns 204 → empty body). */
export default async function getMyEmployee(): Promise<MyEmployeeModel | null> {
  const result = await api.get<MyEmployeeModel | string | null>("Employee/me");
  if (!result || typeof result === "string") return null;
  return result.id ? result : null;
}
