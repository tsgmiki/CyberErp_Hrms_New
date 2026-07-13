"use client";
import { memo, useMemo } from "react";
import type { EmployeeEducationModel } from "@/models";
import {
  getCandidateEducations,
  saveCandidateEducation,
  deleteCandidateEducation,
} from "@/services/admin/recruitment";
import EducationSection from "@/components/common/personBackground/educationSection";
import type { BackgroundDataSource } from "@/components/common/personBackground/types";
import BackgroundAttachments from "./backgroundAttachments";

/**
 * Candidate education — renders the SAME shared Education section the Employee module uses, so the two
 * forms (fields, custom fields, attachments) are identical. Writes the shared person-owned rows; at
 * hire they appear on the employee's Education tab with no re-entry. Read-only for internal applicants.
 */
function CandidateEducationSection({ candidateId, readOnly }: { candidateId: string; readOnly?: boolean }) {
  const ds: BackgroundDataSource<EmployeeEducationModel> = useMemo(
    () => ({
      ownerId: candidateId,
      queryKey: ["candidateEducations", candidateId],
      list: () => getCandidateEducations(candidateId),
      save: (fd) => saveCandidateEducation(candidateId, fd),
      remove: (id) => deleteCandidateEducation(id),
      renderAttachments: (recordId) => (
        <BackgroundAttachments candidateId={candidateId} ownerType="Education" ownerId={recordId} readOnly={readOnly} />
      ),
      readOnly,
      hint: readOnly ? "Maintained on the employee record — read-only for internal applicants." : undefined,
    }),
    [candidateId, readOnly],
  );
  return <EducationSection ds={ds} />;
}

export default memo(CandidateEducationSection);
