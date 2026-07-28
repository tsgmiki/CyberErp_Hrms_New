"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { BadgeCheck } from "lucide-react";
import type { EmployeeGuaranteeModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import EmployeePicker from "@/components/common/employeePicker";
import ButtonField from "@/components/ui/buttonField";
import { getGuarantee, saveGuarantee, releaseGuarantee } from "@/services/admin/employeeGuarantee";
import { useLookupOptions } from "@/services/admin/lookup";
import { guaranteeTypeOptions } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);

/**
 * §3.12 HC305/HC306 — guided, validated guarantee-commitment form. Used by the HR register
 * (employee picked freely), the self-service screen (`mine` — employee pinned by the backend to
 * the signed-in user), and the employee-profile Guarantees tab (`fixedEmployeeId` — employee
 * locked to the open profile). Approval states are workflow-owned (HC307).
 */
function GuaranteeForm({
  id,
  setId,
  mine = false,
  fixedEmployeeId,
  fixedEmployeeName,
}: {
  id: string;
  setId: (id: string) => void;
  mine?: boolean;
  fixedEmployeeId?: string;
  fixedEmployeeName?: string;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [releaseNote, setReleaseNote] = useState("");
  const [formData, setFormData] = useState<EmployeeGuaranteeModel>({ employeeId: fixedEmployeeId });

  // Guarantee types come from the GLOBAL "GuaranteeType" lookup category (id = value NAME, the
  // stored form) — falling back to the built-in defaults until the category is configured.
  const { options: lookupTypes } = useLookupOptions("GuaranteeType");
  const typeOptions = lookupTypes.length > 0
    ? lookupTypes
    : guaranteeTypeOptions.map((o) => ({ id: o.name, name: o.name }));

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["employeeGuarantee", id],
    queryFn: () => getGuarantee(id),
    enabled: typeof id != "undefined" && id != "",
  });
  useEffect(() => { if (record) setFormData(record); }, [record]);

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["employeeGuarantees"] });
    queryClient.invalidateQueries({ queryKey: ["myGuarantees"] });
    queryClient.invalidateQueries({ queryKey: ["guaranteeDashboard"] });
  };

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const res = await saveGuarantee({
      ...formData,
      amount: formData.amount !== undefined ? Number(formData.amount) : undefined,
      startDate: formData.startDate,
      endDate: formData.endDate || undefined,
    });
    setFormState(res.ok
      ? { status: "success", message: res.message, id: res.id }
      : { status: "error", message: res.message, zodErrors: {} });
    setIsLoading(false);
    if (res.ok) {
      invalidate();
      setId("");
    }
  };

  const releaseHandler = async () => {
    if (!formData.id) return;
    setIsLoading(true);
    const res = await releaseGuarantee(formData.id, releaseNote.trim() || undefined);
    setFormState(res.ok
      ? { status: "success", message: t("Commitment released") }
      : { status: "error", message: res.message, zodErrors: {} });
    setIsLoading(false);
    if (res.ok) {
      invalidate();
      setId("");
    }
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  return (
    <div className="text-foreground">
      {pending && <Loading />}

      {/* HR picks the employee; self-service pins the signed-in employee server-side; the profile
          tab implies the employee (picker hidden). */}
      {!mine && !fixedEmployeeId && (
        <div className="mb-4 rounded-lg border border-border bg-card p-4">
          <label className="mb-1 block text-xs font-medium text-muted">{t("Employee")} *</label>
          <EmployeePicker
            value={formData.employeeId}
            displayValue={formData.employeeName ?? fixedEmployeeName}
            onSelect={(empId, name) => setFormData((p) => ({ ...p, employeeId: empId, employeeName: name }))}
            disabled={!!id || !!fixedEmployeeId}
            placeholder={t("Search employee…") ?? undefined}
          />
        </div>
      )}

      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          formId: "employeeGuaranteeForm",
          components: [
            {
              name: "type", label: "Guarantee Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.type, displayValue: formData.type,
              error: formState?.zodErrors?.type, data: typeOptions as never,
            },
            {
              name: "externalOrganization", label: "External Organization", required: true, type: "text",
              placeholder: "e.g. Awash Bank", value: formData.externalOrganization, onChange: changeHandler,
              error: formState?.zodErrors?.externalOrganization,
            },
            {
              name: "beneficiaryName", label: "Beneficiary (guaranteed person)", required: true, type: "text",
              value: formData.beneficiaryName, onChange: changeHandler, error: formState?.zodErrors?.beneficiaryName,
            },
            {
              name: "beneficiaryRelationship", label: "Relationship", type: "text",
              placeholder: "e.g. Sibling", value: formData.beneficiaryRelationship, onChange: changeHandler,
            },
            {
              name: "referenceNumber", label: "Reference No.", type: "text",
              placeholder: "Guarantee letter / contract ref", value: formData.referenceNumber, onChange: changeHandler,
            },
            {
              name: "amount", label: "Committed Amount", required: true, type: "text", inputType: "number",
              value: formData.amount as never, onChange: changeHandler, error: formState?.zodErrors?.amount,
            },
            {
              name: "startDate", label: "Start Date", required: true, type: "date",
              value: (formData.startDate ?? "").slice(0, 10), onChange: changeHandler,
              error: formState?.zodErrors?.startDate,
            },
            {
              name: "endDate", label: "End Date", type: "date",
              value: (formData.endDate ?? "").slice(0, 10), onChange: changeHandler,
              error: formState?.zodErrors?.endDate,
            },
            {
              name: "remarks", label: "Remarks", type: "textarea", colSpan: "full",
              value: formData.remarks, onChange: changeHandler,
            },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />

      {formData.status === "PendingApproval" && (
        <p className="mt-3 rounded-md bg-info/10 px-3 py-2 text-xs text-info">
          {t("This commitment is awaiting workflow approval — it takes effect once the chain approves it (see My Approvals).")}
        </p>
      )}
      {formData.status === "Rejected" && (
        <p className="mt-3 rounded-md bg-error/10 px-3 py-2 text-xs text-error">
          {t("This commitment was rejected by the approval workflow. Saving it resubmits it for approval.")}
        </p>
      )}
      {formData.status === "Released" && (
        <p className="mt-3 rounded-md bg-secondary/30 px-3 py-2 text-xs text-foreground">
          {t("Released on")} {(formData.releasedDate ?? "").slice(0, 10)}
          {formData.releaseNote ? ` — ${formData.releaseNote}` : ""}
        </p>
      )}

      {/* HR-only discharge of an active commitment (HC305). */}
      {!mine && !!id && formData.status === "Active" && (
        <div className="mt-4 rounded-lg border border-border bg-card p-4">
          <h4 className="mb-1 text-sm font-semibold">{t("Release Commitment")}</h4>
          <p className="mb-3 text-xs text-muted">
            {t("Discharge this guarantee once the external obligation has ended. Released commitments stay on record.")}
          </p>
          <div className="flex flex-wrap items-center gap-2">
            <input
              className="h-10 flex-1 rounded-lg border border-border bg-background px-3 text-sm text-foreground"
              value={releaseNote}
              onChange={(e) => setReleaseNote(e.target.value)}
              placeholder={t("Release note (optional)") ?? ""}
            />
            <ButtonField
              value={t("Release")}
              icon={<BadgeCheck className="h-4 w-4" />}
              htmlType="button"
              variant="outline"
              disabled={isLoading}
              onClick={releaseHandler}
            />
          </div>
        </div>
      )}

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(GuaranteeForm);
