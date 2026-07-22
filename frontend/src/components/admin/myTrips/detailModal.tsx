"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Play, Flag, Banknote, Ban } from "lucide-react";
import DialogModal from "@/components/common/dialog";
import InputField from "@/components/ui/inputField";
import DateField from "@/components/ui/dateField";
import ButtonField from "@/components/ui/buttonField";
import Loading from "../../common/loader/loader";
import { getTrip, cancelTrip, startTrip, completeTrip, settleTrip, addTripExpense } from "@/services/admin/trip";
import { tripStatusBadge, DetailSection, TripSummary, ExpenseTable } from "../trip/shared";

const iso = (d: Date) => d.toISOString().slice(0, 10);

/** HC260/HC262/HC264 — the traveller's own trip: summary, expenses, start/complete/settle, cancel. */
function MyTripDetailModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [exp, setExp] = useState({ category: "", amount: "", expenseDate: iso(new Date()) });
  const [busy, setBusy] = useState(false);

  const { data: trip, isLoading } = useQuery({ queryKey: ["myTrip", id], queryFn: () => getTrip(id) });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["myTrips"] });
    queryClient.invalidateQueries({ queryKey: ["myTrip", id] });
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
              <p className="truncate text-sm font-semibold text-foreground">{trip.destination}</p>
              <p className="truncate text-xs text-muted">{t(trip.tripType ?? "")}{trip.purpose ? ` · ${trip.purpose}` : ""}</p>
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

          <div className="flex flex-wrap items-center justify-end gap-2 border-t border-border pt-3">
            {trip.status === "Requested" && <ButtonField value="Cancel request" variant="danger" icon={<Ban size={14} />} disabled={busy} onClick={() => run(() => cancelTrip(id), true)} />}
            {trip.status === "Approved" && <ButtonField value="Start trip" variant="outline" icon={<Play size={14} />} disabled={busy} onClick={() => run(() => startTrip(id))} />}
            {trip.status === "InProgress" && <ButtonField value="Complete trip" variant="outline" icon={<Flag size={14} />} disabled={busy} onClick={() => run(() => completeTrip(id))} />}
            {(trip.status === "Completed" || trip.status === "InProgress") && <ButtonField value="Submit settlement" variant="primary" icon={<Banknote size={14} />} disabled={busy} onClick={() => run(() => settleTrip(id), true)} />}
          </div>
        </div>
      )}
    </DialogModal>
  );
}

export default memo(MyTripDetailModal);
