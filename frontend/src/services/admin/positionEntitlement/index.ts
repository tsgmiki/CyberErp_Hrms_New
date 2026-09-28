import { api } from "@/utils/apiClient";
import type { PositionEntitlementModel } from "@/models";

/** Everything one post class carries beyond its salary. */
export const getPositionEntitlements = (positionClassId: string) =>
  api.get<PositionEntitlementModel[]>(`PositionEntitlement?positionClassId=${positionClassId}`);

export const savePositionEntitlement = (m: PositionEntitlementModel) =>
  api.post<{ id: string }>("PositionEntitlement", {
    id: m.id || undefined,
    positionClassId: m.positionClassId,
    kind: m.kind,
    allowanceTypeId: m.kind === "Allowance" ? m.allowanceTypeId : null,
    benefitPlanId: m.kind === "BenefitPlan" ? m.benefitPlanId : null,
    // An empty box means "use the catalogue default", which is not a value of zero.
    value: m.value === null || m.value === undefined || (m.value as unknown) === "" ? null : Number(m.value),
    grantedWhenActing: m.grantedWhenActing !== false,
    isActive: m.isActive !== false,
    notes: m.notes || null,
  });

export const deletePositionEntitlement = (id: string) =>
  api.delete<{ message: string }>(`PositionEntitlement/${id}`);
