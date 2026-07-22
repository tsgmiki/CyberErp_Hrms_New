import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  ExitQuestionModel,
  ExitInterviewModel,
  TerminationSettlementModel,
  SettlementLineModel,
  GeneratedDocumentModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

async function jsonAction(method: string, path: string, body?: unknown): Promise<{ ok: boolean; message: string; id?: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method,
    credentials: "include",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  let id: string | undefined;
  try {
    const parsed = JSON.parse(text);
    message = errorMessageParser(parsed.errors || parsed) || parsed?.message || message;
    if (res.ok) message = parsed?.message ?? "Done";
    id = parsed?.id;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message, id };
}

async function getJson<T>(path: string): Promise<T | null> {
  const res = await fetch(`${API_BASE_URL}/${path}`, { credentials: "include" });
  if (!res.ok) throw new Error("Request failed");
  const text = await res.text();
  if (!text || !isValidJson(text)) return null;
  return JSON.parse(text) as T;
}

/* ---- Exit-interview questionnaire (HC219) ---- */

export const getExitQuestionnaire = () =>
  getJson<{ questions: ExitQuestionModel[] }>("ExitInterview/questionnaire");

export const saveExitQuestionnaire = (questions: ExitQuestionModel[]) =>
  jsonAction("PUT", "ExitInterview/questionnaire", { questions });

export const launchExitInterview = (terminationId: string) =>
  jsonAction("POST", `ExitInterview/launch/${terminationId}`);

export const submitExitInterview = (id: string, answers: Record<string, string>) =>
  jsonAction("POST", `ExitInterview/${id}/submit`, { answers });

/** Null when not launched. */
export const getExitInterview = (terminationId: string) =>
  getJson<ExitInterviewModel>(`ExitInterview/${terminationId}`);

/* ---- Final settlement (HC216/HC217/HC218) ---- */

export const buildSettlement = (terminationId: string) =>
  jsonAction("POST", `TerminationSettlement/build/${terminationId}`);

/** Null when not built. */
export const getSettlement = (terminationId: string) =>
  getJson<TerminationSettlementModel>(`TerminationSettlement/${terminationId}`);

export const updateSettlementLines = (settlementId: string, lines: SettlementLineModel[], notes?: string) =>
  jsonAction("PUT", `TerminationSettlement/${settlementId}/lines`, { lines, notes });

export const approveSettlement = (settlementId: string) =>
  jsonAction("POST", `TerminationSettlement/${settlementId}/approve`);

export const markSettlementPaid = (settlementId: string, reference?: string) =>
  jsonAction("POST", `TerminationSettlement/${settlementId}/mark-paid`, { reference });

export const generateSettlementLetter = async (templateId: string, settlementId: string): Promise<GeneratedDocumentModel> => {
  const res = await fetch(`${API_BASE_URL}/DocumentTemplate/${templateId}/generate-settlement/${settlementId}`, {
    credentials: "include",
  });
  if (!res.ok) throw new Error((await res.text()) || "Failed to generate the letter");
  return (await res.json()) as GeneratedDocumentModel;
};
