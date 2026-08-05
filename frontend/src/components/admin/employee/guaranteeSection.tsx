"use client";
import { lazy, memo, useState, Suspense } from "react";
import { useTranslation } from "react-i18next";
import { ArrowLeft, Plus } from "lucide-react";
import ButtonField from "@/components/ui/buttonField";
import Loading from "../../common/loader/loader";

const GuaranteeList = memo(lazy(() => import("../employeeGuarantee/list")));
const GuaranteeForm = memo(lazy(() => import("../employeeGuarantee/form")));

/**
 * §3.12 — the employee-profile "Guarantees" tab: this employee's guarantee commitments toward
 * external organizations. Reuses the Guarantee Register list/form scoped + locked to the employee.
 */
function GuaranteeSection({ employeeId }: { employeeId: string }) {
  const { t } = useTranslation();
  const [recId, setRecId] = useState("");
  const [showForm, setShowForm] = useState(false);

  const closeForm = (nextId: string) => {
    setRecId(nextId);
    if (!nextId) setShowForm(false);
  };

  return (
    <Suspense fallback={<Loading />}>
      {showForm ? (
        <div className="space-y-3">
          <ButtonField
            value={t("Back to list")}
            icon={<ArrowLeft className="h-4 w-4" />}
            htmlType="button"
            variant="outline"
            onClick={() => { setRecId(""); setShowForm(false); }}
          />
          <GuaranteeForm id={recId} setId={closeForm} fixedEmployeeId={employeeId} />
        </div>
      ) : (
        <div className="space-y-3">
          <div className="flex justify-end">
            <ButtonField
              value={t("Add Commitment")}
              icon={<Plus className="h-4 w-4" />}
              htmlType="button"
              variant="primary"
              onClick={() => { setRecId(""); setShowForm(true); }}
            />
          </div>
          <GuaranteeList
            employeeId={employeeId}
            editHandler={(gid) => { setRecId(gid); setShowForm(true); }}
          />
        </div>
      )}
    </Suspense>
  );
}

export default memo(GuaranteeSection);
