"use client";

import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import Modal from "@/components/ui/modal";
import Loading from "@/components/common/loader/loader";
import { reviewHiringRequest, reviewJobRequisition } from "@/services/admin/recruitment";
import type { HiringRequestModel, JobRequisitionModel } from "@/models";

export type RecruitmentKind = "hiringRequest" | "jobRequisition";

interface Props {
  kind: RecruitmentKind;
  requestId: string;
  onClose: () => void;
}

const day = (v?: string) => (v ? String(v).slice(0, 10) : "—");
/** "PendingApproval" → "Pending Approval". Statuses are stored PascalCase; nobody reads them that way. */
const spaced = (v?: string) => (v ?? "").replace(/([a-z])([A-Z])/g, "$1 $2");
const num = (v?: number | string | null) =>
  v === null || v === undefined || v === "" ? "—" : Number(v).toLocaleString();
const text = (v?: string | null) => (v && String(v).trim() ? String(v) : "—");

/** One label/value pair in the attribute grid. */
const Attr = memo(function Attr({ label, value }: { label: string; value: string }) {
  return (
    <div className="min-w-0">
      <p className="truncate text-[10px] font-medium uppercase tracking-wide text-muted">{label}</p>
      <p className="truncate text-[13px] font-semibold text-foreground" title={value}>{value}</p>
    </div>
  );
});

/** A long free-text field (justification, description) — wrapped, not truncated. */
const Prose = memo(function Prose({ label, value }: { label: string; value?: string | null }) {
  if (!value || !String(value).trim()) return null;
  return (
    <div>
      <p className="mb-1 text-[10px] font-medium uppercase tracking-wide text-muted">{label}</p>
      <p className="whitespace-pre-wrap rounded-lg border border-border bg-secondary/10 px-3 py-2 text-[13px] text-foreground">
        {value}
      </p>
    </div>
  );
});

/**
 * What the APPROVER is being asked to decide on — the full hiring request or job requisition,
 * on the Home page, without leaving Workflow Tracking.
 *
 * <p>Before this the row said only "Hiring Need — HRQ-0004: 1 × role (budget 0)": no unit, no role,
 * no justification, no establishment position. An approver could approve or reject, but could not
 * read what they were deciding.</p>
 *
 * <p>⚠️ It reads the <c>/review</c> endpoints, NOT the ordinary GET-by-id. Those are gated on the
 * recruitment operations, and the approval chain runs Immediate Manager → HR → Finance — a Finance
 * approver holds neither, so the plain endpoint would answer 403 and this panel would render empty.
 * The review endpoints authorise on being the current approver instead.</p>
 */
function RecruitmentDetails({ kind, requestId, onClose }: Props) {
  const { t } = useTranslation();
  const isHiring = kind === "hiringRequest";

  const { data, isLoading, isError } = useQuery({
    queryKey: [isHiring ? "hiringRequestReview" : "jobRequisitionReview", requestId],
    queryFn: () =>
      isHiring
        ? (reviewHiringRequest(requestId) as Promise<HiringRequestModel | JobRequisitionModel>)
        : (reviewJobRequisition(requestId) as Promise<HiringRequestModel | JobRequisitionModel>),
    retry: false,
  });

  const hiring = isHiring ? (data as HiringRequestModel | undefined) : undefined;
  const req = !isHiring ? (data as JobRequisitionModel | undefined) : undefined;

  return (
    <Modal
      isOpen
      onClose={onClose}
      size="lg"
      title={(isHiring ? t("Hiring Request Details") : t("Job Requisition Details")) ?? undefined}
    >
      {isLoading ? (
        <Loading />
      ) : isError || !data ? (
        <p className="py-8 text-center text-sm text-muted">
          {t("These details could not be loaded. You may no longer be the approver for this request.")}
        </p>
      ) : (
        <div className="space-y-3">
          {/* Object header — identity and status first, then the decision-relevant figures. */}
          <div className="rounded-lg border border-border bg-secondary/20 px-3 py-2.5">
            <div className="flex flex-wrap items-center gap-2">
              <h3 className="text-sm font-bold text-foreground">
                {isHiring
                  ? hiring?.requestNumber
                  : `${req?.requisitionNumber ?? ""}${req?.title ? ` — ${req.title}` : ""}`}
              </h3>
              <span className="rounded-full bg-warning/15 px-2 py-0.5 text-[11px] font-semibold text-warning">
                {t(spaced(data.status))}
              </span>
            </div>

            <div className="mt-2 grid grid-cols-2 gap-x-4 gap-y-2 sm:grid-cols-4">
              <Attr label={t("Requesting Unit")} value={text(data.organizationUnitName)} />
              <Attr label={t("Role")} value={text(data.positionClassTitle)} />
              <Attr label={t("Job Grade")} value={text(data.jobGradeName)} />
              <Attr label={t("Positions")} value={num(data.numberOfPositions)} />
              <Attr label={t("Employment Type")} value={text(data.employmentType)} />
              <Attr label={t("Submitted")} value={day(data.submittedAt)} />

              {isHiring ? (
                <>
                  <Attr label={t("Estimated Budget")} value={num(hiring?.estimatedBudget)} />
                  <Attr label={t("Expected Start")} value={day(hiring?.expectedStartDate)} />
                  <Attr label={t("Workforce Plan")} value={text(hiring?.workforcePlanName)} />
                  {/* The establishment position: what the request is checked against on submit. */}
                  <Attr label={t("Vacant Seats")} value={num(hiring?.vacantSeats)} />
                  <Attr label={t("Already Requisitioned")} value={num(hiring?.requisitionedPositions)} />
                </>
              ) : (
                <>
                  <Attr label={t("From Hiring Request")} value={text(req?.hiringRequestNumber)} />
                  <Attr label={t("Work Location")} value={text(req?.workLocationName)} />
                  <Attr label={t("Min. Experience")} value={
                    req?.minExperienceYears == null ? "—" : `${req.minExperienceYears} ${t("years")}`} />
                  <Attr label={t("Salary Scale")} value={num(req?.salaryScaleAmount)} />
                  <Attr label={t("Posting Channel")} value={text(req?.postingChannel)} />
                  <Attr label={t("Open From")} value={day(req?.openFrom)} />
                  <Attr label={t("Open Until")} value={day(req?.openUntil)} />
                </>
              )}
            </div>
          </div>

          {/* The reasoning — the part an approver actually weighs. */}
          {isHiring ? (
            <>
              <Prose label={t("Justification")} value={hiring?.justification} />
              <Prose label={t("Job Requirements")} value={hiring?.jobRequirements} />
              <Prose label={t("Timeline Remarks")} value={hiring?.timelineRemarks} />
            </>
          ) : (
            <>
              <Prose label={t("Description")} value={req?.description} />
              <Prose label={t("Minimum Qualifications")} value={req?.minQualifications} />
              <Prose label={t("Skills")} value={req?.skills} />
              <Prose label={t("Posting Text")} value={req?.postingText} />
            </>
          )}
        </div>
      )}
    </Modal>
  );
}

export default memo(RecruitmentDetails);
