import { createPagedQuery } from "@/template/createPagedQuery";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  SuggestionModel,
  GrievanceModel,
  AnnouncementModel,
  SurveyModel,
  SurveyQuestionModel,
  SurveyResultsModel,
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

async function getJson<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE_URL}/${path}`, { credentials: "include" });
  const text = await res.text();
  if (!res.ok) {
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    throw new Error(errorMessageParser(parsed.errors || parsed) || "Request failed");
  }
  return JSON.parse(text) as T;
}

/* ---- Suggestions (HC203/HC207) ---- */

export const getAllSuggestions = createPagedQuery<SuggestionModel>("Suggestion");
export const submitSuggestion = (title: string, body: string, isAnonymous: boolean) =>
  jsonAction("POST", "Suggestion", { title, body, isAnonymous });
export const respondSuggestion = (id: string, status: string, managementResponse?: string) =>
  jsonAction("PUT", "Suggestion/respond", { id, status, managementResponse });
export const deleteSuggestion = (id: string) => jsonAction("DELETE", `Suggestion/${id}`);

/* ---- Grievances (HC205) ---- */

export const getAllGrievances = createPagedQuery<GrievanceModel>("Grievance");
export const getGrievance = (id: string) => getJson<GrievanceModel>(`Grievance/${id}`);
export const submitGrievance = (model: GrievanceModel) => jsonAction("POST", "Grievance", model);
export const assignGrievance = (id: string, assigneeEmployeeId: string) =>
  jsonAction("POST", `Grievance/${id}/assign/${assigneeEmployeeId}`);
export const resolveGrievance = (id: string, resolution: string) =>
  jsonAction("POST", `Grievance/${id}/resolve`, { resolution });
export const closeGrievance = (id: string) => jsonAction("POST", `Grievance/${id}/close`);
export const addGrievanceNote = (id: string, note: string) =>
  jsonAction("POST", `Grievance/${id}/notes`, { note });

/* ---- Announcements (HC206) ---- */

export const getAllAnnouncements = createPagedQuery<AnnouncementModel>("Announcement");
export const getAnnouncementFeed = createPagedQuery<AnnouncementModel>("Announcement/feed");
export const saveAnnouncement = async (model: AnnouncementModel): Promise<{ ok: boolean; message: string }> =>
  jsonAction(model.id ? "PUT" : "POST", "Announcement", model);
export const deleteAnnouncement = (id: string) => jsonAction("DELETE", `Announcement/${id}`);

/* ---- Surveys & polls (HC204) ---- */

export const getAllSurveys = createPagedQuery<SurveyModel>("Survey");
export const getSurveyFeed = createPagedQuery<SurveyModel>("Survey/feed");
export const getSurveyById = (id: string) => getJson<SurveyModel>(`Survey/${id}`);
export const getSurveyResults = (id: string) => getJson<SurveyResultsModel>(`Survey/${id}/results`);
export const saveSurvey = (model: SurveyModel & { questions: SurveyQuestionModel[] }) =>
  jsonAction(model.id ? "PUT" : "POST", "Survey", model);
export const openSurvey = (id: string) => jsonAction("POST", `Survey/${id}/open`);
export const closeSurvey = (id: string) => jsonAction("POST", `Survey/${id}/close`);
export const respondSurvey = (id: string, answers: Record<string, string>) =>
  jsonAction("POST", `Survey/${id}/respond`, { answers });
export const deleteSurvey = (id: string) => jsonAction("DELETE", `Survey/${id}`);
