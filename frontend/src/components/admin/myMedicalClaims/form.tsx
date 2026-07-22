"use client";
import { memo, useCallback, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Paperclip, Trash2 } from "lucide-react";
import FormProviders from "@/components/common/formProvider/formProvider";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { getMyMedicalEnrollments, submitMedicalClaim, fileToBase64 } from "@/services/admin/medical";

const FormProvider = memo(FormProviders);
const iso = (d: Date) => d.toISOString().slice(0, 10);
const NEW_DEFAULTS = { medicalEnrollmentId: "", medicalBeneficiaryId: "", serviceDate: iso(new Date()), claimedAmount: "", diagnosis: "", description: "" };

/** HC240 — the signed-in employee's medical claim request form (plan, beneficiary, service, receipts). */
function MyMedicalClaimForm({ onDone }: { onDone: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();

  const [formState, setFormState] = useState<any>({});
  const [form, setForm] = useState({ ...NEW_DEFAULTS });
  const [files, setFiles] = useState<File[]>([]);
  const [busy, setBusy] = useState(false);

  const { data: enrollments, isLoading } = useQuery({ queryKey: ["myMedicalEnrollmentOpts"], queryFn: getMyMedicalEnrollments, staleTime: 60_000 });
  const active = (enrollments ?? []).filter((e) => e.status === "Active");
  const selEnrollment = active.find((e) => e.id === form.medicalEnrollmentId);
  const beneficiaries = (selEnrollment?.beneficiaries ?? []).filter((b) => b.isActive);
  const planOptions = active.map((e) => ({ id: e.id!, name: e.medicalPlanName ?? "" }));
  const beneficiaryOptions = beneficiaries.map((b) => ({ id: b.id!, name: `${b.fullName} (${t(b.category ?? "")})` }));

  const changeHandler = useCallback((e: any) => setForm((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const planSelect = useCallback((_name: string, r: any) => setForm((p) => ({ ...p, medicalEnrollmentId: r.id, medicalBeneficiaryId: "" })), []);
  const beneficiarySelect = useCallback((_name: string, r: any) => setForm((p) => ({ ...p, medicalBeneficiaryId: r.id })), []);
  const fileHandler = useCallback((e: any) => {
    const picked = Array.from(e.target.files ?? []) as File[];
    if (picked.length) setFiles((prev) => [...prev, ...picked]);
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    if (!form.medicalEnrollmentId || !form.medicalBeneficiaryId || form.claimedAmount === "" || !form.description.trim()) {
      setFormState({ status: "error", message: t("Please fill the plan, beneficiary, amount and description."), zodErrors: {} });
      return;
    }
    setBusy(true);
    const attachments = await Promise.all(files.map(async (f) => ({ fileName: f.name, contentType: f.type, contentBase64: await fileToBase64(f) })));
    const res = await submitMedicalClaim({
      medicalEnrollmentId: form.medicalEnrollmentId,
      medicalBeneficiaryId: form.medicalBeneficiaryId,
      serviceDate: form.serviceDate,
      claimedAmount: Number(form.claimedAmount),
      description: form.description.trim(),
      diagnosis: form.diagnosis.trim() || undefined,
      attachments,
    });
    setBusy(false);
    setFormState({ status: res.ok ? "success" : "error", message: res.message, zodErrors: {} });
    if (res.ok) {
      setForm({ ...NEW_DEFAULTS });
      setFiles([]);
      queryClient.invalidateQueries({ queryKey: ["myMedicalClaims"] });
      onDone();
    }
  };

  if (isLoading) return <Loading />;
  if (active.length === 0)
    return <p className="m-4 rounded-lg border border-dashed border-border p-6 text-center text-sm text-muted">{t("You have no active medical coverage. Contact HR to enroll.")}</p>;

  return (
    <div className="text-white">
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: busy,
          SubmitButton: "top",
          submitBtnTitle: "Submit claim",
          components: [
            { name: "medicalEnrollmentId", label: "Plan", required: true, type: "dropDown", onSelect: planSelect, value: form.medicalEnrollmentId, displayValue: planOptions.find((o) => o.id === form.medicalEnrollmentId)?.name, data: planOptions as never },
            { name: "medicalBeneficiaryId", label: "Beneficiary", required: true, type: "dropDown", onSelect: beneficiarySelect, disabled: !selEnrollment, value: form.medicalBeneficiaryId, displayValue: beneficiaryOptions.find((o) => o.id === form.medicalBeneficiaryId)?.name, data: beneficiaryOptions as never },
            { name: "serviceDate", label: "Service Date", required: true, type: "date", value: form.serviceDate, onChange: changeHandler },
            { name: "claimedAmount", label: "Claimed Amount", required: true, value: form.claimedAmount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "diagnosis", label: "Diagnosis", value: form.diagnosis, onChange: changeHandler, type: "text" },
            { name: "description", label: "Description", required: true, value: form.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "attachments", label: "Attachments (receipts, prescriptions — max 5 MB each)", type: "file", onChange: fileHandler, colSpan: "full" },
          ],
        }}
      >
        {selEnrollment && (
          <div className="mt-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
            {t("Plan")}: <b className="text-foreground">{selEnrollment.medicalPlanName}</b>
            {selEnrollment.coverageStart ? ` · ${t("since")} ${selEnrollment.coverageStart.slice(0, 10)}` : ""}
            {` · ${beneficiaries.length} ${t("beneficiaries")}`}
          </div>
        )}

        <div className="mt-3">
          <div className="mb-1 flex items-center gap-2">
            <Paperclip size={13} className="text-muted" />
            <span className="text-xs font-semibold uppercase tracking-wide text-muted">{t("Attached files")}</span>
          </div>
          {files.length > 0 ? (
            <div className="flex flex-wrap gap-2">
              {files.map((f, i) => (
                <span key={i} className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs">
                  <Paperclip size={12} /> {f.name}
                  <button type="button" onClick={() => setFiles((p) => p.filter((_, j) => j !== i))} className="text-error hover:opacity-80">
                    <Trash2 size={12} />
                  </button>
                </span>
              ))}
            </div>
          ) : (
            <span className="text-xs text-muted">{t("No files attached.")}</span>
          )}
        </div>
      </FormProvider>
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default MyMedicalClaimForm;
