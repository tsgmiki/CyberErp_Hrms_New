"use client";
import { memo, useMemo } from "react";
import type { EmployeeExperienceModel } from "@/models";
import { getExperiences, saveExperience, deleteExperience } from "@/services/admin/employee/children";
import ExperienceSection from "@/components/common/personBackground/experienceSection";
import type { BackgroundDataSource } from "@/components/common/personBackground/types";
import DocumentAttachments from "./documentAttachments";

/** Employee work history — the SAME shared Experience section the Candidate module uses. */
function EmployeeExperienceSection({ employeeId }: { employeeId: string }) {
  const ds: BackgroundDataSource<EmployeeExperienceModel> = useMemo(
    () => ({
      ownerId: employeeId,
      queryKey: ["employeeExperiences", employeeId],
      list: () => getExperiences(employeeId),
      save: (fd) => saveExperience(fd),
      remove: (id) => deleteExperience(id),
      ownerIdField: { name: "employeeId", value: employeeId },
      renderAttachments: (recordId) => (
        <DocumentAttachments employeeId={employeeId} ownerType="Experience" ownerId={recordId} />
      ),
    }),
    [employeeId],
  );
  return <ExperienceSection ds={ds} />;
}

export default memo(EmployeeExperienceSection);
