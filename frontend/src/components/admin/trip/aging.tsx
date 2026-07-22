"use client";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Send } from "lucide-react";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import ButtonField from "@/components/ui/buttonField";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { getTripAgingReport, runSettlementReminders } from "@/services/admin/trip";
import { money } from "./shared";

const agingBadge = (days?: number) => ((days ?? 0) > 30 ? "bg-error/15 text-error" : (days ?? 0) > 15 ? "bg-warning/15 text-warning" : "bg-secondary/40 text-foreground");

/** HC263/HC265 — outstanding trip advances aged by days since return, with the reminder sweep. */
function TripAging({ onSelect }: { onSelect: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [msg, setMsg] = useState("");
  const [busy, setBusy] = useState(false);
  const { data: aging } = useQuery({ queryKey: ["tripAging"], queryFn: getTripAgingReport });

  const sendReminders = async () => {
    setBusy(true);
    const r = await runSettlementReminders();
    setBusy(false);
    setMsg(r.message);
    queryClient.invalidateQueries({ queryKey: ["tripAging"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "tripNumber",
          label: "Trip #",
          render: (text: string, r: any) => <button type="button" onClick={() => r.tripId && onSelect(r.tripId)} className="font-semibold">{text}</button>,
        },
        { name: "employeeName", label: "Employee" },
        { name: "tripType", label: "Type", render: (v: string) => t(v ?? "") },
        { name: "endDate", label: "Ended", render: (v: string) => v?.slice(0, 10) },
        { name: "daysOutstanding", label: "Days out", render: (_t: unknown, r: any) => <span className="tabular-nums">{r.daysOutstanding}</span> },
        { name: "bucket", label: "Bucket", render: (text: string, r: any) => <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${agingBadge(r.daysOutstanding)}`}>{text}</span> },
        { name: "advanceAmount", label: "Advance", render: (_t: unknown, r: any) => <span className="tabular-nums">{money(r.advanceAmount)}</span> },
      ] as DataTableColumnModel[],
    [onSelect, t],
  );

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-3">
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex flex-wrap gap-2">
          {(aging?.buckets ?? []).map((b) => (
            <span key={b.bucket} className="rounded-lg border border-border bg-card px-3 py-1.5 text-xs">
              <b className="text-foreground">{b.bucket}</b>: {b.count} · <span className="tabular-nums">{money(b.totalOutstanding)}</span>
            </span>
          ))}
        </div>
        <span className="ml-auto text-sm text-muted">
          {t("Total outstanding")}: <b className="tabular-nums text-foreground">{money(aging?.totalOutstanding)}</b> ({aging?.totalCount ?? 0})
        </span>
        <ButtonField value="Send reminders" variant="outline" icon={<Send size={14} />} disabled={busy} onClick={sendReminders} />
      </div>
      {msg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}
      <DataTableProvider dataTable={{ columns, data: aging?.items ?? [], count: aging?.items?.length ?? 0, pagination: "None", search: "None", key: "tripId" }} />
    </div>
  );
}

export default TripAging;
