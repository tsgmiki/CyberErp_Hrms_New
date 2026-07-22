"use client";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save } from "lucide-react";
import type { AnnualLeaveModel, AnnualLeaveDetailModel } from "@/models";
import saveAnnualLeave from "@/services/admin/annualLeave/save";
import getAnnualLeave from "@/services/admin/annualLeave/get";
import getLeaveBalances from "@/services/admin/leaveBalance/getByEmployee";
import getAllEmployee from "@/services/admin/employee/getAll";
import getAllWorkWeekConfiguration from "@/services/admin/workWeekConfiguration/getAll";
import getAllHoliday from "@/services/admin/holiday/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { annualLeaveUsageOptions, halfDayPartOptions, optionLabel, leaveStatusTone } from "@/constants/leave";
import {
  buildWorkValues,
  buildHolidaySets,
  dayValue,
  countLeaveDays,
  addLeaveDays,
  parseDate,
  formatDate,
} from "@/utils/leaveCalendar";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT =
  "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

const lookupParam = { ...parameterInitialData, take: 200 };
const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

interface EditableDetail extends AnnualLeaveDetailModel {
  _key: number;
}

function AnnualLeaveForm({
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

  const [meta, setMeta] = useState<AnnualLeaveModel>({});
  const [details, setDetails] = useState<EditableDetail[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["annualLeave", id],
    queryFn: () => getAnnualLeave(id),
    enabled: viewing,
  });

  // Only needed for the open picker (HR profile-tab style callers pass a locked employee instead).
  const { data: employees } = useQuery({
    queryKey: ["employees", lookupParam],
    queryFn: () => getAllEmployee(lookupParam),
    enabled: !viewing && !lockedEmployeeId,
  });
  // The employee's ledgers (LeaveBalance rows) — the user picks the annual-leave one to charge.
  const { data: balances } = useQuery({
    queryKey: ["leaveBalances", meta.employeeId],
    queryFn: () => getLeaveBalances(meta.employeeId!),
    enabled: !viewing && !!meta.employeeId,
  });

  // Calendar inputs for the interactive day-math (active work-week config + holidays). Cached — the
  // backend re-applies the same rules authoritatively on submit, so this only drives the live preview.
  const { data: workWeeks } = useQuery({
    queryKey: ["workWeekConfigurations", "calc"],
    queryFn: () => getAllWorkWeekConfiguration({ ...parameterInitialData, take: 50 }),
    staleTime: 5 * 60 * 1000,
  });
  const { data: holidaysResp } = useQuery({
    queryKey: ["holidays", "calc"],
    queryFn: () => getAllHoliday({ ...parameterInitialData, take: 500 }),
    staleTime: 5 * 60 * 1000,
  });

  // Closures over the resolved calendar: value of a day, days in a range, and end-date from a count.
  const calc = useMemo(() => {
    const active = (workWeeks?.data ?? []).find((w) => w.isActive) ?? null;
    const workValues = buildWorkValues(active);
    const holidays = buildHolidaySets(holidaysResp?.data ?? []);
    return {
      dayVal: (iso: string) => {
        const d = parseDate(iso);
        return d ? dayValue(d, workValues, holidays) : 0;
      },
      count: (s: string, e: string, half: boolean) => {
        const sd = parseDate(s);
        const ed = parseDate(e);
        return sd && ed ? countLeaveDays(sd, ed, half, workValues, holidays) : 0;
      },
      endFromDays: (s: string, days: number) => {
        const sd = parseDate(s);
        const r = sd ? addLeaveDays(sd, days, workValues, holidays) : null;
        return r ? formatDate(r) : "";
      },
    };
  }, [workWeeks, holidaysResp]);

  useEffect(() => {
    if (!viewing) {
      setMeta(lockedEmployeeId ? { employeeId: lockedEmployeeId } : {});
      setDetails([{ _key: nextKey(), leaveUsage: "FullDay", startDate: "", endDate: "" }]);
    }
  }, [viewing, lockedEmployeeId]);

  const setMetaField = (name: keyof AnnualLeaveModel, value: unknown) =>
    setMeta((p) => ({ ...p, [name]: value }));

  // Reset the ledger selection whenever the employee changes.
  const onEmployee = (employeeId: string) =>
    setMeta((p) => ({ ...p, employeeId, annualLeaveLedgerId: "" }));

  const addDetail = () =>
    setDetails((p) => [...p, { _key: nextKey(), leaveUsage: "FullDay", startDate: "", endDate: "" }]);
  const removeDetail = (key: number) => setDetails((p) => p.filter((d) => d._key !== key));

  const setDetail = (key: number, updater: (d: EditableDetail) => EditableDetail) =>
    setDetails((p) => p.map((d) => (d._key === key ? updater(d) : d)));

  // A half day is a single date charged at half that day's value; it must carry a morning/afternoon part.
  const asHalf = (d: EditableDetail): EditableDetail => ({
    ...d,
    endDate: d.startDate,
    leaveDays: d.startDate ? calc.dayVal(d.startDate) * 0.5 : undefined,
    halfDayPart: d.halfDayPart || "Morning",
  });

  // Usage change → re-derive the row for the new mode (full days never carry a half-day part).
  const changeUsage = (key: number, leaveUsage: string) =>
    setDetail(key, (d) => {
      const next = { ...d, leaveUsage };
      if (leaveUsage === "HalfDay") return asHalf(next);
      next.halfDayPart = undefined;
      if (next.startDate && next.endDate) next.leaveDays = calc.count(next.startDate, next.endDate, false);
      return next;
    });

  const changeHalfDayPart = (key: number, halfDayPart: string) =>
    setDetail(key, (d) => ({ ...d, halfDayPart }));

  // Start date change → half: re-pin; else recompute days from the range, or (if days were entered
  // first) recompute the end date from the day count.
  const changeStart = (key: number, startDate: string) =>
    setDetail(key, (d) => {
      const next = { ...d, startDate };
      if (d.leaveUsage === "HalfDay") return asHalf(next);
      if (next.endDate) next.leaveDays = startDate ? calc.count(startDate, next.endDate, false) : d.leaveDays;
      else if ((d.leaveDays ?? 0) > 0) next.endDate = startDate ? calc.endFromDays(startDate, d.leaveDays!) : "";
      return next;
    });

  // RULE 1 — End date change → auto-calculate leave days for the range.
  const changeEnd = (key: number, endDate: string) =>
    setDetail(key, (d) => {
      const next = { ...d, endDate };
      if (next.startDate && d.leaveUsage !== "HalfDay") next.leaveDays = calc.count(next.startDate, endDate, false);
      return next;
    });

  // RULE 2 — Leave-days input → auto-calculate the end date (skipping weekends & holidays).
  const changeDays = (key: number, raw: string) =>
    setDetail(key, (d) => {
      const days = raw === "" ? undefined : Number(raw);
      const next = { ...d, leaveDays: Number.isFinite(days) ? days : undefined };
      if (next.startDate && days && days > 0 && d.leaveUsage !== "HalfDay")
        next.endDate = calc.endFromDays(next.startDate, days);
      return next;
    });

  // Once the work-week config / holidays load (or change), refresh each row's day count from its
  // dates. Recomputing days from a fixed start+end is idempotent, so it never fights manual input.
  useEffect(() => {
    if (viewing) return;
    setDetails((prev) =>
      prev.map((d) => {
        if (!d.startDate) return d;
        if (d.leaveUsage === "HalfDay") return asHalf(d);
        if (d.endDate) return { ...d, leaveDays: calc.count(d.startDate, d.endDate, false) };
        return d;
      }),
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [calc, viewing]);

  // RULE 3 — live grand total across all rows for the header summary.
  const totalLeaveDays = useMemo(
    () => Math.round(details.reduce((s, d) => s + (Number(d.leaveDays) || 0), 0) * 100) / 100,
    [details],
  );

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const payload: AnnualLeaveModel = {
      ...meta,
      details: details.map(({ _key, ...d }) => ({
        leaveUsage: d.leaveUsage || "FullDay",
        startDate: d.startDate,
        endDate: d.endDate,
      })),
    };
    const result = await saveAnnualLeave(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["annualLeaves"] });
      setId("");
    }
  };

  const selectedLedger = useMemo(
    () => (balances ?? []).find((b) => b.id === meta.annualLeaveLedgerId),
    [balances, meta.annualLeaveLedgerId],
  );

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
          <h3 className="mb-3 text-sm font-semibold">{t("Annual Leave Request")}</h3>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            {info("Employee", `${record.employeeName ?? ""} (${record.employeeNumber ?? ""})`)}
            {info("Fiscal Year", record.fiscalYearName || "—")}
            {info("Request Date", fmt(record.requestDate))}
            {info(
              "Status",
              <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${leaveStatusTone[record.status || ""] || ""}`}>
                {record.status}
              </span>,
            )}
            {info("Total Leave Days", record.totalLeaveDays)}
            {info("Ledger Available", record.ledgerAvailable)}
            <div className="sm:col-span-3">{info("Remark", record.remark || "—")}</div>
          </div>
        </section>

        <section className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-3 text-sm font-semibold">{t("Leave Lines")}</h3>
          <div className="overflow-x-auto">
            <table className="w-full text-[13px]">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                  <th className="px-4 py-2 font-semibold">{t("Usage")}</th>
                  <th className="px-4 py-2 font-semibold">{t("From")}</th>
                  <th className="px-4 py-2 font-semibold">{t("To")}</th>
                  <th className="px-4 py-2 text-right font-semibold">{t("Days")}</th>
                </tr>
              </thead>
              <tbody>
                {(record.details ?? []).map((d, i) => (
                  <tr key={d.id ?? i} className="border-b border-border/60 hover:bg-secondary/40">
                    <td className="px-4 py-2.5 text-foreground">
                      {optionLabel(annualLeaveUsageOptions, d.leaveUsage) || d.leaveUsage}
                      {d.halfDayPart && ` (${optionLabel(halfDayPartOptions, d.halfDayPart) || d.halfDayPart})`}
                    </td>
                    <td className="px-4 py-2.5 text-foreground">{fmt(d.startDate)}</td>
                    <td className="px-4 py-2.5 text-foreground">{fmt(d.endDate)}</td>
                    <td className="px-4 py-2.5 text-right tabular-nums text-foreground">{d.leaveDays}</td>
                  </tr>
                ))}
                <tr className="bg-secondary/30 font-semibold">
                  <td className="px-4 py-2.5" colSpan={3}>{t("Total")}</td>
                  <td className="px-4 py-2.5 text-right tabular-nums">{record.totalLeaveDays}</td>
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
        <h3 className="mb-3 text-sm font-semibold">{t("Annual Leave Request")}</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            {lockedEmployeeId ? (
              <input className={`${INPUT} opacity-70`} value={lockedEmployeeName ?? ""} disabled readOnly />
            ) : (
              <select className={INPUT} value={meta.employeeId ?? ""} onChange={(e) => onEmployee(e.target.value)} required>
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
          <div>
            <label className={LABEL}>{t("Annual Leave Ledger")} *</label>
            <select
              className={INPUT}
              value={meta.annualLeaveLedgerId ?? ""}
              onChange={(e) => setMetaField("annualLeaveLedgerId", e.target.value)}
              disabled={!meta.employeeId}
              required
            >
              <option value="">{meta.employeeId ? t("Select ledger") : t("Pick an employee first")}</option>
              {(balances ?? []).map((b) => (
                <option key={b.id} value={b.id}>
                  {(b.leaveTypeName || b.leaveTypeCode) ?? ""} · {b.fiscalYearName ?? ""} · {b.available} avail
                </option>
              ))}
            </select>
            {formState?.zodErrors?.annualLeaveLedgerId && (
              <p className="mt-1 text-xs text-error">{formState.zodErrors.annualLeaveLedgerId[0]}</p>
            )}
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Remark")}</label>
            <input className={INPUT} value={meta.remark ?? ""} onChange={(e) => setMetaField("remark", e.target.value)} />
          </div>
        </div>

        <div className="mt-3 flex flex-wrap items-center gap-2">
          {selectedLedger && (
            <span className="rounded-md border border-border/70 bg-secondary/20 px-2.5 py-1 text-xs text-foreground">
              {(selectedLedger.leaveTypeName || selectedLedger.leaveTypeCode) ?? ""} — {selectedLedger.fiscalYearName ?? ""}:{" "}
              <b>{selectedLedger.available}</b> {t("days available")}
            </span>
          )}
          <span className="rounded-md border border-primary/30 bg-primary/10 px-2.5 py-1 text-xs text-foreground">
            {t("Total Leave Days")}: <b className="tabular-nums">{totalLeaveDays}</b>
          </span>
          {selectedLedger && (
            <span
              className={`rounded-md border px-2.5 py-1 text-xs ${
                totalLeaveDays > (selectedLedger.available ?? 0)
                  ? "border-error/40 bg-error/10 text-error"
                  : "border-border/70 bg-secondary/20 text-foreground"
              }`}
            >
              {t("Remaining after")}: <b className="tabular-nums">{Math.round(((selectedLedger.available ?? 0) - totalLeaveDays) * 100) / 100}</b>
            </span>
          )}
        </div>
      </section>

      {/* Detail lines */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("Leave Lines")}</h3>
          <button
            type="button"
            onClick={addDetail}
            className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus className="h-3.5 w-3.5" /> {t("Add Line")}
          </button>
        </div>

        {formState?.zodErrors?.details && (
          <p className="mb-2 text-xs text-error">{formState.zodErrors.details[0]}</p>
        )}

        {details.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No lines yet. Add at least one line.")}</p>
        ) : (
          <div className="space-y-2">
            {details.map((d) => {
              const half = d.leaveUsage === "HalfDay";
              return (
                <div
                  key={d._key}
                  className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[140px_1fr_1fr_110px_auto]"
                >
                  <div>
                    <label className={LABEL}>{t("Usage")}</label>
                    <select
                      className={INPUT}
                      value={d.leaveUsage ?? "FullDay"}
                      onChange={(e) => changeUsage(d._key, e.target.value)}
                    >
                      {annualLeaveUsageOptions.map((o) => (
                        <option key={o.id} value={o.id}>{t(o.name)}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className={LABEL}>{t("Start Date")} *</label>
                    <input
                      type="date"
                      className={INPUT}
                      value={fmt(d.startDate)}
                      onChange={(e) => changeStart(d._key, e.target.value)}
                      required
                    />
                  </div>
                  {half ? (
                    // A half day is a single date, so its End Date slot becomes the Morning/Afternoon picker.
                    <div>
                      <label className={LABEL}>{t("Half Day")} *</label>
                      <select
                        className={INPUT}
                        value={d.halfDayPart ?? "Morning"}
                        onChange={(e) => changeHalfDayPart(d._key, e.target.value)}
                        required
                      >
                        {halfDayPartOptions.map((o) => (
                          <option key={o.id} value={o.id}>{t(o.name)}</option>
                        ))}
                      </select>
                    </div>
                  ) : (
                    <div>
                      <label className={LABEL}>{t("End Date")} *</label>
                      <input
                        type="date"
                        className={INPUT}
                        value={fmt(d.endDate)}
                        min={fmt(d.startDate)}
                        onChange={(e) => changeEnd(d._key, e.target.value)}
                        required
                      />
                    </div>
                  )}
                  <div>
                    <label className={LABEL}>{t("Leave Days")}</label>
                    <input
                      type="number"
                      min={0}
                      step={0.5}
                      inputMode="decimal"
                      className={`${INPUT} ${half ? "opacity-70" : ""} text-right tabular-nums`}
                      value={d.leaveDays ?? ""}
                      disabled={half}
                      title={half ? (t("A half day is always 0.5") ?? "") : (t("Enter days to auto-fill the end date") ?? "")}
                      onChange={(e) => changeDays(d._key, e.target.value)}
                    />
                  </div>
                  <div className="flex items-center pb-1">
                    {details.length > 1 && (
                      <button
                        type="button"
                        onClick={() => removeDetail(d._key)}
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

        {/* RULE 3 — grand total across all rows */}
        <div className="mt-3 flex items-center justify-between rounded-md border border-border bg-secondary/30 px-3 py-2 text-sm">
          <span className="font-medium text-muted">{t("Total Leave Days")}</span>
          <span className="text-base font-bold tabular-nums text-foreground">{totalLeaveDays}</span>
        </div>

        <p className="mt-2 text-xs text-muted">
          {t(
            "Leave days auto-calculate from the active work-week configuration (rest days excluded, half days counted as 0.5) and the holiday calendar. Enter a day count to auto-fill the end date. The server re-validates on submit.",
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

export default memo(AnnualLeaveForm);
