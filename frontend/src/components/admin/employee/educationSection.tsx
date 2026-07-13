"use client";
import { memo, useMemo } from "react";
import type { EmployeeEducationModel } from "@/models";
import { getEducations, saveEducation, deleteEducation } from "@/services/admin/employee/children";
import EducationSection from "@/components/common/personBackground/educationSection";
import type { BackgroundDataSource } from "@/components/common/personBackground/types";
import DocumentAttachments from "./documentAttachments";

/** Employee education — the SAME shared Education section the Candidate module uses. */
function EmployeeEducationSection({ employeeId }: { employeeId: string }) {
  const ds: BackgroundDataSource<EmployeeEducationModel> = useMemo(
    () => ({
      ownerId: employeeId,
      queryKey: ["employeeEducations", employeeId],
      list: () => getEducations(employeeId),
      save: (fd) => saveEducation(fd),
      remove: (id) => deleteEducation(id),
      ownerIdField: { name: "employeeId", value: employeeId },
      renderAttachments: (recordId) => (
        <DocumentAttachments employeeId={employeeId} ownerType="Education" ownerId={recordId} />
      ),
    }),
    [employeeId],
  );
  return <EducationSection ds={ds} />;
}

export default memo(EmployeeEducationSection);
