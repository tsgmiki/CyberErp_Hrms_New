"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { HiringRequestModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Send, Hourglass, XCircle } from "lucide-react";
import {
  getHiringRequest,
  saveHiringRequest,
  submitHiringRequest,
  closeHiringRequest,
} from "@/services/admin/recruitment";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import { getAllWorkforcePlans } from "@/services/admin/workforcePlan";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { plannedEmploymentTypeOptions } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const lookupParam = { ...parameterInitialData, take: 200 };

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  Submitted: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
  Closed: "bg-info/15 text-info",
};

function HiringRequestForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<HiringRequestModel>({
    numberOfPositions: 1,
    employmentType: "Permanent",
    estimatedBudget: 0,
  });
  const [busy, setBusy] = useState(false);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["hiringRequest", id],
    queryFn: () => getHiringRequest(id),
    enabled: typeof id != "undefined" && id != "",
  });
  const { data: units } = useQuery({
    queryKey: ["organizationUnits", lookupParam],
    queryFn: () => getAllOrganizationUnit(lookupParam),
  });
  const { data: classes } = useQuery({
    queryKey: ["positionClasses", lookupParam],
    queryFn: () => getAllPositionClass(lookupParam),
  });
  const { data: plans } = useQuery({
    queryKey: ["workforcePlans", lookupParam],
    queryFn: () => getAllWorkforcePlans(lookupParam),
  });

  const readOnly = !!record && record.status !== "Draft" && record.status !== "Rejected";

  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["hiringRequests"] });
      if (!formData.id && formState.id) {
        setId(formState.id);
        queryClient.invalidateQueries({ queryKey: ["hiringRequest", formState.id] });
      } else if (formData.id) {
        queryClient.invalidateQueries({ queryKey: ["hiringRequest", formData.id] });
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

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const result = await saveHiringRequest(formData);
    setFormState(result);
    setIsLoading(false);
  };

  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ["hiringRequest", id] });
    queryClient.invalidateQueries({ queryKey: ["hiringRequests"] });
    queryClient.invalidateQueries({ queryKey: ["workflows"] });
    queryClient.invalidateQueries({ queryKey: ["workflowStats"] });
    queryClient.invalidateQueries({ queryKey: ["myApprovals"] });
  }, [queryClient, id]);

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    setActionMessage(res.message);
    refresh();
  };

  return (
    <div className="text-foreground">
      {pending && <Loading />}

      {record && (
        <div className="mb-2 flex flex-wrap items-center gap-2 text-sm">
          <span className="font-semibold">{record.requestNumber}</span>
          <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[record.status ?? ""] ?? ""}`}>
            {t(record.status ?? "")}
          </span>
          {record.awaitingWorkflow && (
            <span className="flex items-center gap-1 rounded border border-info/30 bg-info/10 px-2 py-0.5 text-xs text-info">
              <Hourglass size={12} /> {t("Awaiting workflow approval")}
            </span>
          )}
          <span className="rounded bg-secondary px-2 py-0.5 text-xs" title={t("Vacant establishment seats for this unit × role (HC082)")}>
            {t("Vacant seats")}: {record.vacantSeats ?? 0}
          </span>
          {record.status === "Approved" && (
            <span className="rounded bg-secondary px-2 py-0.5 text-xs">
              {t("Requisitioned")}: {record.requisitionedPositions ?? 0}/{record.numberOfPositions}
            </span>
          )}
        </div>
      )}

      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: (readOnly ? "none" : "top") as "top",
          submitBtnTitle: "Save Request",
          components: [
            {
              name: "organizationUnitId", label: "Requesting Unit", required: true, type: "dropDown",
              onSelect: selectHandler, value: formData.organizationUnitId,
              displayValue: formData.organizationUnitName, disabled: readOnly,
              error: formState?.zodErrors?.organizationUnitId,
              data: (units?.data ?? []).map((u) => ({ id: u.id, name: u.name })) as never,
            },
            {
              name: "positionClassId", label: "Role (Position Class)", required: true, type: "dropDown",
              onSelect: selectHandler, value: formData.positionClassId,
              displayValue: formData.positionClassTitle ?? (formData as any).positionClassName,
              disabled: readOnly, error: formState?.zodErrors?.positionClassId,
              data: (classes?.data ?? []).map((c) => ({ id: c.id, name: c.title })) as never,
            },
            {
              name: "numberOfPositions", label: "Positions Requested", required: true, type: "text",
              value: formData.numberOfPositions, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "employmentType", label: "Employment Type", type: "dropDown", onSelect: selectHandler,
              value: formData.employmentType, displayValue: formData.employmentType, disabled: readOnly,
              data: plannedEmploymentTypeOptions as never,
            },
            {
              name: "estimatedBudget", label: "Estimated Budget", type: "text",
              value: formData.estimatedBudget, onChange: changeHandler, disabled: readOnly, placeholder: "0",
            },
            {
              name: "expectedStartDate", label: "Expected Start", type: "date",
              value: formData.expectedStartDate, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "workforcePlanId", label: "Workforce Plan (optional)", type: "dropDown",
              onSelect: selectHandler, value: formData.workforcePlanId,
              displayValue: formData.workforcePlanName, disabled: readOnly,
              placeholder: "Link the plan this need fulfils (HC081)",
              data: (plans?.data ?? []).map((p) => ({ id: p.id, name: `${p.name} v${p.version}` })) as never,
            },
            {
              name: "timelineRemarks", label: "Timeline Remarks", type: "text",
              value: formData.timelineRemarks, onChange: changeHandler, disabled: readOnly,
            },
            {
              name: "justification", label: "Justification", required: true, type: "textarea", colSpan: "full",
              value: formData.justification, onChange: changeHandler, disabled: readOnly,
              error: formState?.zodErrors?.justification,
            },
            {
              name: "jobRequirements", label: "Job Requirements", type: "textarea", colSpan: "full",
              value: formData.jobRequirements, onChange: changeHandler, disabled: readOnly,
            },
          ],
        }}
      />

      {/* Actions */}
      <div className="mt-3 flex flex-wrap items-center gap-2">
        {id && !readOnly && (
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => submitHiringRequest(id))}
            title={t("Validated against vacant establishment seats before routing for approval (HC082)")}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
          >
            <Send size={14} /> {t("Submit for Approval")}
          </button>
        )}
        {record?.status === "Approved" && (
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => closeHiringRequest(id))}
            className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-error hover:text-error disabled:opacity-50"
          >
            <XCircle size={14} /> {t("Close Request")}
          </button>
        )}
        {actionMessage && <span className="text-xs text-muted">{actionMessage}</span>}
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default HiringRequestForm;
