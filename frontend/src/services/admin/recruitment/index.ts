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
  CandidateEducationModel,
  CandidateExperienceModel,
  CandidateMatchModel,
  JobApplicationModel,
  CandidateDocumentModel,
  ApplicationRankingRowModel,
  EmployeeDocumentModel,
  InterviewModel,
  InterviewConsolidatedModel,
  JobOfferModel,
  OfferDefaultsModel,
  HireQueueRowModel,
  BulkMoveResultModel,
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
      evaluators: (c.evaluators ?? []).map((e) => ({
        evaluatorType: e.evaluatorType,
        employeeId: e.employeeId || undefined,
        name: e.name || undefined,
      })),
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

/* ---- Candidate structured background (education / experience) --------------------------
   These write the SAME person-owned rows the employee profile uses. Because the candidate
   already carries a PersonId, the rows become the employee's automatically at hire — no copy.
   The endpoints always POST (the handler upserts on the dto's id); read-only for internal. */

const numOrNull = (v: unknown) =>
  v === undefined || v === null || String(v) === "" ? null : Number(v);

/** POST to a candidate sub-resource (create/update decided server-side by the dto id). */
async function saveCandidateChild(path: string, data: Record<string, unknown>): Promise<SaveResult> {
  try {
    const response = await fetch(`${API_BASE_URL}/${path}`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
    const text = await response.text();
    if (!response.ok) {
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return { status: "error", message: errorMessageParser(parsed.errors || parsed), zodErrors: {} };
    }
    const parsed = isValidJson(text) ? JSON.parse(text) : {};
    return { status: "success", message: parsed?.message ?? "Successfully saved", zodErrors: {}, id: parsed?.id };
  } catch {
    return { status: "error", message: "Network error", zodErrors: {} };
  }
}

export const getCandidateEducations = (candidateId: string) =>
  api.get<CandidateEducationModel[]>(`Candidate/${candidateId}/education`);
export const saveCandidateEducation = (candidateId: string, data: CandidateEducationModel) =>
  saveCandidateChild(`Candidate/${candidateId}/education`, {
    ...data,
    graduationYear: numOrNull(data.graduationYear),
  });
export const deleteCandidateEducation = createDeleteService("Candidate/education");

export const getCandidateExperiences = (candidateId: string) =>
  api.get<CandidateExperienceModel[]>(`Candidate/${candidateId}/experience`);
export const saveCandidateExperience = (candidateId: string, data: CandidateExperienceModel) =>
  saveCandidateChild(`Candidate/${candidateId}/experience`, {
    ...data,
    startDate: data.startDate || null,
    endDate: data.endDate || null,
  });
export const deleteCandidateExperience = createDeleteService("Candidate/experience");

/* Attachments on one education/experience row — same EmployeeDocument storage as the employee
   profile, so the files follow the row (and the shared person) to the employee at hire. */

export type CandidateBackgroundOwnerType = "Education" | "Experience";

export const getCandidateBackgroundDocuments = (
  candidateId: string,
  ownerType: CandidateBackgroundOwnerType,
  ownerId: string,
) =>
  api.get<EmployeeDocumentModel[]>(
    `Candidate/${candidateId}/background-documents?ownerType=${ownerType}&ownerId=${ownerId}`,
  );

export async function uploadCandidateBackgroundDocument(
  candidateId: string,
  ownerType: CandidateBackgroundOwnerType,
  ownerId: string,
  file: File,
): Promise<{ ok: boolean; message: string }> {
  const form = new FormData();
  form.append("ownerType", ownerType);
  form.append("ownerId", ownerId);
  form.append("file", file);
  const res = await fetch(`${API_BASE_URL}/Candidate/${candidateId}/background-documents`, {
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

export async function deleteCandidateBackgroundDocument(documentId: string): Promise<boolean> {
  const res = await fetch(`${API_BASE_URL}/Candidate/background-documents/${documentId}`, {
    method: "DELETE",
    credentials: "include",
  });
  return res.ok;
}

/** Fetches the file with credentials and triggers a browser download with its filename. */
export async function downloadCandidateBackgroundDocument(id: string, fileName: string): Promise<void> {
  const res = await fetch(`${API_BASE_URL}/Candidate/background-documents/${id}/download`, {
    credentials: "include",
  });
  if (!res.ok) return;
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  a.remove();
  URL.revokeObjectURL(url);
}

/* ---- Evaluator scoring & ranking ---------------------------------------------------- */

export const scoreJobApplication = (dto: {
  id: string;
  scores: { criterionId: string; score: number; remarks?: string }[];
}) => put("JobApplication/scores", dto);

export interface EvaluatorContextModel {
  isConstrainedEvaluator: boolean;
  assignedCriterionIds: string[];
  assignedRequisitionIds: string[];
}

/** The current user's evaluator scope — an assigned evaluator sees/scores only their own criteria. */
export const getEvaluatorContext = () =>
  api.get<EvaluatorContextModel>("JobApplication/evaluator-context");

export const getApplicationRanking = (requisitionId: string) =>
  api.get<ApplicationRankingRowModel[]>(`JobApplication/ranking?requisitionId=${requisitionId}`);

/** The "Hire Employee" queue — fully qualified, ranked applicants (+ waitlist). */
export const getHireQueue = () => api.get<HireQueueRowModel[]>("JobApplication/hire-queue");

/** Copies the consolidated per-criterion interview averages into the ranking score sheet. */
export const adoptInterviewScores = (applicationId: string) =>
  post(`JobApplication/${applicationId}/adopt-interview-scores`);

/* ---- Interviews & panels (HC101–HC109) --------------------------------------------- */

export const getInterviews = (applicationId: string) =>
  api.get<InterviewModel[]>(`Interview?applicationId=${applicationId}`);

export const getInterviewConsolidated = (applicationId: string) =>
  api.get<InterviewConsolidatedModel>(`Interview/consolidated?applicationId=${applicationId}`);

/** Create a round, or reschedule/repanel a pending one (server upserts on dto.id). */
export const saveInterview = (dto: {
  id?: string;
  applicationId: string;
  scheduledStart: string;
  scheduledEnd: string;
  format: string;
  location?: string;
  meetingLink?: string;
  notes?: string;
  panelists: { employeeId?: string; panelistName?: string; isLead?: boolean }[];
}) => post("Interview", dto);

export const setInterviewStatus = (dto: { id: string; action: "Complete" | "Cancel" | "NoShow"; note?: string }) =>
  put("Interview/status", dto);

export const submitInterviewFeedback = (dto: {
  panelistId: string;
  entries: { criterionId?: string; criterionName?: string; score: number; comments?: string }[];
}) => put("Interview/feedback", dto);

export const deleteInterview = createDeleteService("Interview");

/* ---- Offers (HC111–HC114) ------------------------------------------------------------ */

export const getJobOffers = (applicationId: string) =>
  api.get<JobOfferModel[]>(`JobOffer?applicationId=${applicationId}`);

/** Vacancy-derived defaults for a new offer: position scale + amount, unit-hierarchy manager. */
export const getOfferDefaults = (applicationId: string) =>
  api.get<OfferDefaultsModel>(`JobOffer/defaults?applicationId=${applicationId}`);

export const saveJobOffer = (data: JobOfferModel) =>
  saveJson("JobOffer", {
    ...data,
    salary: Number(String(data.salary ?? "").replace(/[,\s]/g, "")) || 0,
  });

export const submitJobOffer = (id: string) => post(`JobOffer/${id}/submit`);
export const sendJobOffer = (id: string) => post(`JobOffer/${id}/send`);
export const respondJobOffer = (dto: { id: string; response: "Accept" | "Decline"; note?: string }) =>
  put("JobOffer/respond", dto);
export const withdrawJobOffer = (id: string, note?: string) =>
  post(`JobOffer/${id}/withdraw`, { note: note || null });
export const generateOfferLetter = (id: string) => api.get<string>(`JobOffer/${id}/generate-letter`);
export const deleteJobOffer = createDeleteService("JobOffer");

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

/** Mass stage move — movable applications move; the rest come back with reasons. */
export async function bulkMoveApplicationStage(dto: {
  ids: string[];
  stage: string;
  note?: string;
}): Promise<{ ok: boolean; message: string; result?: BulkMoveResultModel }> {
  try {
    const result = await api.put<BulkMoveResultModel>("JobApplication/stage/bulk", dto);
    return { ok: true, message: "", result };
  } catch (e) {
    return { ok: false, message: errorMessageParser(e) || (e as Error).message };
  }
}
