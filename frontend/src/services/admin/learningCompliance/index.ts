import { api } from "@/utils/apiClient";
import type {
  ComplianceOverviewModel, ComplianceRunResultModel, EffectivenessRowModel, ObligationModel,
} from "@/models";
import type { PagedResult } from "@/template/types";

type Result = { status: "success" | "error"; message: string };

const fail = (e: unknown, fallback: string): Result => ({
  status: "error",
  message: e instanceof Error ? e.message : fallback,
});

const qs = (params: Record<string, string | number | undefined>) =>
  Object.entries(params)
    .filter(([, v]) => v !== undefined && v !== "")
    .map(([k, v]) => `${k}=${encodeURIComponent(String(v))}`)
    .join("&");

/**
 * Who owes what.
 *
 * `status` accepts "Overdue" as well as the stored statuses — the server translates it, because
 * overdue is Pending with a passed due date rather than a value the column holds.
 */
export const getObligations = (params: {
  skip?: number;
  take?: number;
  itemId?: string;
  employeeId?: string;
  status?: string;
}) =>
  api.get<PagedResult<ObligationModel>>(
    `LearningCompliance/obligations?${qs({
      skip: params.skip ?? 0,
      take: params.take ?? 25,
      itemId: params.itemId,
      employeeId: params.employeeId,
      status: params.status,
    })}`,
  );

/** Where the organisation stands, by course and by unit. */
export const getComplianceOverview = (trainingCourseId?: string) =>
  api.get<ComplianceOverviewModel>(
    `LearningCompliance/overview${trainingCourseId ? `?trainingCourseId=${encodeURIComponent(trainingCourseId)}` : ""}`,
  );

/** Kirkpatrick levels 1 and 2 measured, level 3 as a signal. */
export const getEffectiveness = (trainingCourseId?: string) =>
  api.get<EffectivenessRowModel[]>(
    `LearningCompliance/effectiveness${trainingCourseId ? `?trainingCourseId=${encodeURIComponent(trainingCourseId)}` : ""}`,
  );

/** Excuses one obligation. The reason is required — an audit has to be able to read why. */
export const waiveObligation = async (id: string, reason: string): Promise<Result> => {
  try {
    const res = await api.post<{ message?: string }>(
      `LearningCompliance/obligations/${encodeURIComponent(id)}/waive`, { reason });
    return { status: "success", message: res?.message ?? "Obligation waived" };
  } catch (e) {
    return fail(e, "Could not waive that obligation.");
  }
};

/**
 * Runs the sweep now instead of waiting for tonight.
 *
 * HR only, and it messages everyone with something outstanding — the screen says so before the
 * button is pressed.
 */
export const runComplianceSweep = async (): Promise<Result & { result?: ComplianceRunResultModel }> => {
  try {
    const result = await api.post<ComplianceRunResultModel>("LearningCompliance/run", {});
    return {
      status: "success",
      message: `${result.obligationsCreated} created · ${result.obligationsSatisfied} satisfied · ${result.remindersSent} reminded`,
      result,
    };
  } catch (e) {
    return fail(e, "Could not run the sweep.");
  }
};
