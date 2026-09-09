import { api } from "@/utils/apiClient";
import type { CourseCompetencyModel } from "@/models";

/** The competencies a course develops, primary first. */
export const getCourseCompetencies = (trainingCourseId: string) =>
  api.get<CourseCompetencyModel[]>(
    `CourseCompetency?trainingCourseId=${encodeURIComponent(trainingCourseId)}`,
  );

/**
 * Replaces the course's whole mapping — set semantics, matching the server handler.
 *
 * Sending the full list rather than per-row add/remove keeps the editor and the server in step: the
 * screen always posts what the user is looking at, so a dropped request can never leave a partial
 * mapping behind.
 */
export const setCourseCompetencies = async (
  trainingCourseId: string,
  competencies: { competencyId: string; isPrimary: boolean }[],
): Promise<{ status: "success" | "error"; message: string }> => {
  try {
    const res = await api.put<{ message?: string }>("CourseCompetency", {
      trainingCourseId,
      competencies,
    });
    return { status: "success", message: res?.message ?? "Competencies updated" };
  } catch (e) {
    return { status: "error", message: e instanceof Error ? e.message : "Could not save the competencies." };
  }
};
