"use client";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save } from "lucide-react";
import type { LeaveRequestModel, LeaveRequestLineModel } from "@/models";
import saveLeaveRequest from "@/services/admin/leaveRequest/save";
import getLeaveRequest from "@/services/admin/leaveRequest/get";
import getLeaveBalances from "@/services/admin/leaveBalance/getByEmployee";
import getAllEmployee from "@/services/admin/employee/getAll";
import getAllLeaveType from "@/services/admin/leaveType/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { dayPartOptions, optionLabel, leaveStatusTone } from "@/constants/leave";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT =
  "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const lookupParam = { ...parameterInitialData, take: 200 };
const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

interface EditableLine extends LeaveRequestLineModel {
  _key: number;
}

function LeaveRequestForm({
  id,
  setId,
  lockedEmployeeId,
  lockedEmployeeName,
}: {
  id: string;
  setId: (id: string) => void;
  /** When set (e.g. opened from the Employee tab), the employee is fixed and its picker is hidden. */
  lockedEmployeeId?: string;
  lockedEmployeeName?: string;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;
  const viewing = typeof id !== "undefined" && id !== "";

  const [meta, setMeta] = useState<LeaveRequestModel>({});
  const [lines, setLines] = useState<EditableLine[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["leaveRequest", id],
    queryFn: () => getLeaveRequest(id),
    enabled: viewing,
  });

  const { data: employees } = useQuery({
    queryKey: ["employees", lookupParam],
    queryFn: () => getAllEmployee(lookupParam),
  });
  const { data: types } = useQuery({
    queryKey: ["leaveTypes", lookupParam],
    queryFn: () => getAllLeaveType(lookupParam),
  });
  const { data: balances } = useQuery({
    queryKey: ["leaveBalances", meta.employeeId],
    queryFn: () => getLeaveBalances(meta.employeeId!),
    enabled: !viewing && !!meta.employeeId,
  });

  const typeList = useMemo(() => types?.data ?? [], [types]);

  // Start a new request with a single blank line (employee prefilled when locked to a tab).
  useEffect(() => {
    if (!viewing) {
      setMeta(lockedEmployeeId ? { employeeId: lockedEmployeeId } : {});
      setLines([{ _key: nextKey(), dayPart: "Full", startDate: "", endDate: "", leaveTypeId: "" }]);
    }
  }, [viewing, lockedEmployeeId]);

  const setMetaField = (name: keyof LeaveRequestModel, value: unknown) =>
    setMeta((p) => ({ ...p, [name]: value }));

  const addLine = () =>
    setLines((p) => [
      ...p,
      { _key: nextKey(), dayPart: "Full", startDate: "", endDate: "", leaveTypeId: "" },
    ]);
  // Half-day lines are single-date, so keep endDate pinned to startDate for them.
  const updateLine = (key: number, patch: Partial<EditableLine>) =>
    setLines((p) =>
      p.map((l) => {
        if (l._key !== key) return l;
        const next = { ...l, ...patch };
        if (next.dayPart !== "Full") next.endDate = next.startDate;
        return next;
      }),
    );
  const removeLine = (key: number) => setLines((p) => p.filter((l) => l._key !== key));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const payload: LeaveRequestModel = {
      ...meta,
      lines: lines.map(({ _key, ...l }) => ({
        leaveTypeId: l.leaveTypeId,
        startDate: l.startDate,
        endDate: l.endDate,
        dayPart: l.dayPart || "Full",
      })),
    };
    const result = await saveLeaveRequest(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["leaveRequests"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  // ---- Read-only detail view for an existing request --------------------
  if (viewing) {
    if (!record) return null;
    const info = (label: string, value: React.ReactNode) => (
      <div>
        <p className={LABEL}>{t(label)}</p>
        <p className="text-sm font-medium text-foreground">{value}</p>
      </div>
    );
    return (
      <div className="space-y-5 text-foreground">
        <section className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-3 text-sm font-semibold">{t("Leave Request")}</h3>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            {info("Employee", `${record.employeeName ?? ""} (${record.employeeNumber ?? ""})`)}
            {info("Submitted", fmt(record.submittedDate))}
            {info(
              "Status",
              <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${leaveStatusTone[record.status || ""] || ""}`}>
                {record.status}
              </span>,
            )}
            {info("Total Working Days", record.totalWorkingDays)}
            <div className="sm:col-span-2">{info("Reason", record.reason || "—")}</div>
            {record.decisionComment && info("Decision", record.decisionComment)}
            {record.cancelReason && info("Cancel Reason", record.cancelReason)}
          </div>
        </section>

        <section className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-3 text-sm font-semibold">{t("Leave Lines")}</h3>
          <div className="overflow-x-auto">
            <table className="w-full text-[13px]">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                  <th className="px-4 py-2 font-semibold">{t("Leave Type")}</th>
                  <th className="px-4 py-2 font-semibold">{t("From")}</th>
                  <th className="px-4 py-2 font-semibold">{t("To")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Day Part")}</th>
                  <th className="px-4 py-2 text-right font-semibold">{t("Days")}</th>
                </tr>
              </thead>
              <tbody>
                {(record.lines ?? []).map((l, i) => (
                  <tr key={l.id ?? i} className="border-b border-border/60 hover:bg-secondary/40">
                    <td className="px-4 py-2.5 text-foreground">{l.leaveTypeName || l.leaveTypeCode || "—"}</td>
                    <td className="px-4 py-2.5 text-foreground">{fmt(l.startDate)}</td>
                    <td className="px-4 py-2.5 text-foreground">{fmt(l.endDate)}</td>
                    <td className="px-4 py-2.5 text-foreground">{optionLabel(dayPartOptions, l.dayPart) || l.dayPart}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums text-foreground">{l.workingDays}</td>
                  </tr>
                ))}
                <tr className="bg-secondary/30 font-semibold">
                  <td className="px-4 py-2.5" colSpan={4}>{t("Total")}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{record.totalWorkingDays}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>
    );
  }

  // ---- New request: header + repeatable detail lines --------------------
  return (
    <form onSubmit={submit} className="space-y-5 text-foreground">
      {/* Request header */}
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Leave Request")}</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            {lockedEmployeeId ? (
              <input className={`${INPUT} opacity-70`} value={lockedEmployeeName ?? ""} disabled readOnly />
            ) : (
              <select
                className={INPUT}
                value={meta.employeeId ?? ""}
                onChange={(e) => setMetaField("employeeId", e.target.value)}
                required
              >
                <option value="">{t("Select employee")}</option>
                {(employees?.data ?? []).map((e) => (
                  <option key={e.id} value={e.id}>
                    {e.employeeNumber} — {e.fullName ?? ""}
                  </option>
                ))}
              </select>
            )}
            {formState?.zodErrors?.employeeId && (
              <p className="mt-1 text-xs text-error">{formState.zodErrors.employeeId[0]}</p>
            )}
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Reason")}</label>
            <input
              className={INPUT}
              value={meta.reason ?? ""}
              onChange={(e) => setMetaField("reason", e.target.value)}
            />
          </div>
        </div>

        {/* Available balances for the selected employee */}
        {!!meta.employeeId && (balances?.length ?? 0) > 0 && (
          <div className="mt-3 flex flex-wrap gap-2">
            {(balances ?? []).map((b) => (
              <span
                key={b.leaveTypeId}
                className="rounded-md border border-border/70 bg-secondary/20 px-2.5 py-1 text-xs text-foreground"
              >
                {b.leaveTypeName || b.leaveTypeCode}: <b>{b.available}</b> {t("available")}
              </span>
            ))}
          </div>
        )}
      </section>

      {/* Detail lines */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("Leave Lines")}</h3>
          <button
            type="button"
            onClick={addLine}
            className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus className="h-3.5 w-3.5" /> {t("Add Line")}
          </button>
        </div>

        {formState?.zodErrors?.lines && (
          <p className="mb-2 text-xs text-error">{formState.zodErrors.lines[0]}</p>
        )}

        {lines.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No lines yet. Add at least one line.")}</p>
        ) : (
          <div className="space-y-2">
            {lines.map((l) => {
              const half = l.dayPart !== "Full";
              return (
                <div
                  key={l._key}
                  className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_160px_160px_140px_auto]"
                >
                  <div>
                    <label className={LABEL}>{t("Leave Type")} *</label>
                    <select
                      className={INPUT}
                      value={l.leaveTypeId ?? ""}
                      onChange={(e) => updateLine(l._key, { leaveTypeId: e.target.value })}
                      required
                    >
                      <option value="">{t("Select type")}</option>
                      {typeList.map((ty) => (
                        <option key={ty.id} value={ty.id}>
                          {ty.code} — {ty.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className={LABEL}>{t("Start Date")} *</label>
                    <input
                      type="date"
                      className={INPUT}
                      value={fmt(l.startDate)}
                      onChange={(e) => updateLine(l._key, { startDate: e.target.value })}
                      required
                    />
                  </div>
                  <div>
                    <label className={LABEL}>{t("End Date")} *</label>
                    <input
                      type="date"
                      className={INPUT}
                      value={fmt(l.endDate)}
                      min={fmt(l.startDate)}
                      disabled={half}
                      title={half ? (t("A half day is a single date") ?? "") : undefined}
                      onChange={(e) => updateLine(l._key, { endDate: e.target.value })}
                      required
                    />
                  </div>
                  <div>
                    <label className={LABEL}>{t("Day Part")}</label>
                    <select
                      className={INPUT}
                      value={l.dayPart ?? "Full"}
                      onChange={(e) => updateLine(l._key, { dayPart: e.target.value })}
                    >
                      {dayPartOptions.map((o) => (
                        <option key={o.id} value={o.id}>{t(o.name)}</option>
                      ))}
                    </select>
                  </div>
                  <div className="flex items-center pb-1">
                    {lines.length > 1 && (
                      <button
                        type="button"
                        onClick={() => removeLine(l._key)}
                        className="rounded p-1 text-error hover:bg-error/10"
                        title={t("Remove") ?? ""}
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
        <p className="mt-3 text-xs text-muted">
          {t(
            "Total working days are calculated on submission using the active work-week configuration (rest days excluded, half days counted as 0.5).",
          )}
        </p>
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isSaving}
          className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
        >
          <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Submit Request")}
        </button>
      </div>
    </form>
  );
}

export default memo(LeaveRequestForm);
