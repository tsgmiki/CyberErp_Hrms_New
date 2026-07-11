"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useRef, useState } from "react";
import type { CandidateModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  Upload,
  FileText,
  Star,
  ShieldOff,
  Download,
  Trash2,
  BadgeCheck,
  UserCheck,
  UserRound,
  GraduationCap,
  BriefcaseBusiness,
} from "lucide-react";
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
} from "@/services/admin/recruitment";
import getAllEmployee from "@/services/admin/employee/getAll";
import getEmployee from "@/services/admin/employee/get";
import CandidateEducationSection from "./educationSection";
import CandidateExperienceSection from "./experienceSection";
import { parameterInitialData } from "@/constants/initialization";
import {
  genderOptions,
  candidateSourceOptions,
  candidateDocumentTypeOptions,
  candidateDocumentTypeLabel,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 100 };

// "Source" mixed the applicant TYPE (internal vs external) with acquisition CHANNELS. We split them:
// an Applicant Type switch drives everything; "Internal" is a type, the rest are external channels.
const sourceChannelOptions = candidateSourceOptions.filter((o) => o.id !== "Internal");

type TabKey = "details" | "education" | "experience";

// Same tabbed-profile structure as the Employee feature (employee/profile.tsx).
const TABS: { key: TabKey; label: string; Icon: typeof UserRound; needsId: boolean }[] = [
  { key: "details", label: "Applicant Details", Icon: UserRound, needsId: false },
  { key: "education", label: "Education", Icon: GraduationCap, needsId: true },
  { key: "experience", label: "Experience", Icon: BriefcaseBusiness, needsId: true },
];

function CandidateForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [tab, setTab] = useState<TabKey>("details");
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<CandidateModel>({ source: "External", consentGiven: false });
  const [busy, setBusy] = useState(false);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [poolNotes, setPoolNotes] = useState("");
  const [confirmAnonymize, setConfirmAnonymize] = useState(false);
  const [docType, setDocType] = useState("NationalId");
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

  // Internal applicants reuse an existing employee's identity (and their person record), so it
  // must not be re-typed; external applicants own an editable identity + an acquisition channel.
  const isInternal = formData.source === "Internal";
  const identityLocked = anonymized || isInternal;

  const setApplicantType = useCallback((type: "internal" | "external") => {
    if (type === "internal") {
      setFormData((p) => ({ ...p, source: "Internal" }));
    } else {
      // Back to external: default channel, drop the employee link and unlock identity entry.
      setFormData((p) => ({ ...p, source: "External", internalEmployeeId: undefined, internalEmployeeName: undefined }));
    }
  }, []);

  // Selecting the employee prefills + locks the identity from the employee master (requirement #2).
  const onInternalEmployeeSelect = useCallback(async (_name: string, r: any) => {
    setFormData((p) => ({ ...p, internalEmployeeId: r.id, internalEmployeeName: r.name, source: "Internal" }));
    const emp = await getEmployee(r.id);
    if (emp) {
      setFormData((p) => ({
        ...p,
        internalEmployeeId: r.id,
        internalEmployeeName: r.name,
        source: "Internal",
        firstName: emp.firstName ?? p.firstName,
        fatherName: emp.fatherName ?? p.fatherName,
        grandFatherName: emp.grandFatherName ?? p.grandFatherName,
        gender: emp.gender ?? p.gender,
        email: emp.email ?? p.email,
        phoneNumber: emp.phoneNumber ?? p.phoneNumber,
      }));
    }
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

  return (
    <div className="mx-auto max-w-5xl space-y-4 text-foreground">
      {pending && <Loading />}

      {/* Tabs — same structure as the employee profile, above the header */}
      <div className="mx-1 flex flex-wrap gap-1 border-b border-border pb-0">
        {TABS.map(({ key, label, Icon, needsId }) => {
          const disabled = needsId && !id;
          const active = tab === key;
          return (
            <button
              key={key}
              type="button"
              disabled={disabled}
              title={disabled ? t("Save the candidate first") : undefined}
              onClick={() => setTab(key)}
              className={`-mb-px flex items-center gap-1.5 rounded-t-lg border-x border-t px-3.5 py-2 text-[13px] font-medium transition-colors ${
                active
                  ? "border-border bg-card text-primary"
                  : "border-transparent text-muted hover:text-foreground"
              } ${disabled ? "cursor-not-allowed opacity-40" : ""}`}
            >
              <Icon className="h-4 w-4" />
              {t(label)}
            </button>
          );
        })}
      </div>

      {/* Header — candidate identity line + status badges (persistent across tabs) */}
      {record && (
        <div className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-border bg-card px-4 py-3">
          <div className="min-w-0">
            <h2 className="truncate text-base font-semibold text-foreground">
              {record.fullName || t("Candidate")}
            </h2>
            <p className="text-xs text-muted">{record.candidateNumber}</p>
          </div>
          <div className="flex flex-wrap items-center gap-2 text-sm">
            {record.isInTalentPool && (
              <span className="flex items-center gap-1 rounded-full bg-warning/15 px-2.5 py-0.5 text-xs font-semibold text-warning">
                <Star size={12} /> {t("Talent Pool")}
              </span>
            )}
            {anonymized && (
              <span className="rounded-full bg-muted/30 px-2.5 py-0.5 text-xs text-muted">{t("Anonymized")}</span>
            )}
            {hired && (
              <span className="flex items-center gap-1 rounded-full bg-success/15 px-2.5 py-0.5 text-xs font-semibold text-success">
                <BadgeCheck size={12} /> {t("Hired")}
              </span>
            )}
            {record && !hired && !anonymized && (
              <span
                className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${
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
                className="rounded-full bg-success/15 px-2.5 py-0.5 text-xs text-success"
                title={t("Data-processing consent recorded (HC097)")}
              >
                {t("Consent")}: {record.consentAt.slice(0, 10)}
              </span>
            )}
          </div>
        </div>
      )}

      {tab === "details" && (
      <>
      {/* Card: Applicant Details */}
      <div className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-3 border-b border-border pb-3">
          <div>
            <h3 className="text-sm font-semibold text-foreground">{t("Applicant Details")}</h3>
            <p className="text-xs text-muted">
              {isInternal
                ? t("Identity is taken from the employee record — no re-entry (HC090).")
                : t("Enter the applicant's details and how they reached us.")}
            </p>
          </div>
          {/* Applicant Type — one switch decides internal-vs-external (replaces the confusing
              "Source" list that mixed applicant type with acquisition channels).
              Unchecked = External (default) · checked = Internal. */}
          <label className="flex cursor-pointer items-center gap-2 text-xs font-medium">
            <span className={!isInternal ? "text-foreground" : "text-muted"}>{t("External")}</span>
            <button
              type="button"
              role="switch"
              aria-checked={isInternal}
              aria-label={t("Internal applicant")}
              disabled={anonymized || hired}
              onClick={() => setApplicantType(isInternal ? "external" : "internal")}
              className={`relative h-6 w-11 shrink-0 rounded-full transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${
                isInternal ? "bg-primary" : "bg-muted/40"
              }`}
            >
              <span
                className={`absolute left-0.5 top-0.5 h-5 w-5 rounded-full bg-white shadow transition-transform ${
                  isInternal ? "translate-x-5" : ""
                }`}
              />
            </button>
            <span className={isInternal ? "text-foreground" : "text-muted"}>{t("Internal")}</span>
          </label>
        </div>

      <FormProvider
        ref={formRef}
        form={{
          formId: "candidateForm",
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: (anonymized ? "none" : "top") as "top",
          submitBtnTitle: "Save Candidate",
          components: [
            // Internal applicants: pick the employee, identity prefills + locks from the employee master.
            ...(isInternal
              ? [
                  {
                    name: "internalEmployeeId", label: "Employee", required: true, type: "dropDown" as const,
                    onSelect: onInternalEmployeeSelect, value: formData.internalEmployeeId,
                    displayValue: formData.internalEmployeeName, disabled: anonymized,
                    placeholder: "Select the internal employee applying",
                    data: (employees?.data ?? []).map((e) => ({ id: e.id, name: e.fullName ?? e.employeeNumber })) as never,
                  },
                ]
              : [
                  // External applicants: choose the acquisition channel.
                  {
                    name: "source", label: "Source Channel", required: true, type: "dropDown" as const, onSelect: selectHandler,
                    value: formData.source,
                    displayValue: sourceChannelOptions.find((o) => o.id === formData.source)?.name,
                    disabled: anonymized, data: sourceChannelOptions as never,
                  },
                ]),
            {
              name: "firstName", label: "First Name", required: true, type: "text",
              value: formData.firstName, onChange: changeHandler, disabled: identityLocked,
              error: formState?.zodErrors?.firstName,
            },
            {
              name: "fatherName", label: "Father Name", type: "text",
              value: formData.fatherName, onChange: changeHandler, disabled: identityLocked,
            },
            {
              name: "grandFatherName", label: "Grandfather Name", type: "text",
              value: formData.grandFatherName, onChange: changeHandler, disabled: identityLocked,
            },
            {
              name: "gender", label: "Gender", type: "dropDown", onSelect: selectHandler,
              value: formData.gender, displayValue: formData.gender, disabled: identityLocked,
              data: genderOptions as never,
            },
            {
              name: "email", label: "Email", type: "text",
              value: formData.email, onChange: changeHandler, disabled: identityLocked,
              error: formState?.zodErrors?.email,
            },
            {
              name: "phoneNumber", label: "Phone", type: "text",
              value: formData.phoneNumber, onChange: changeHandler, disabled: identityLocked,
            },
            {
              name: "yearsOfExperience", label: "Years of Experience", type: "text",
              value: formData.yearsOfExperience, onChange: changeHandler, disabled: anonymized,
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
        <label className="mt-3 flex items-start gap-2 rounded-md border border-border bg-background px-3 py-2 text-sm">
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
      </div>
      {/* end Applicant Details card */}

      {/* Card: Resume & Retention */}
      {record && !anonymized && (
        <div className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-3 border-b border-border pb-2 text-sm font-semibold text-foreground">
            {t("Resume & Retention")}
          </h3>
          <div className="flex flex-wrap items-center gap-2">
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
        </div>
      )}

      {/* Card: Documents & compliance (requirements #3/#5): typed attachments; the mandatory set gates hire */}
      {record && !anonymized && (
        <div className="rounded-lg border border-border bg-card p-4">
          <div className="mb-3 flex flex-wrap items-center justify-between gap-2 border-b border-border pb-2">
            <h3 className="text-sm font-semibold text-foreground">
              {t("Documents & Compliance")}{" "}
              <span className="text-xs font-normal text-muted">
                ({t("migrated to the employee record automatically at hire")})
              </span>
            </h3>
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
            <p className="mt-2 flex items-center gap-1.5 text-[11px] text-muted">
              <UserCheck size={13} />
              {t("Hiring happens from the Hire Employee menu — only ranked, hire-eligible applicants appear there.")}
            </p>
          )}
        </div>
      )}

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
      </>
      )}
      {/* end Details tab */}

      {/* Education / Experience tabs — the SAME person-owned rows the employee profile uses, so at
          hire they hand off to the employee automatically (shared PersonId). Read-only for internal
          applicants (the employee master is authoritative). */}
      {tab === "education" && id && (
        <div>
          <p className="mx-1 mb-2 text-xs text-muted">
            {isInternal
              ? t("Education is maintained on the employee record and shown here read-only.")
              : t("Captured here, this data transfers to the employee record automatically when the candidate is hired.")}
          </p>
          <CandidateEducationSection candidateId={id} readOnly={isInternal} />
        </div>
      )}
      {tab === "experience" && id && (
        <div>
          <p className="mx-1 mb-2 text-xs text-muted">
            {isInternal
              ? t("Experience is maintained on the employee record and shown here read-only.")
              : t("Captured here, this data transfers to the employee record automatically when the candidate is hired.")}
          </p>
          <CandidateExperienceSection candidateId={id} readOnly={isInternal} />
        </div>
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
