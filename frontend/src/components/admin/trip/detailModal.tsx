"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, XCircle, Banknote, Play, Flag, Plus } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import InputField from "@/components/ui/inputField";
import DateField from "@/components/ui/dateField";
import ButtonField from "@/components/ui/buttonField";
import Loading from "../../common/loader/loader";
import {
  getTrip, approveTrip, rejectTrip, disburseTripAdvance, startTrip, completeTrip, settleTrip, addTripExpense,
} from "@/services/admin/trip";
import { tripStatusBadge, DetailSection, TripSummary, ExpenseTable } from "./shared";

const iso = (d: Date) => d.toISOString().slice(0, 10);

/** HC260–268 — HR trip detail + actions: approve/reject, pay advance, start/complete, settle, expenses. */
function TripDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [note, setNote] = useState("");
  const [reason, setReason] = useState("");
  const [advanceRef, setAdvanceRef] = useState("");
  const [settleRef, setSettleRef] = useState("");
  const [exp, setExp] = useState({ category: "", amount: "", expenseDate: iso(new Date()) });
  const [busy, setBusy] = useState(false);

  const { data: trip, isLoading } = useQuery({ queryKey: ["trip", id], queryFn: () => getTrip(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["trips"] });
    queryClient.invalidateQueries({ queryKey: ["tripAging"] });
    queryClient.invalidateQueries({ queryKey: ["trip", id] });
  };
  const run = async (fn: () => Promise<{ ok: boolean; message: string }>, close = false) => {
    setBusy(true);
    const r = await fn();
    setBusy(false);
    if (r.ok) { invalidate(); if (close) onClose(); }
  };

  const canExpense = !!trip && ["Approved", "InProgress", "Completed"].includes(trip.status ?? "");
  const addExpense = () =>
    run(() => addTripExpense({ tripRequestId: id, category: exp.category.trim(), amount: Number(exp.amount), expenseDate: exp.expenseDate }).then((r) => {
      if (r.ok) setExp({ category: "", amount: "", expenseDate: iso(new Date()) });
      return r;
    }));

  return (
    <DialogModal title={trip?.tripNumber ?? t("Trip")} visible onClose={onClose} hideOk cancelLabel="Close">
      {isLoading || !trip ? (
        <Loading />
      ) : (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border pb-3">
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-foreground">{trip.employeeName}</p>
              <p className="truncate text-xs text-muted">{t(trip.tripType ?? "")} · {trip.destination}</p>
            </div>
            <span className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${tripStatusBadge(trip.status)}`}>{t(trip.status ?? "")}</span>
          </div>

          <DetailSection title="Trip Summary">
            <TripSummary trip={trip} />
          </DetailSection>

          <DetailSection title="Expenses">
            <ExpenseTable trip={trip} />
          </DetailSection>

          {canExpense && (
            <DetailSection title="Add Expense">
              <div className="flex flex-wrap items-end gap-2">
                <div className="w-36"><InputField type="text" name="category" label="" placeholder={t("Category") ?? ""} value={exp.category} onChange={(e: any) => setExp((p) => ({ ...p, category: e.target.value }))} /></div>
                <div className="w-28"><InputField type="text" inputType="number" name="amount" label="" placeholder={t("Amount") ?? ""} value={exp.amount} onChange={(e: any) => setExp((p) => ({ ...p, amount: e.target.value }))} /></div>
                <div className="w-40"><DateField type="date" name="expenseDate" label="" value={exp.expenseDate} onChange={(e: any) => setExp((p) => ({ ...p, expenseDate: e.target.value }))} /></div>
                <ButtonField value="Add" variant="outline" icon={<Plus size={14} />} disabled={busy || !exp.category.trim() || exp.amount === ""} onClick={addExpense} />
              </div>
            </DetailSection>
          )}

          {trip.status === "Requested" && (
            <DetailSection title="Approval">
              <div className="space-y-2">
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" name="note" label="" placeholder={t("Approval note (optional)") ?? ""} value={note} onChange={(e: any) => setNote(e.target.value)} /></div>
                  <ButtonField value="Approve" variant="primary" icon={<CheckCircle2 size={15} />} disabled={busy} onClick={() => run(() => approveTrip(id, note || undefined), true)} />
                </div>
                <div className="flex items-end gap-2">
                  <div className="flex-1"><InputField type="text" name="reason" label="" placeholder={t("Rejection reason") ?? ""} value={reason} onChange={(e: any) => setReason(e.target.value)} /></div>
                  <ButtonField value="Reject" variant="danger" icon={<XCircle size={15} />} disabled={busy || !reason.trim()} onClick={() => run(() => rejectTrip(id, reason), true)} />
                </div>
              </div>
            </DetailSection>
          )}

          {(trip.status === "Approved" || trip.status === "InProgress") && !trip.advanceDisbursedAt && (
            <DetailSection title="Advance Payment">
              <div className="flex items-end gap-2">
                <div className="flex-1"><InputField type="text" name="advanceRef" label="" placeholder={t("Advance payment reference (CBS)") ?? ""} value={advanceRef} onChange={(e: any) => setAdvanceRef(e.target.value)} /></div>
                <ButtonField value="Pay advance" variant="primary" icon={<Banknote size={15} />} disabled={busy} onClick={() => run(() => disburseTripAdvance(id, advanceRef || undefined))} />
              </div>
            </DetailSection>
          )}

          {(trip.status === "Completed" || trip.status === "InProgress") && (
            <DetailSection title="Settlement">
              <div className="flex items-end gap-2">
                <div className="flex-1"><InputField type="text" name="settleRef" label="" placeholder={t("Settlement reference") ?? ""} value={settleRef} onChange={(e: any) => setSettleRef(e.target.value)} /></div>
                <ButtonField value="Settle" variant="primary" icon={<Banknote size={15} />} disabled={busy} onClick={() => run(() => settleTrip(id, settleRef || undefined), true)} />
              </div>
            </DetailSection>
          )}

          {(trip.status === "Approved" || trip.status === "InProgress") && (
            <div className="flex flex-wrap items-center justify-end gap-2 border-t border-border pt-3">
              {trip.status === "Approved" && <ButtonField value="Start trip" variant="outline" icon={<Play size={14} />} disabled={busy} onClick={() => run(() => startTrip(id))} />}
              {trip.status === "InProgress" && <ButtonField value="Complete trip" variant="outline" icon={<Flag size={14} />} disabled={busy} onClick={() => run(() => completeTrip(id))} />}
            </div>
          )}
        </div>
      )}
    </DialogModal>
  );
}

export default memo(TripDetailModal);
