import { api } from "@/utils/apiClient";
import { createPagedQuery } from "@/template/createPagedQuery";
import { createDeleteService } from "@/template/createDeleteService";
import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type {
  HiringRequestModel,
  RecruitmentBudgetRowModel,
  JobRequisitionModel,
  CandidateModel,
  CandidateMatchModel,
  JobApplicationModel,
  CandidateDocumentModel,
  ApplicationRankingRowModel,
} from "@/models";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

interface SaveResult {
  status: "success" | "error";
  message: string;
  zodErrors: Record<string, string[] | undefined>;
  id?: string;
}

/** Shared bespoke JSON save (the recruitment forms post component state, not FormData). */
async function saveJson(resource: string, data: Record<string, unknown>): Promise<SaveResult> {
  const isUpdate = !!data.id;
  const body = { ...data };
  if (!isUpdate) delete body.id;
  try {
    const response = await fetch(`${API_BASE_URL}/${resource}`, {
      method: isUpdate ? "PUT" : "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    });
    if (!response.ok) {
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    let savedId = data.id as string | undefined;
    if (!isUpdate) {
      const text = await response.text();
      savedId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
    }
    return { status: "success", message: "Successfully saved", zodErrors: {}, id: savedId };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

async function post(path: string, body?: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "POST",
    credentials: "include",
    ...(body ? { headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) } : {}),
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

async function put(path: string, body: unknown): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/${path}`, {
    method: "PUT",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  const text = await res.text();
  let message = res.ok ? "Done" : "Request failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/* ---- Hiring requests (HC077–HC083) ---------------------------------------- */

export const getAllHiringRequests = createPagedQuery<HiringRequestModel>("HiringRequest");
export const getHiringRequest = (id: string) => api.get<HiringRequestModel>(`HiringRequest/${id}`);
export const deleteHiringRequest = createDeleteService("HiringRequest");
export const saveHiringRequest = (data: HiringRequestModel) =>
  saveJson("HiringRequest", {
    ...data,
    numberOfPositions: Number(data.numberOfPositions) || 1,
    estimatedBudget: Number(String(data.estimatedBudget ?? "").replace(/[,\s]/g, "")) || 0,
  });
export const submitHiringRequest = (id: string) => post(`HiringRequest/${id}/submit`);
export const closeHiringRequest = (id: string) => post(`HiringRequest/${id}/close`);
export const getRecruitmentBudgetMonitor = () =>
  api.get<RecruitmentBudgetRowModel[]>("HiringRequest/budget-monitor");

/* ---- Job requisitions (HC084–HC088, HC091) ---------------------------------- */

export const getAllJobRequisitions = createPagedQuery<JobRequisitionModel>("JobRequisition");
export const getJobRequisition = (id: string) => api.get<JobRequisitionModel>(`JobRequisition/${id}`);
export const deleteJobRequisition = createDeleteService("JobRequisition");
export const saveJobRequisition = (data: JobRequisitionModel) =>
  saveJson("JobRequisition", {
    ...data,
    numberOfPositions: Number(data.numberOfPositions) || 1,
    minExperienceYears:
      data.minExperienceYears === undefined || data.minExperienceYears === null || String(data.minExperienceYears) === ""
        ? null
        : Number(data.minExperienceYears),
    screeningCriteria: (data.screeningCriteria ?? []).map((c) => ({
      ...c,
      weight: Number(c.weight) || 1,
      isMandatory: c.isMandatory === true || String(c.isMandatory) === "true",
    })),
  });
export const submitJobRequisition = (id: string) => post(`JobRequisition/${id}/submit`);
export const postJobRequisition = (id: string) => post(`JobRequisition/${id}/post`);
export const closeJobRequisition = (id: string) => post(`JobRequisition/${id}/close`);
export const cancelJobRequisition = (id: string) => post(`JobRequisition/${id}/cancel`);
export const generateRequisitionPosting = (id: string) =>
  api.get<string>(`JobRequisition/${id}/generate-posting`);
export const setRequisitionPosting = (dto: {
  id: string;
  postingChannel: string;
  postingText?: string;
  openFrom?: string;
  openUntil?: string;
}) => put("JobRequisition/posting", dto);

/* ---- Candidates (HC092–HC097, HC089–HC090) ------------------------------------ */

export const getAllCandidates = createPagedQuery<CandidateModel>("Candidate");
export const getCandidate = (id: string) => api.get<CandidateModel>(`Candidate/${id}`);
export const deleteCandidate = createDeleteService("Candidate");
export const saveCandidate = (data: CandidateModel) =>
  saveJson("Candidate", {
    ...data,
    yearsOfExperience:
      data.yearsOfExperience === undefined || data.yearsOfExperience === null || String(data.yearsOfExperience) === ""
        ? null
        : Number(data.yearsOfExperience),
    consentGiven: data.consentGiven === true || String(data.consentGiven) === "true",
  });
export const setCandidateTalentPool = (id: string, inPool: boolean, notes?: string) =>
  put("Candidate/talent-pool", { id, inPool, notes: notes || null });
export const anonymizeCandidate = (id: string) => post(`Candidate/${id}/anonymize`);
export const matchCandidates = (requisitionId: string) =>
  api.get<CandidateMatchModel[]>(`Candidate/match?requisitionId=${requisitionId}`);

export async function uploadCandidateResume(
  id: string,
  file: File,
): Promise<{ ok: boolean; message: string }> {
  const form = new FormData();
  form.append("file", file);
  const res = await fetch(`${API_BASE_URL}/Candidate/${id}/resume`, {
    method: "POST",
    credentials: "include",
    body: form,
  });
  const text = await res.text();
  let message = res.ok ? "Resume uploaded" : "Upload failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export const candidateResumeUrl = (id: string) => `${API_BASE_URL}/Candidate/${id}/resume`;

/* ---- Candidate documents (credentials + mandatory compliance set) ------------------ */

export const getCandidateDocuments = (candidateId: string) =>
  api.get<CandidateDocumentModel[]>(`Candidate/${candidateId}/documents`);

export const candidateDocumentUrl = (documentId: string) =>
  `${API_BASE_URL}/Candidate/documents/${documentId}`;

export async function uploadCandidateDocument(
  candidateId: string,
  documentType: string,
  file: File,
): Promise<{ ok: boolean; message: string }> {
  const form = new FormData();
  form.append("documentType", documentType);
  form.append("file", file);
  const res = await fetch(`${API_BASE_URL}/Candidate/${candidateId}/documents`, {
    method: "POST",
    credentials: "include",
    body: form,
  });
  const text = await res.text();
  let message = res.ok ? "Document uploaded" : "Upload failed";
  try {
    const parsed = JSON.parse(text);
    message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

export async function deleteCandidateDocument(documentId: string): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/Candidate/documents/${documentId}`, {
    method: "DELETE",
    credentials: "include",
  });
  const text = await res.text();
  let message = res.ok ? "Document deleted" : "Request failed";
  try {
    message = JSON.parse(text)?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}

/** Converts a hired candidate into an employee on the same person record (returns employee id). */
export async function hireCandidate(
  id: string,
  dto: {
    employeeNumber: string;
    hireDate?: string;
    positionId?: string;
    salaryScaleId?: string;
    salary?: number;
    employmentNature: string;
    contractPeriod?: number;
    isProbation: boolean;
    probationEndDate?: string;
  },
): Promise<{ ok: boolean; message: string; employeeId?: string }> {
  const res = await fetch(`${API_BASE_URL}/Candidate/${id}/hire`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ ...dto, id }),
  });
  const text = await res.text();
  if (!res.ok) {
    let message = "Hire failed";
    try {
      const parsed = JSON.parse(text);
      message = parsed?.errors ? errorMessageParser(parsed.errors) : (parsed?.message ?? message);
    } catch {
      if (text) message = text;
    }
    return { ok: false, message };
  }
  const employeeId = isValidJson(text) ? JSON.parse(text) : text.replace(/"/g, "");
  return { ok: true, message: "Candidate hired", employeeId };
}

/* ---- Evaluator scoring & ranking ---------------------------------------------------- */

export const scoreJobApplication = (dto: {
  id: string;
  scores: { criterionId: string; score: number; remarks?: string }[];
}) => put("JobApplication/scores", dto);

export const getApplicationRanking = (requisitionId: string) =>
  api.get<ApplicationRankingRowModel[]>(`JobApplication/ranking?requisitionId=${requisitionId}`);

/* ---- Applications (HC098–HC099) --------------------------------------------------- */

export const getAllJobApplications = createPagedQuery<JobApplicationModel>("JobApplication");
export const getJobApplication = (id: string) => api.get<JobApplicationModel>(`JobApplication/${id}`);
export const createJobApplication = (dto: { candidateId: string; requisitionId: string; appliedAt?: string }) =>
  post("JobApplication", dto);
export const moveApplicationStage = (dto: {
  id: string;
  stage: string;
  note?: string;
  screeningScore?: number;
  screeningRemarks?: string;
}) => put("JobApplication/stage", dto);
