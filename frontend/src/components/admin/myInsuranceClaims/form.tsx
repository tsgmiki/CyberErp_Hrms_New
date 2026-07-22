"use client";
import { memo, useCallback, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import FormProviders from "@/components/common/formProvider/formProvider";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { getAllInsurancePolicies, submitInsuranceClaim, fileToBase64 } from "@/services/admin/insurance";
import type { InsurancePolicyModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const iso = (d: Date) => d.toISOString().slice(0, 10);
const blank = { insurancePolicyId: "", claimType: "", incidentDate: iso(new Date()), claimedAmount: "", description: "" };

/** HC248 — the signed-in employee's insurance claim submission form. */
function MyInsuranceClaimForm({ onDone }: { onDone: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();

  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState({ ...blank });
  const [files, setFiles] = useState<File[]>([]);
  const [busy, setBusy] = useState(false);

  const { data: policies, isLoading } = useQuery({
    queryKey: ["activeInsurancePolicies"],
    queryFn: () => getAllInsurancePolicies({ ...parameterInitialData, take: 200, status: "Active" }),
    staleTime: 60_000,
  });
  const active = (policies?.data ?? []) as InsurancePolicyModel[];
  const policyOptions = active.map((p) => ({ id: p.id!, name: `${p.policyNumber} — ${t(p.insuranceType ?? "")}` }));

  const changeHandler = useCallback((e: any) => setFormData((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setFormData((p) => ({ ...p, [name]: r.id })), []);
  const fileHandler = useCallback((e: any) => setFiles(Array.from(e.target.files ?? [])), []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    if (!formData.insurancePolicyId || !formData.claimType.trim() || formData.claimedAmount === "" || !formData.description.trim()) {
      setFormState({ status: "error", message: t("Please fill the policy, claim type, amount and description."), zodErrors: {} });
      return;
    }
    setBusy(true);
    const attachments = await Promise.all(files.map(async (f) => ({ fileName: f.name, contentType: f.type, contentBase64: await fileToBase64(f) })));
    const res = await submitInsuranceClaim({
      insurancePolicyId: formData.insurancePolicyId,
      claimType: formData.claimType.trim(),
      incidentDate: formData.incidentDate,
      claimedAmount: Number(formData.claimedAmount),
      description: formData.description.trim(),
      attachments,
    });
    setBusy(false);
    setFormState({ status: res.ok ? "success" : "error", message: res.message, zodErrors: {} });
    if (res.ok) {
      setFormData({ ...blank });
      setFiles([]);
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["myInsuranceClaims"] });
      onDone();
    }
  };

  if (isLoading) return <Loading />;
  if (active.length === 0)
    return <p className="m-4 rounded-lg border border-dashed border-border p-6 text-center text-sm text-muted">{t("There are no active insurance policies to claim against. Contact HR.")}</p>;

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
            { name: "insurancePolicyId", label: "Policy", required: true, type: "dropDown", onSelect: selectHandler, value: formData.insurancePolicyId, displayValue: policyOptions.find((o) => o.id === formData.insurancePolicyId)?.name, data: policyOptions as never },
            { name: "claimType", label: "Claim Type", placeholder: "e.g. Hospitalization", required: true, value: formData.claimType, onChange: changeHandler, type: "text" },
            { name: "incidentDate", label: "Incident Date", required: true, type: "date", value: formData.incidentDate, onChange: changeHandler },
            { name: "claimedAmount", label: "Claimed Amount", required: true, value: formData.claimedAmount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "description", label: "Description", required: true, value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "attachment", label: "Attachment (certificate / document, max 5 MB)", type: "file", onChange: fileHandler, colSpan: "full" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default MyInsuranceClaimForm;
