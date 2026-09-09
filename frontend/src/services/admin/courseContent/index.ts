import { api } from "@/utils/apiClient";
import type { ContentModuleModel, CourseVersionModel } from "@/models";

type Result = { status: "success" | "error"; message: string };

const fail = (e: unknown, fallback: string): Result => ({
  status: "error",
  // The server's own words ("A version needs at least one module before it can be published.")
  // tell the author what to do; a generic failure does not.
  message: e instanceof Error ? e.message : fallback,
});

/** Every version of a course, newest first. */
export const getCourseVersions = (trainingCourseId: string) =>
  api.get<CourseVersionModel[]>(
    `CourseVersion?trainingCourseId=${encodeURIComponent(trainingCourseId)}`,
  );

/** Opens a new draft. The server refuses a second one — a course has at most one open draft. */
export const createCourseVersion = async (
  trainingCourseId: string,
  changeNote?: string,
): Promise<Result & { id?: string }> => {
  try {
    const res = await api.post<{ id: string }>("CourseVersion", { trainingCourseId, changeNote });
    return { status: "success", message: "Draft created", id: res?.id };
  } catch (e) {
    return fail(e, "Could not create the draft.");
  }
};

/**
 * Replaces the draft's whole module list — set semantics, matching the server handler.
 *
 * The screen always posts what the author is looking at, so a dropped request can never leave a
 * half-written course behind.
 */
export const setCourseVersionModules = async (
  courseVersionId: string,
  modules: ContentModuleModel[],
): Promise<Result> => {
  try {
    const res = await api.put<{ message?: string }>("CourseVersion/modules", {
      courseVersionId,
      modules,
    });
    return { status: "success", message: res?.message ?? "Content updated" };
  } catch (e) {
    return fail(e, "Could not save the content.");
  }
};

/** Makes the draft live and retires the version it replaces. */
export const publishCourseVersion = async (courseVersionId: string): Promise<Result> => {
  try {
    const res = await api.post<{ message?: string }>(`CourseVersion/${courseVersionId}/publish`, {});
    return { status: "success", message: res?.message ?? "Version published" };
  } catch (e) {
    return fail(e, "Could not publish the version.");
  }
};
