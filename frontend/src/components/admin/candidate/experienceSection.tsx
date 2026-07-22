"use client";
import { memo, useMemo } from "react";
import type { EmployeeExperienceModel } from "@/models";
import {
  getCandidateExperiences,
  saveCandidateExperience,
  deleteCandidateExperience,
} from "@/services/admin/recruitment";
import ExperienceSection from "@/components/common/personBackground/experienceSection";
import type { BackgroundDataSource } from "@/components/common/personBackground/types";
import BackgroundAttachments from "./backgroundAttachments";

/**
 * Candidate work history — renders the SAME shared Experience section the Employee module uses, so the
 * two forms (fields, custom fields, external/governmental toggles, attachments) are identical. Writes
 * the shared person-owned rows; read-only for internal applicants.
 */
function CandidateExperienceSection({ candidateId, readOnly }: { candidateId: string; readOnly?: boolean }) {
  const ds: BackgroundDataSource<EmployeeExperienceModel> = useMemo(
    () => ({
      ownerId: candidateId,
      queryKey: ["candidateExperiences", candidateId],
      list: () => getCandidateExperiences(candidateId),
      save: (fd) => saveCandidateExperience(candidateId, fd),
      remove: (id) => deleteCandidateExperience(id),
      // The owner (candidateId) rides in the URL — no hidden owner field in the form.
      renderAttachments: (recordId) => (
        <BackgroundAttachments candidateId={candidateId} ownerType="Experience" ownerId={recordId} readOnly={readOnly} />
      ),
      readOnly,
      hint: readOnly ? "Maintained on the employee record — read-only for internal applicants." : undefined,
    }),
    [candidateId, readOnly],
  );
  return <ExperienceSection ds={ds} />;
}

export default memo(CandidateExperienceSection);
