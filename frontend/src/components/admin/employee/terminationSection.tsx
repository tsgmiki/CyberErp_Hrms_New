"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  UserX,
  Plus,
  Ban,
  Hourglass,
  BadgeCheck,
} from "lucide-react";
import type { EmployeeTerminationModel, TerminationClearanceModel } from "@/models";
import {
  getTerminations,
  saveTermination,
  finalizeTermination,
  cancelTermination,
} from "@/services/admin/employee/termination";
import Loading from "../../common/loader/loader";
import Modal from "@/components/common/modal";
import { useCustomFields } from "./customFieldsHook";
import { StatusMessage } from "../../common/statusMessage/status";
import { terminationTypeOptions } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "—");

const CASE_TONE: Record<string, string> = {
  Initiated: "bg-info/15 text-info",
  ClearanceInProgress: "bg-warning/15 text-warning",
  Settled: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};
const CLEAR_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Cleared: "bg-success/15 text-success",
  Blocked: "bg-error/15 text-error",
};

/**
 * One departmental clearance row — read-only status view. Clearance decisions are made by the
 * assigned approvers from their Dashboard "Clearance" tab, not here; this shows progress only.
 */
function ClearanceRow({ item }: { item: TerminationClearanceModel }) {
  const { t } = useTranslation();

  return (
    <tr className="border-b border-border/60">
      <td className="px-4 py-2.5 font-semibold text-foreground">
        {item.department}
        {(item.approverNames?.length ?? 0) > 0 && (
          <span
            className="mt-0.5 block text-[11px] font-normal text-muted"
            title={`${t("Authorized")}: ${item.approverNames!.join(", ")}`}
          >
            {t("Approvers")}: {item.approverNames!.join(", ")}
          </span>
        )}
      </td>
      <td className="px-4 py-2.5 text-xs text-muted">{item.description}</td>
      <td className="px-4 py-2.5">
        <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CLEAR_TONE[item.status] ?? ""}`}>
          {t(item.status)}
        </span>
        {item.clearedBy && (
          <span className="mt-0.5 block text-[11px] text-muted">
            {item.clearedBy} · {fmtDate(item.clearedAt)}
          </span>
        )}
      </td>
      <td className="px-4 py-2.5 text-xs text-muted">{item.note || "—"}</td>
    </tr>
  );
}

function TerminationSection({ employeeId }: { employeeId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [confirmFinalize, setConfirmFinalize] = useState(false);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<EmployeeTerminationModel>({});
  const [isSaving, setIsSaving] = useState(false);
  const customFields = useCustomFields("Termination");
  const [busy, setBusy] = useState(false);

  const queryKey = ["employeeTerminations", employeeId];
  const { data: rows, isLoading } = useQuery({
    queryKey,
    queryFn: () => getTerminations(employeeId),
  });

  const active = (rows ?? []).find(
    (x) => x.status === "Initiated" || x.status === "ClearanceInProgress",
  );
  const history = (rows ?? []).filter((x) => x.id !== active?.id);
  const allCleared =
    !!active?.clearances?.length && active.clearances.every((c) => c.status === "Cleared");

  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey });
    // Settlement changes the employee master, position vacancy and workflow feeds.
    queryClient.invalidateQueries({ queryKey: ["employees"] });
    queryClient.invalidateQueries({ queryKey: ["employee", employeeId] });
    queryClient.invalidateQueries({ queryKey: ["positions"] });
    queryClient.invalidateQueries({ queryKey: ["workflows"] });
    queryClient.invalidateQueries({ queryKey: ["workflowStats"] });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [queryClient, employeeId]);

  const run = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    setBusy(true);
    const res = await fn();
    setBusy(false);
    setError(res.ok ? null : res.message);
    refresh();
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsSaving(true);
    const result = await saveTermination(fd);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      refresh();
      setShowForm(false);
    }
  };

  if (isLoading) return <Loading />;

  return (
    <div className="m-1 space-y-3">
      {error && (
        <div className="flex items-center justify-between rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="font-semibold">×</button>
        </div>
      )}

      {/* No active case → initiation entry point */}
      {!active && (
        <div className="rounded-lg border border-border bg-card px-4 py-8 text-center">
          <UserX className="mx-auto h-8 w-8 text-muted" />
          <p className="mt-2 text-sm text-muted">{t("No active termination case.")}</p>
          <button
            type="button"
            onClick={() => {
              setFormData({ terminationType: "Voluntary" });
              customFields.hydrate();
              setFormState({});
              setShowForm(true);
            }}
            className="mt-3 inline-flex items-center gap-1 rounded bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus className="h-3.5 w-3.5" /> {t("Initiate Termination")}
          </button>
        </div>
      )}

      {/* Active case card */}
      {active && (
        <div className="rounded-lg border border-border bg-card">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border px-4 py-2.5">
            <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground">
              <UserX size={16} className="text-primary" />
              {t("Termination Case")}
              <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CASE_TONE[active.status ?? ""] ?? ""}`}>
                {t(active.status ?? "")}
              </span>
              <span className="rounded bg-secondary px-2 py-0.5 text-xs text-foreground">
                {t(active.terminationType ?? "")}
              </span>
            </h3>
            <div className="flex items-center gap-2">
              {active.status === "ClearanceInProgress" && (
                <button
                  type="button"
                  disabled={!allCleared || busy}
                  title={allCleared ? undefined : t("All clearances must be 'Cleared' first")}
                  onClick={() => setConfirmFinalize(true)}
                  className="inline-flex items-center gap-1.5 rounded-md bg-success px-3 py-1.5 text-xs font-semibold text-on-accent disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <BadgeCheck size={14} /> {t("Finalize Settlement")}
                </button>
              )}
              {!active.awaitingWorkflow && (
                <button
                  type="button"
                  disabled={busy}
                  onClick={() => active.id && run(() => cancelTermination(active.id!))}
                  className="inline-flex items-center gap-1.5 rounded-md border border-border px-3 py-1.5 text-xs text-foreground hover:border-error hover:text-error disabled:opacity-50"
                >
                  <Ban size={14} /> {t("Cancel Case")}
                </button>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-x-6 gap-y-2 px-4 py-3 text-sm md:grid-cols-4">
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Notice Date")}</p>
              <p className="text-foreground">{fmtDate(active.noticeDate)}</p>
            </div>
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Last Working Date")}</p>
              <p className="text-foreground">{fmtDate(active.lastWorkingDate)}</p>
            </div>
            <div className="col-span-2">
              <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Reason")}</p>
              <p className="text-foreground">{active.reason}</p>
            </div>
            {active.remarks && (
              <div className="col-span-2 md:col-span-4">
                <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{t("Remarks")}</p>
                <p className="text-foreground">{active.remarks}</p>
              </div>
            )}
          </div>

          {active.awaitingWorkflow && (
            <div className="mx-4 mb-3 flex items-center gap-2 rounded-md border border-info/30 bg-info/10 px-3 py-2 text-xs text-info">
              <Hourglass size={14} />
              {t("Awaiting workflow approval — track and decide under Workflow Tracking.")}
            </div>
          )}

          {/* Departmental clearance checklist */}
          {active.status === "ClearanceInProgress" && (
            <div className="border-t border-border">
              <div className="flex items-center justify-between px-4 py-2">
                <h4 className="text-xs font-bold uppercase tracking-wide text-muted">
                  {t("Departmental Clearance")}
                </h4>
                <span className="text-xs text-muted">
                  {active.clearances?.filter((c) => c.status === "Cleared").length}/{active.clearances?.length} {t("cleared")}
                </span>
              </div>
              <div className="px-4 pb-1 text-[11px] text-muted">
                {t("Assigned approvers clear these from their Dashboard “Clearance” tab. Settlement unlocks once all assigned approvers are done.")}
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-[13px]">
                  <thead>
                    <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                      <th className="px-4 py-2 font-semibold">{t("Department")}</th>
                      <th className="px-4 py-2 font-semibold">{t("Requirement")}</th>
                      <th className="px-4 py-2 font-semibold">{t("Status")}</th>
                      <th className="px-4 py-2 font-semibold">{t("Note")}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(active.clearances ?? []).map((c) => (
                      <ClearanceRow key={c.id} item={c} />
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Case history */}
      {history.length > 0 && (
        <div className="rounded-lg border border-border bg-card">
          <h4 className="border-b border-border px-4 py-2.5 text-sm font-semibold text-foreground">
            {t("Termination History")}
          </h4>
          <div className="overflow-x-auto">
            <table className="w-full text-[13px]">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                  <th className="px-4 py-2 font-semibold">{t("Type")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Notice Date")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Last Working Date")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Reason")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Status")}</th>
                </tr>
              </thead>
              <tbody>
                {history.map((h) => (
                  <tr key={h.id} className="border-b border-border/60">
                    <td className="px-4 py-2.5 text-foreground">{t(h.terminationType ?? "")}</td>
                    <td className="px-4 py-2.5 text-foreground">{fmtDate(h.noticeDate)}</td>
                    <td className="px-4 py-2.5 text-foreground">{fmtDate(h.lastWorkingDate)}</td>
                    <td className="max-w-[240px] truncate px-4 py-2.5 text-muted" title={h.reason}>{h.reason}</td>
                    <td className="px-4 py-2.5">
                      <span className={`rounded px-2 py-0.5 text-xs font-semibold ${CASE_TONE[h.status ?? ""] ?? ""}`}>
                        {t(h.status ?? "")}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Initiation form */}
      {showForm && (
        <FormProvider
          form={{
            columnsNo: 2,
            submitHandler,
            fieldLayout: "auth",
            isPending: isSaving,
            SubmitButton: "top",
            showModal: true,
            modalVisible: true,
            modalTitle: "Initiate Termination",
            description: "End the employment and start the clearance process.",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Submit",
            components: [
              {
                name: "terminationType", label: "Termination Type", required: true, type: "dropDown",
                onSelect: selectHandler, value: formData.terminationType, displayValue: formData.terminationType,
                error: formState?.zodErrors?.terminationType, data: terminationTypeOptions as never,
              },
              { name: "noticeDate", label: "Notice Date", required: true, type: "date",
                value: formData.noticeDate, onChange: changeHandler, error: formState?.zodErrors?.noticeDate },
              { name: "lastWorkingDate", label: "Last Working Date", required: true, type: "date",
                value: formData.lastWorkingDate, onChange: changeHandler, error: formState?.zodErrors?.lastWorkingDate },
              { name: "reason", label: "Termination Reason", required: true, type: "textarea", colSpan: "full",
                value: formData.reason, onChange: changeHandler, error: formState?.zodErrors?.reason },
              { name: "remarks", label: "Remarks", type: "textarea", colSpan: "full",
                value: formData.remarks, onChange: changeHandler },
              ...customFields.components,
              { name: "employeeId", value: employeeId, type: "hidden" },
              { name: "id", value: formData.id, type: "hidden" },
            ],
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
        </FormProvider>
      )}

      {/* Finalize confirmation */}
      {confirmFinalize && active?.id && (
        <Modal
          visible
          size="md"
          title={t("Finalize Settlement")}
          description={t("This ends the employment: the employee becomes Terminated and their position is released as vacant. This cannot be undone.")}
          onClose={() => setConfirmFinalize(false)}
          footer={
            <>
              <button
                type="button"
                onClick={() => setConfirmFinalize(false)}
                className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary"
              >
                {t("Close")}
              </button>
              <button
                type="button"
                disabled={busy}
                onClick={async () => {
                  setConfirmFinalize(false);
                  await run(() => finalizeTermination(active.id!));
                }}
                className="rounded-md bg-success px-3 py-1.5 text-sm font-medium text-on-accent disabled:opacity-50"
              >
                {t("Finalize Settlement")}
              </button>
            </>
          }
        >
          <p className="text-sm text-foreground">
            {t("All departmental clearances are cleared. Proceed with the final settlement?")}
          </p>
        </Modal>
      )}
    </div>
  );
}

export default TerminationSection;
