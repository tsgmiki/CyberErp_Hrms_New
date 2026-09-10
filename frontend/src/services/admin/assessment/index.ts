import { api } from "@/utils/apiClient";
import type { AssessmentModel, QuestionModel } from "@/models";

type Result = { status: "success" | "error"; message: string };

const fail = (e: unknown, fallback: string): Result => ({
  status: "error",
  message: e instanceof Error ? e.message : fallback,
});

/**
 * The quiz on a Quiz module, or null when it has not been built yet.
 *
 * Null is the normal state of a freshly added Quiz module, not an error — the editor renders empty
 * for it.
 */
export const getAssessment = (contentModuleId: string) =>
  api.get<AssessmentModel | null>(`Assessment/${encodeURIComponent(contentModuleId)}`);

/** Creates or updates the quiz's settings. Refused once the course version is published. */
export const saveAssessment = async (dto: {
  contentModuleId: string;
  title: string;
  instructions?: string | null;
  passMark: number;
  maxAttempts?: number | null;
  timeLimitMinutes?: number | null;
  shuffleQuestions: boolean;
  revealAnswers: boolean;
}): Promise<Result & { id?: string }> => {
  try {
    const res = await api.post<{ id: string }>("Assessment", dto);
    return { status: "success", message: "Quiz saved", id: res?.id };
  } catch (e) {
    return fail(e, "Could not save the quiz.");
  }
};

/** Replaces the quiz's whole question list — set semantics. */
export const setAssessmentQuestions = async (
  assessmentId: string,
  questions: QuestionModel[],
): Promise<Result> => {
  try {
    const res = await api.put<{ message?: string }>("Assessment/questions", {
      assessmentId,
      questions,
    });
    return { status: "success", message: res?.message ?? "Questions updated" };
  } catch (e) {
    return fail(e, "Could not save the questions.");
  }
};

/**
 * Copies bank questions onto the END of the quiz.
 *
 * Appending rather than replacing is what lets one quiz be built from several banks. An empty
 * `questionIds` means the whole bank.
 */
export const importQuestions = async (
  assessmentId: string,
  questionBankId: string,
  questionIds: string[] = [],
): Promise<Result & { imported?: number }> => {
  try {
    const res = await api.post<{ imported: number; message?: string }>("Assessment/import", {
      assessmentId,
      questionBankId,
      questionIds,
    });
    return {
      status: "success",
      message: `${res?.imported ?? 0} question(s) imported`,
      imported: res?.imported,
    };
  } catch (e) {
    return fail(e, "Could not import the questions.");
  }
};
