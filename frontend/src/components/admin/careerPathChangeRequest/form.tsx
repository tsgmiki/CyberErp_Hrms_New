"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useMemo, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Send, Check, X } from "lucide-react";
import type { CareerPathChangeRequestModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveChangeRequest from "@/services/admin/careerPathChangeRequest/save";
import getChangeRequest from "@/services/admin/careerPathChangeRequest/get";
import { submitChangeRequest, approveChangeRequest, rejectChangeRequest } from "@/services/admin/careerPathChangeRequest/actions";
import getAllCareerPath from "@/services/admin/careerPath/getAll";
import EmployeePicker from "@/components/common/employeePicker";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);
const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted", Submitted: "bg-info/15 text-info", Approved: "bg-success/15 text-success", Rejected: "bg-error/15 text-error",
};

function ChangeRequestForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as CareerPathChangeRequestModel);
  const [decisionNotes, setDecisionNotes] = useState("");
  const [acting, setActing] = useState(false);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["careerPathChangeRequest", id],
    queryFn: () => getChangeRequest(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: paths } = useQuery({ queryKey: ["careerPaths", "crPicker"], queryFn: () => getAllCareerPath({ ...parameterInitialData, take: 300 }), staleTime: 60_000 });
  const pathOptions = useMemo(() => (paths?.data ?? []).map((p) => ({ id: p.id!, name: `${p.name} (${p.code})` })), [paths]);
  const pName = (v?: string) => pathOptions.find((o) => o.id === v)?.name ?? "";

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    setFormState(await saveChangeRequest(fd));
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => setFormData((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setFormData((p) => ({ ...p, [name]: r.id })), []);

  useEffect(() => { if (record) setFormData(record); }, [record]);
  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["careerPathChangeRequests"] });
      if (formState.id && !id) setId(formState.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  const runAction = async (fn: () => Promise<unknown>) => {
    setActing(true);
    try {
      await fn();
      queryClient.invalidateQueries({ queryKey: ["careerPathChangeRequest", id] });
      queryClient.invalidateQueries({ queryKey: ["careerPathChangeRequests"] });
      queryClient.invalidateQueries({ queryKey: ["employeeCareerPaths"] });
      setDecisionNotes("");
    } finally {
      setActing(false);
    }
  };

  const status = formData.status ?? "Draft";

  return (
    <div className="space-y-4">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            // Server-search picker (no bulk employee load); the hidden field carries the id into FormData.
            { name: "employeePicker", label: "Employee", required: true, type: "custom", error: formState?.zodErrors?.employeeId,
              customChildren: (
                <EmployeePicker
                  value={formData.employeeId}
                  displayValue={formData.employeeName}
                  onSelect={(eid, name) => setFormData((p) => ({ ...p, employeeId: eid, employeeName: name }))}
                />
              ) },
            { name: "employeeId", value: formData.employeeId, type: "hidden" },
            { name: "currentCareerPathId", label: "Current Career Path", type: "dropDown", onSelect: selectHandler,
              value: formData.currentCareerPathId, displayValue: pName(formData.currentCareerPathId), data: pathOptions as never },
            { name: "requestedCareerPathId", label: "Requested Career Path", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.requestedCareerPathId, displayValue: pName(formData.requestedCareerPathId), error: formState?.zodErrors?.requestedCareerPathId, data: pathOptions as never },
            { name: "reason", label: "Reason", value: formData.reason, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Approval flow (HC169) */}
      {id && (
        <section className="rounded-xl border border-border bg-card p-4 shadow-sm">
          <div className="mb-3 flex items-center gap-2">
            <h3 className="mr-auto text-sm font-semibold text-foreground">Approval</h3>
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[status]}`}>{status}</span>
          </div>

          {status === "Draft" && (
            <button type="button" disabled={acting} onClick={() => runAction(() => submitChangeRequest(id))}
              className="inline-flex items-center gap-1 rounded bg-primary px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Send size={13} /> Submit for review
            </button>
          )}

          {status === "Submitted" && (
            <div className="space-y-2">
              <input className="w-full rounded border border-border bg-card px-2 py-1.5 text-sm" placeholder="Decision notes (optional)" value={decisionNotes} onChange={(e) => setDecisionNotes(e.target.value)} />
              <div className="flex gap-2">
                <button type="button" disabled={acting} onClick={() => runAction(() => approveChangeRequest(id, decisionNotes || undefined))}
                  className="inline-flex items-center gap-1 rounded bg-success px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                  <Check size={13} /> Approve
                </button>
                <button type="button" disabled={acting} onClick={() => runAction(() => rejectChangeRequest(id, decisionNotes || undefined))}
                  className="inline-flex items-center gap-1 rounded bg-error px-3 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                  <X size={13} /> Reject
                </button>
              </div>
              <p className="text-xs text-muted">Approving assigns the employee to the requested career path.</p>
            </div>
          )}

          {(status === "Approved" || status === "Rejected") && (
            <p className="text-xs text-muted">
              Decided{formData.decidedAt ? ` on ${formData.decidedAt.slice(0, 10)}` : ""}
              {formData.decisionNotes ? ` — ${formData.decisionNotes}` : ""}.
            </p>
          )}
        </section>
      )}
    </div>
  );
}

export default memo(ChangeRequestForm);
