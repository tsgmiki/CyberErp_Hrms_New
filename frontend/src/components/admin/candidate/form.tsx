"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useRef, useState } from "react";
import type { CandidateModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Upload, FileText, Star, ShieldOff, Download, Trash2, BadgeCheck, UserCheck } from "lucide-react";
import Modal from "@/components/common/modal";
import Loading from "../../common/loader/loader";
import {
  getCandidate,
  saveCandidate,
  uploadCandidateResume,
  candidateResumeUrl,
  setCandidateTalentPool,
  anonymizeCandidate,
  getCandidateDocuments,
  uploadCandidateDocument,
  deleteCandidateDocument,
  candidateDocumentUrl,
  hireCandidate,
} from "@/services/admin/recruitment";
import getAllEmployee from "@/services/admin/employee/getAll";
import getAllPosition from "@/services/admin/position/getAll";
import { parameterInitialData } from "@/constants/initialization";
import {
  genderOptions,
  candidateSourceOptions,
  candidateDocumentTypeOptions,
  candidateDocumentTypeLabel,
  employmentNatureOptions,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

function CandidateForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<CandidateModel>({ source: "External", consentGiven: false });
  const [busy, setBusy] = useState(false);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [poolNotes, setPoolNotes] = useState("");
  const [confirmAnonymize, setConfirmAnonymize] = useState(false);
  const [docType, setDocType] = useState("NationalId");
  const [showHire, setShowHire] = useState(false);
  const [hire, setHire] = useState({
    employeeNumber: "",
    hireDate: "",
    positionId: "",
    employmentNature: "Permanent",
    contractPeriod: "",
    isProbation: false,
    probationEndDate: "",
    salary: "",
  });
  const [hireError, setHireError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const docInputRef = useRef<HTMLInputElement>(null);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["candidate", id],
    queryFn: () => getCandidate(id),
    enabled: typeof id != "undefined" && id != "",
  });
  const { data: employees } = useQuery({
    queryKey: ["employees", lookupParam],
    queryFn: () => getAllEmployee(lookupParam),
  });
  const { data: documents } = useQuery({
    queryKey: ["candidateDocuments", id],
    queryFn: () => getCandidateDocuments(id),
    enabled: typeof id != "undefined" && id != "",
  });
  const { data: vacantPositions } = useQuery({
    queryKey: ["positions", "vacant-hire"],
    queryFn: () => getAllPosition({ ...parameterInitialData, take: 200, isVacant: true } as never),
    enabled: showHire,
  });

  const anonymized = !!record?.anonymizedAt;
  const hired = !!record?.hiredEmployeeId;

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData(record);
      setPoolNotes(record.talentPoolNotes ?? "");
    }
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["candidates"] });
      if (!formData.id && formState.id) {
        setId(formState.id);
        queryClient.invalidateQueries({ queryKey: ["candidate", formState.id] });
      } else if (formData.id) {
        queryClient.invalidateQueries({ queryKey: ["candidate", formData.id] });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id, [`${name.replace(/Id$/, "")}Name`]: r.name }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const result = await saveCandidate(formData);
    setFormState(result);
    setIsLoading(false);
  };

  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["candidate", id] });
    queryClient.invalidateQueries({ queryKey: ["candidates"] });
  }, [queryClient, id]);

  const onResumePicked = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file || !id) return;
    setBusy(true);
    const res = await uploadCandidateResume(id, file);
    setBusy(false);
    setActionMessage(res.message);
    refresh();
  };

  const togglePool = async () => {
    setBusy(true);
    const res = await setCandidateTalentPool(id, !(record?.isInTalentPool === true), poolNotes);
    setBusy(false);
    setActionMessage(res.message);
    refresh();
  };

  const doAnonymize = async () => {
    setConfirmAnonymize(false);
    setBusy(true);
    const res = await anonymizeCandidate(id);
    setBusy(false);
    setActionMessage(res.message);
    refresh();
  };

  const onDocumentPicked = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file || !id) return;
    setBusy(true);
    const res = await uploadCandidateDocument(id, docType, file);
    setBusy(false);
    setActionMessage(res.message);
    queryClient.invalidateQueries({ queryKey: ["candidateDocuments", id] });
    refresh(); // compliance status may change
  };

  const removeDocument = async (documentId: string) => {
    setBusy(true);
    const res = await deleteCandidateDocument(documentId);
    setBusy(false);
    setActionMessage(res.message);
    queryClient.invalidateQueries({ queryKey: ["candidateDocuments", id] });
    refresh();
  };

  const doHire = async () => {
    setHireError(null);
    if (!hire.employeeNumber.trim()) {
      setHireError(t("An employee number is required."));
      return;
    }
    setBusy(true);
    const res = await hireCandidate(id, {
      employeeNumber: hire.employeeNumber.trim(),
      hireDate: hire.hireDate || undefined,
      positionId: hire.positionId || undefined,
      salary: hire.salary === "" ? undefined : Number(String(hire.salary).replace(/[,\s]/g, "")),
      employmentNature: hire.employmentNature,
      contractPeriod: hire.contractPeriod === "" ? undefined : Number(hire.contractPeriod),
      isProbation: hire.isProbation,
      probationEndDate: hire.probationEndDate || undefined,
    });
    setBusy(false);
    if (!res.ok) {
      setHireError(res.message);
      return;
    }
    setShowHire(false);
    setActionMessage(t("Hired — employee record created with the candidate's person and documents."));
    queryClient.invalidateQueries({ queryKey: ["employees"] });
    queryClient.invalidateQueries({ queryKey: ["jobApplications"] });
    refresh();
  };

  return (
    <div className="text-foreground">
      {pending && <Loading />}

      {record && (
        <div className="mb-2 flex flex-wrap items-center gap-2 text-sm">
          <span className="font-semibold">{record.candidateNumber}</span>
          {record.isInTalentPool && (
            <span className="flex items-center gap-1 rounded bg-warning/15 px-2 py-0.5 text-xs font-semibold text-warning">
              <Star size={12} /> {t("Talent Pool")}
            </span>
          )}
          {anonymized && (
            <span className="rounded bg-muted/30 px-2 py-0.5 text-xs text-muted">{t("Anonymized")}</span>
          )}
          {hired && (
            <span className="flex items-center gap-1 rounded bg-success/15 px-2 py-0.5 text-xs font-semibold text-success">
              <BadgeCheck size={12} /> {t("Hired")}
            </span>
          )}
          {record && !hired && !anonymized && (
            <span
              className={`rounded px-2 py-0.5 text-xs font-semibold ${
                record.complianceComplete ? "bg-success/15 text-success" : "bg-warning/15 text-warning"
              }`}
              title={
                record.complianceComplete
                  ? t("All mandatory compliance documents are on file")
                  : `${t("Missing")}: ${(record.missingComplianceDocuments ?? []).join(", ")}`
              }
            >
              {record.complianceComplete ? t("Compliance Complete") : t("Compliance Incomplete")}
            </span>
          )}
          {record.consentAt && !anonymized && (
            <span
              className="rounded bg-success/15 px-2 py-0.5 text-xs text-success"
              title={t("Data-processing consent recorded (HC097)")}
            >
              {t("Consent")}: {record.consentAt.slice(0, 10)}
            </span>
          )}
        </div>
      )}

      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: (anonymized ? "none" : "top") as "top",
          submitBtnTitle: "Save Candidate",
          components: [
            {
              name: "firstName", label: "First Name", required: true, type: "text",
              value: formData.firstName, onChange: changeHandler, disabled: anonymized,
              error: formState?.zodErrors?.firstName,
            },
            {
              name: "fatherName", label: "Father Name", type: "text",
              value: formData.fatherName, onChange: changeHandler, disabled: anonymized,
            },
            {
              name: "grandFatherName", label: "Grandfather Name", type: "text",
              value: formData.grandFatherName, onChange: changeHandler, disabled: anonymized,
            },
            {
              name: "gender", label: "Gender", type: "dropDown", onSelect: selectHandler,
              value: formData.gender, displayValue: formData.gender, disabled: anonymized,
              data: genderOptions as never,
            },
            {
              name: "email", label: "Email", type: "text",
              value: formData.email, onChange: changeHandler, disabled: anonymized,
              error: formState?.zodErrors?.email,
            },
            {
              name: "phoneNumber", label: "Phone", type: "text",
              value: formData.phoneNumber, onChange: changeHandler, disabled: anonymized,
            },
            {
              name: "source", label: "Source", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.source,
              displayValue: candidateSourceOptions.find((o) => o.id === formData.source)?.name,
              disabled: anonymized, data: candidateSourceOptions as never,
            },
            {
              name: "internalEmployeeId", label: "Internal Employee", type: "dropDown",
              onSelect: selectHandler, value: formData.internalEmployeeId,
              displayValue: formData.internalEmployeeName, disabled: anonymized,
              placeholder: "Required for Internal source (HC090)",
              data: (employees?.data ?? []).map((e) => ({ id: e.id, name: e.fullName ?? e.employeeNumber })) as never,
            },
            {
              name: "yearsOfExperience", label: "Years of Experience", type: "text",
              value: formData.yearsOfExperience, onChange: changeHandler, disabled: anonymized,
            },
            {
              name: "educationSummary", label: "Education", type: "textarea", colSpan: "full",
              placeholder: "Degrees, institutions, graduation years (parsed from the resume later — HC094)",
              value: formData.educationSummary, onChange: changeHandler, disabled: anonymized,
            },
            {
              name: "experienceSummary", label: "Experience", type: "textarea", colSpan: "full",
              value: formData.experienceSummary, onChange: changeHandler, disabled: anonymized,
            },
            {
              name: "skillsSummary", label: "Skills (comma-separated)", type: "text", colSpan: "full",
              placeholder: "Drives vacancy matching (HC090)",
              value: formData.skillsSummary, onChange: changeHandler, disabled: anonymized,
            },
          ],
        }}
      />

      {/* Consent (HC097) — captured at creation, mandatory */}
      {!record && (
        <label className="mt-2 flex items-start gap-2 rounded-md border border-border bg-card px-3 py-2 text-sm">
          <input
            type="checkbox"
            checked={formData.consentGiven === true}
            onChange={(e) => setFormData((p) => ({ ...p, consentGiven: e.target.checked }))}
            className="mt-0.5"
          />
          <span>
            {t("The candidate consents to the collection, processing and storage of their personal data for recruitment purposes (HC097).")}
            <span className="text-error"> *</span>
          </span>
        </label>
      )}

      {/* Resume + talent pool + retention actions */}
      {record && !anonymized && (
        <div className="mt-3 flex flex-wrap items-center gap-2">
          <input ref={fileInputRef} type="file" accept=".pdf,.doc,.docx" className="hidden" onChange={onResumePicked} />
          <button
            type="button"
            disabled={busy}
            onClick={() => fileInputRef.current?.click()}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
          >
            <Upload size={14} /> {record.resumeFileName ? t("Replace Resume") : t("Upload Resume")}
          </button>
          {record.resumeFileName && (
            <a
              href={candidateResumeUrl(id)}
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary"
            >
              <FileText size={14} /> {t("View Resume")}
            </a>
          )}
          <span className="mx-1 h-5 w-px bg-border" />
          <input
            type="text"
            value={poolNotes}
            disabled={busy}
            onChange={(e) => setPoolNotes(e.target.value)}
            placeholder={t("Talent pool notes (HC089)")}
            className="h-8 w-56 rounded-md border border-border bg-background px-2 text-xs text-foreground disabled:opacity-50"
          />
          <button
            type="button"
            disabled={busy}
            onClick={togglePool}
            className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-medium disabled:opacity-50 ${
              record.isInTalentPool
                ? "border border-warning/40 bg-warning/10 text-warning hover:bg-warning/20"
                : "border border-border text-foreground hover:border-warning hover:text-warning"
            }`}
          >
            <Star size={14} /> {record.isInTalentPool ? t("Remove from Talent Pool") : t("Add to Talent Pool")}
          </button>
          <span className="mx-1 h-5 w-px bg-border" />
          <button
            type="button"
            disabled={busy}
            onClick={() => setConfirmAnonymize(true)}
            title={t("Retention policy: scrubs all personal data irreversibly (HC097)")}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-muted hover:border-error hover:text-error disabled:opacity-50"
          >
            <ShieldOff size={14} /> {t("Anonymize")}
          </button>
          {actionMessage && <span className="text-xs text-muted">{actionMessage}</span>}
        </div>
      )}

      {/* Documents & compliance (requirements #3/#5): typed attachments; the mandatory set gates hire */}
      {record && !anonymized && (
        <div className="mt-3 rounded-lg border border-border bg-card p-3">
          <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
            <h4 className="text-sm font-semibold">
              {t("Documents")}{" "}
              <span className="text-xs font-normal text-muted">
                ({t("migrated to the employee record automatically at hire")})
              </span>
            </h4>
            {!hired && (
              <div className="flex items-center gap-2">
                <select
                  value={docType}
                  disabled={busy}
                  onChange={(e) => setDocType(e.target.value)}
                  className="h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground"
                >
                  {candidateDocumentTypeOptions.map((o) => (
                    <option key={o.id} value={o.id}>{o.name}</option>
                  ))}
                </select>
                <input ref={docInputRef} type="file" className="hidden" onChange={onDocumentPicked} />
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => docInputRef.current?.click()}
                  className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-primary hover:text-primary disabled:opacity-50"
                >
                  <Upload size={14} /> {t("Upload")}
                </button>
              </div>
            )}
          </div>
          {(record.missingComplianceDocuments?.length ?? 0) > 0 && !hired && (
            <p className="mb-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-xs text-foreground">
              {t("Mandatory documents missing before hire")}:{" "}
              <strong>{record.missingComplianceDocuments!.join(", ")}</strong>
            </p>
          )}
          {(documents ?? []).length === 0 && (
            <p className="text-xs text-muted">{t("No documents attached yet.")}</p>
          )}
          {(documents ?? []).map((d) => (
            <div key={d.id} className="mb-1 flex items-center gap-2 rounded-md border border-border/60 px-3 py-1.5 text-sm">
              <FileText size={14} className="shrink-0 text-muted" />
              <span className="rounded bg-secondary px-1.5 py-0.5 text-[11px] font-medium text-muted">
                {candidateDocumentTypeLabel(d.documentType)}
              </span>
              <span className="min-w-0 flex-1 truncate text-foreground">{d.fileName}</span>
              <span className="text-[11px] text-muted">{Math.ceil(d.fileSize / 1024)} KB</span>
              <a
                href={candidateDocumentUrl(d.id)}
                target="_blank"
                rel="noreferrer"
                className="rounded p-1 text-muted hover:bg-secondary hover:text-primary"
                title={t("Download")}
              >
                <Download size={14} />
              </a>
              {!hired && (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => removeDocument(d.id)}
                  className="rounded p-1 text-muted hover:bg-error/10 hover:text-error"
                  title={t("Delete")}
                >
                  <Trash2 size={14} />
                </button>
              )}
            </div>
          ))}
          {!hired && (
            <div className="mt-2 flex items-center gap-2">
              <button
                type="button"
                disabled={busy || !record.complianceComplete}
                onClick={() => {
                  setHireError(null);
                  setShowHire(true);
                }}
                title={
                  record.complianceComplete
                    ? t("Creates the employee on the candidate's person record and migrates all documents")
                    : t("Complete the mandatory compliance documents first")
                }
                className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <UserCheck size={14} /> {t("Hire as Employee")}
              </button>
              <span className="text-[11px] text-muted">
                {t("Requires an application at the Selected stage.")}
              </span>
            </div>
          )}
        </div>
      )}

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Hire modal — employee created on the SAME person record (requirement #2) */}
      {showHire && (
        <Modal
          visible
          size="md"
          title={t("Hire as Employee")}
          description={record?.fullName}
          onClose={() => setShowHire(false)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setShowHire(false)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={busy}
                onClick={doHire}
                className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
              >
                <UserCheck size={15} /> {t("Confirm Hire")}
              </button>
            </>
          }
        >
          <div className="space-y-2 text-sm">
            <p className="rounded-md border border-info/30 bg-info/10 px-3 py-2 text-xs text-foreground">
              {t("The employee is created on the candidate's existing person record — no re-entry. All attached documents (and the resume) migrate to the employee history automatically.")}
            </p>
            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                  {t("Employee Number")} <span className="text-error">*</span>
                </label>
                <input
                  type="text"
                  value={hire.employeeNumber}
                  onChange={(e) => setHire((p) => ({ ...p, employeeNumber: e.target.value }))}
                  className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Hire Date")}</label>
                <input
                  type="date"
                  value={hire.hireDate}
                  onChange={(e) => setHire((p) => ({ ...p, hireDate: e.target.value }))}
                  className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
                />
              </div>
            </div>
            <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Position (vacant)")}</label>
            <select
              value={hire.positionId}
              onChange={(e) => setHire((p) => ({ ...p, positionId: e.target.value }))}
              className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
            >
              <option value="">{t("Assign later (onboarding)")}</option>
              {(vacantPositions?.data ?? []).map((p) => (
                <option key={p.id} value={p.id}>
                  {p.code} — {p.positionClassTitle ?? ""}
                </option>
              ))}
            </select>
            <div className="grid grid-cols-2 gap-2">
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Nature")}</label>
                <select
                  value={hire.employmentNature}
                  onChange={(e) => setHire((p) => ({ ...p, employmentNature: e.target.value }))}
                  className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
                >
                  {employmentNatureOptions.map((o) => (
                    <option key={o.id} value={o.id}>{o.name}</option>
                  ))}
                </select>
              </div>
              {hire.employmentNature === "Contract" ? (
                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                    {t("Contract (Months)")} <span className="text-error">*</span>
                  </label>
                  <input
                    type="text"
                    value={hire.contractPeriod}
                    onChange={(e) => setHire((p) => ({ ...p, contractPeriod: e.target.value }))}
                    className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
                  />
                </div>
              ) : (
                <div>
                  <label className="block text-xs font-semibold uppercase tracking-wide text-muted">{t("Salary")}</label>
                  <input
                    type="text"
                    value={hire.salary}
                    onChange={(e) => setHire((p) => ({ ...p, salary: e.target.value }))}
                    className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
                  />
                </div>
              )}
            </div>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={hire.isProbation}
                onChange={(e) => setHire((p) => ({ ...p, isProbation: e.target.checked }))}
              />
              {t("Start probation tracking")}
            </label>
            {hire.isProbation && (
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wide text-muted">
                  {t("Probation End Date")} <span className="text-error">*</span>
                </label>
                <input
                  type="date"
                  value={hire.probationEndDate}
                  onChange={(e) => setHire((p) => ({ ...p, probationEndDate: e.target.value }))}
                  className="h-9 w-full rounded-md border border-border bg-background px-2 text-sm text-foreground"
                />
              </div>
            )}
            {hireError && <p className="text-xs text-error">{hireError}</p>}
          </div>
        </Modal>
      )}

      {confirmAnonymize && (
        <Modal
          visible
          size="md"
          title={t("Anonymize Candidate")}
          description={record?.fullName}
          onClose={() => setConfirmAnonymize(false)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setConfirmAnonymize(false)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Cancel")}
              </button>
              <button
                type="button"
                disabled={busy}
                onClick={doAnonymize}
                className="rounded-md bg-error px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
              >
                {t("Anonymize Permanently")}
              </button>
            </>
          }
        >
          <p className="text-sm text-foreground">
            {t("All personal data (names, contacts, summaries and the resume file) will be scrubbed irreversibly; the anonymous application history is kept for statistics (HC097).")}
          </p>
        </Modal>
      )}
    </div>
  );
}

export default CandidateForm;
