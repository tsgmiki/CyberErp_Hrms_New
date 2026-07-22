import { api } from "@/utils/apiClient";
import {
  EmployeeMovementSchema,
  DisciplinaryMeasureSchema,
} from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";
import { createDeleteService } from "@/template/createDeleteService";
import type { EmployeeMovementModel, DisciplinaryMeasureModel } from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/* Movements — transfers / promotions / demotions (SAP-style personnel actions) */
export const getMovements = (employeeId: string) =>
  api.get<EmployeeMovementModel[]>(`EmployeeMovement?employeeId=${employeeId}`);
export const saveMovement = createSaveService("EmployeeMovement", EmployeeMovementSchema, {
  numberFields: ["toSalary"],
  customFields: true,
});
export const deleteMovement = createDeleteService("EmployeeMovement");

async function postAction(path: string): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
    credentials: "include",
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors?.status?.[0] ?? parsed?.errors?.toPositionId?.[0] ?? parsed?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/** Applies the movement to the employee master (position/grade/salary + vacancy sync). */
export const executeMovement = (id: string) => postAction(`EmployeeMovement/${id}/execute`);
export const cancelMovement = (id: string) => postAction(`EmployeeMovement/${id}/cancel`);

/* Disciplinary measures */
export const getDisciplinaryMeasures = (employeeId: string) =>
  api.get<DisciplinaryMeasureModel[]>(`DisciplinaryMeasure?employeeId=${employeeId}`);
export const saveDisciplinaryMeasure = createSaveService("DisciplinaryMeasure", DisciplinaryMeasureSchema, {
  customFields: true,
  booleanFields: ["affectsPromotion", "affectsReward"],
});
export const deleteDisciplinaryMeasure = createDeleteService("DisciplinaryMeasure");
