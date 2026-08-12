"use client";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save } from "lucide-react";
import type { OtherLeaveModel, OtherLeaveDetailModel } from "@/models";
import {
  getOtherLeave,
  getOtherLeaveBalances,
  getLumpSumEnd,
  submitOtherLeave,
} from "@/services/admin/otherLeave";
import getAllWorkWeekConfiguration from "@/services/admin/workWeekConfiguration/getAll";
import getAllHoliday from "@/services/admin/holiday/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { leaveStatusTone, otherLeaveGenderLabel } from "@/constants/leave";
import { buildWorkValues, buildHolidaySets, countLeaveDays, parseDate } from "@/utils/leaveCalendar";
import ButtonField from "@/components/ui/buttonField";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT =
  "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";
const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

interface EditableDetail extends OtherLeaveDetailModel {
  _key: number;
}

/**
 * Other-leave request (same mechanics as Annual Leave, static allocation): the entitlement dropdown
 * lists the ACTIVE fiscal year's settings the employee's gender qualifies for, with the remaining
 * balance; lump-sum types lock the request to ONE block whose end date the server computes.
 */
function OtherLeaveForm({
  id,
  setId,
  lockedEmployeeId,
  lockedEmployeeName,
}: {
  id: string;
  setId: (id: string) => void;
  lockedEmployeeId?: string;
  lockedEmployeeName?: string;
}) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;
  const viewing = typeof id !== "undefined" && id !== "";

  const [meta, setMeta] = useState<OtherLeaveModel>({});
  const [details, setDetails] = useState<EditableDetail[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["otherLeave", id],
    queryFn: () => getOtherLeave(id),
    enabled: viewing,
  });

  const employeeId = lockedEmployeeId ?? meta.employeeId;
  const { data: balances } = useQuery({
    queryKey: ["otherLeaveBalances", employeeId],
    queryFn: () => getOtherLeaveBalances(employeeId!),
    enabled: !viewing && !!employeeId,
  });
  const selectedBalance = (balances ?? []).find((b) => b.otherLeaveSettingId === meta.otherLeaveSettingId);

  // Live working-day preview (the backend re-applies the same rules authoritatively on submit).
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
  const calc = useMemo(() => {
    const active = (workWeeks?.data ?? []).find((w) => w.isActive) ?? null;
    const workValues = buildWorkValues(active);
    const holidays = buildHolidaySets(holidaysResp?.data ?? []);
    return (s?: string, e?: string) => {
      const sd = parseDate(s ?? "");
      const ed = parseDate(e ?? "");
      return sd && ed ? countLeaveDays(sd, ed, false, workValues, holidays) : 0;
    };
  }, [workWeeks, holidaysResp]);

  // Preview honoring the setting's day-counting rule: CalendarDays charges every day in the
  // range (holidays/weekends included); WorkingDays skips them (the backend mirrors this).
  const countsCalendarDays = selectedBalance?.dayCounting === "CalendarDays";
  const previewDays = (s?: string, e?: string) => {
    if (!countsCalendarDays) return calc(s, e);
    const sd = parseDate(s ?? "");
    const ed = parseDate(e ?? "");
    return sd && ed && ed >= sd ? Math.floor((ed.getTime() - sd.getTime()) / 86_400_000) + 1 : 0;
  };

  useEffect(() => {
    if (!viewing) {
      setMeta(lockedEmployeeId ? { employeeId: lockedEmployeeId } : {});
      setDetails([{ _key: nextKey(), startDate: "", endDate: "" }]);
    }
  }, [viewing, lockedEmployeeId]);

  // Lump-sum: whenever the start date changes, the server computes the block's end date.
  const onStartChange = async (key: number, value: string) => {
    setDetails((p) => p.map((d) => (d._key === key ? { ...d, startDate: value } : d)));
    if (selectedBalance?.isLumpSum && value && employeeId && meta.otherLeaveSettingId) {
      try {
        const end = await getLumpSumEnd(employeeId, meta.otherLeaveSettingId, value);
        setDetails((p) => p.map((d) => (d._key === key ? { ...d, endDate: fmt(end.endDate) } : d)));
      } catch { /* preview only — submit validates authoritatively */ }
    }
  };

  const addDetail = () => setDetails((p) => [...p, { _key: nextKey(), startDate: "", endDate: "" }]);
  const removeDetail = (key: number) => setDetails((p) => p.filter((d) => d._key !== key));

  const totalPreview = details.reduce((sum, d) => sum + previewDays(d.startDate, d.endDate), 0);

  const submitHandler = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const res = await submitOtherLeave({
      employeeId,
      otherLeaveSettingId: meta.otherLeaveSettingId,
      remark: meta.remark || undefined,
      details: details
        .filter((d) => d.startDate && d.endDate)
        .map((d) => ({ startDate: d.startDate, endDate: d.endDate })),
    });
    setFormState(res.ok
      ? { status: "success", message: t("Leave request submitted for approval") }
      : { status: "error", message: res.message, zodErrors: {} });
    setIsSaving(false);
    if (res.ok) {
      queryClient.invalidateQueries({ queryKey: ["otherLeaves"] });
      queryClient.invalidateQueries({ queryKey: ["otherLeaveBalances"] });
      // The record's OWN cache entry, not just the list: without this the detail query
      // ["otherLeave", id] kept the pre-save copy and the client's 30 s staleTime served it to the
      // next Edit WITHOUT refetching -- grid fresh, form stale until a full page reload.
      queryClient.invalidateQueries({ queryKey: ["otherLeave"] });
      setId("");
    }
  };

  // ---- Read-only view of an existing request --------------------------------
  if (viewing) {
    if (isLoading || !record) return <Loading />;
    return (
      <div className="space-y-4 text-foreground">
        <div className="rounded-lg border border-border bg-card p-4">
          <div className="mb-3 flex items-center justify-between">
            <h3 className="text-sm font-semibold">
              {record.leaveName}
              {record.isLumpSum && <span className="ml-2 rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-semibold text-primary">ONE BLOCK</span>}
            </h3>
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${leaveStatusTone[record.status ?? ""] ?? ""}`}>
              {t(record.status ?? "")}
            </span>
          </div>
          <div className="grid grid-cols-1 gap-2 text-sm sm:grid-cols-2">
            <p><span className="text-muted">{t("Employee")}: </span>{record.employeeName} ({record.employeeNumber})</p>
            <p><span className="text-muted">{t("Fiscal Year")}: </span>{record.fiscalYearName}</p>
            <p><span className="text-muted">{t("Requested")}: </span>{fmt(record.requestDate)}</p>
            <p><span className="text-muted">{t("Total Days")}: </span><b className="tabular-nums">{record.totalLeaveDays}</b></p>
            {record.remark && <p className="sm:col-span-2"><span className="text-muted">{t("Remark")}: </span>{record.remark}</p>}
          </div>
        </div>
        <div className="rounded-lg border border-border bg-card p-4">
          <h4 className="mb-2 text-sm font-semibold">{t("Leave Lines")}</h4>
          {(record.details ?? []).map((d) => (
            <p key={d.id} className="border-b border-border/60 py-1.5 text-sm last:border-0">
              {fmt(d.startDate)} → {fmt(d.endDate)} · <b className="tabular-nums">{d.leaveDays}</b> {t("day(s)")}
            </p>
          ))}
        </div>
      </div>
    );
  }

  // ---- New request ----------------------------------------------------------
  return (
    <form onSubmit={submitHandler} className="space-y-4 text-foreground">
      <div className="rounded-lg border border-border bg-card p-4">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            <input className={INPUT} value={lockedEmployeeName ?? ""} readOnly disabled />
          </div>
          <div>
            <label className={LABEL}>{t("Leave Type (active fiscal year)")} *</label>
            <select
              className={INPUT}
              value={meta.otherLeaveSettingId ?? ""}
              onChange={(e) => setMeta((p) => ({ ...p, otherLeaveSettingId: e.target.value }))}
              required
            >
              <option value="">{t("— select —")}</option>
              {(balances ?? []).map((b) => (
                <option key={b.otherLeaveSettingId} value={b.otherLeaveSettingId}>
                  {b.name} — {b.remaining}/{b.allocation}d {t("remaining")}{b.isLumpSum ? ` · ${t("one block")}` : ""}
                </option>
              ))}
            </select>
            {selectedBalance && (
              <p className="mt-1 text-[11px] text-muted">
                {otherLeaveGenderLabel(selectedBalance.gender)} · {selectedBalance.fiscalYearName}
                {selectedBalance.isLumpSum && ` · ${t("taken all at once — the end date is computed for you")}`}
              </p>
            )}
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Remark")}</label>
            <input className={INPUT} value={meta.remark ?? ""} onChange={(e) => setMeta((p) => ({ ...p, remark: e.target.value }))} />
          </div>
        </div>
      </div>

      <div className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <h4 className="text-sm font-semibold">
            {t("Leave Lines")}
            <span className="ml-2 text-xs font-normal text-muted">
              {countsCalendarDays
                ? t("~{{n}} calendar day(s)", { n: totalPreview })
                : t("~{{n}} working day(s)", { n: totalPreview })}
            </span>
          </h4>
          {!selectedBalance?.isLumpSum && (
            <ButtonField value={t("Add Line")} icon={<Plus className="h-4 w-4" />} htmlType="button" variant="outline" onClick={addDetail} />
          )}
        </div>
        <div className="space-y-2">
          {(selectedBalance?.isLumpSum ? details.slice(0, 1) : details).map((d) => (
            <div key={d._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_1fr_90px_auto]">
              <div>
                <label className={LABEL}>{t("From")} *</label>
                <input type="date" className={INPUT} value={d.startDate ?? ""} onChange={(e) => onStartChange(d._key, e.target.value)} required />
              </div>
              <div>
                <label className={LABEL}>{t("To")} *{selectedBalance?.isLumpSum ? ` (${t("computed")})` : ""}</label>
                <input
                  type="date" className={INPUT} value={d.endDate ?? ""}
                  onChange={(e) => setDetails((p) => p.map((x) => (x._key === d._key ? { ...x, endDate: e.target.value } : x)))}
                  readOnly={!!selectedBalance?.isLumpSum}
                  required
                />
              </div>
              <div>
                <label className={LABEL}>{t("Days")}</label>
                <input className={INPUT} value={previewDays(d.startDate, d.endDate) || ""} readOnly disabled />
              </div>
              <div className="pb-1">
                {!selectedBalance?.isLumpSum && details.length > 1 && (
                  <button type="button" onClick={() => removeDetail(d._key)} className="rounded p-1.5 text-error hover:bg-error/10">
                    <Trash2 className="h-4 w-4" />
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <ButtonField
          value={isSaving ? t("Submitting…") : t("Submit for Approval")}
          icon={<Save className="h-4 w-4" />}
          htmlType="submit"
          variant="primary"
          disabled={isSaving}
        />
      </div>
    </form>
  );
}

export default memo(OtherLeaveForm);
