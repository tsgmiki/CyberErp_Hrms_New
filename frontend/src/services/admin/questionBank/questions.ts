import { api } from "@/utils/apiClient";
import type { QuestionModel } from "@/models";

type Result = { status: "success" | "error"; message: string };

const fail = (e: unknown, fallback: string): Result => ({
  status: "error",
  // The server names the offending question ("'Cold chain…' has no correct answer."), which is far
  // more use to an author than a generic failure.
  message: e instanceof Error ? e.message : fallback,
});

/** The bank's questions, in order, with the answer key — this is an authoring endpoint. */
export const getBankQuestions = (questionBankId: string) =>
  api.get<QuestionModel[]>(`QuestionBank/${encodeURIComponent(questionBankId)}/questions`);

/**
 * Replaces the bank's whole question list — set semantics, matching the server handler.
 *
 * The screen always posts what the author is looking at, so a dropped request can never leave a
 * half-written bank behind.
 */
export const setBankQuestions = async (
  questionBankId: string,
  questions: QuestionModel[],
): Promise<Result> => {
  try {
    const res = await api.put<{ message?: string }>("QuestionBank/questions", {
      questionBankId,
      questions,
    });
    return { status: "success", message: res?.message ?? "Questions updated" };
  } catch (e) {
    return fail(e, "Could not save the questions.");
  }
};
