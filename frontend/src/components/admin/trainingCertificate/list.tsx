"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Trash2, Printer, CalendarClock, AlertTriangle, X } from "lucide-react";
import { getAllTrainingCertificates, getExpiringCertificates, renewCertificate, deleteCertificate } from "@/services/admin/trainingCertificate";
import type { TrainingCertificateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import PrintModal from "./printModal";

interface Props {
  editHandler: (id: string) => void;
}

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");
const isLapsed = (v?: string) => !!v && new Date(v) < new Date();

function TrainingCertificateList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [printFor, setPrintFor] = useState<TrainingCertificateModel | null>(null);
  const [renewFor, setRenewFor] = useState<TrainingCertificateModel | null>(null);
  const [renewDate, setRenewDate] = useState("");
  const [busy, setBusy] = useState(false);
  const [actionMsg, setActionMsg] = useState("");

  const list = useEntityList({
    queryKey: "trainingCertificates",
    fetchPage: getAllTrainingCertificates,
  });

  // HC200 renewal tracking — the sweep is admin-only; a 400 simply hides the banner.
  const { data: expiring } = useQuery({
    queryKey: ["expiringCertificates"],
    queryFn: () => getExpiringCertificates(90),
    retry: false,
  });

  const refresh = (msg: string) => {
    setActionMsg(msg);
    queryClient.invalidateQueries({ queryKey: ["trainingCertificates"] });
    queryClient.invalidateQueries({ queryKey: ["expiringCertificates"] });
  };

  const confirmRenew = async () => {
    if (!renewFor?.id || !renewDate) return;
    setBusy(true);
    const res = await renewCertificate(renewFor.id, renewDate);
    setBusy(false);
    if (res.ok) {
      setRenewFor(null);
      setRenewDate("");
    }
    refresh(res.message);
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: TrainingCertificateModel) => (
            <span>
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">{record.employeeNumber}</span>
            </span>
          ),
        },
        {
          name: "title",
          label: "Certificate",
          render: (text: string, record: TrainingCertificateModel) => (
            <span>
              <span className="block">{text}</span>
              <span className="block text-xs text-muted">{record.certificateNo}</span>
            </span>
          ),
        },
        { name: "courseName", label: "Course", render: (v: string) => v || "External" },
        { name: "issuedOn", label: "Issued", sort: true, render: fmtDate },
        {
          name: "expiresOn",
          label: "Expires",
          render: (v: string) =>
            v ? (
              <span className={`text-xs font-semibold ${isLapsed(v) ? "text-error" : "text-foreground"}`}>{fmtDate(v)}</span>
            ) : (
              <span className="text-xs text-muted">{t("No expiry")}</span>
            ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: TrainingCertificateModel) => (
            <span className="flex items-center gap-1.5">
              <button type="button" title={t("Print certificate")} onClick={() => setPrintFor(record)} className="rounded p-1 text-muted hover:text-primary">
                <Printer size={15} />
              </button>
              <button type="button" title={t("Renew")} onClick={() => { setRenewFor(record); setRenewDate(""); }} className="rounded p-1 text-muted hover:text-success">
                <CalendarClock size={15} />
              </button>
              <button type="button" title={t("Edit")} onClick={() => record.id && editHandler(record.id)} className="rounded p-1 text-muted hover:text-primary">
                <Pencil size={15} />
              </button>
              <button
                type="button"
                title={t("Delete")}
                onClick={async () => {
                  if (!record.id) return;
                  const res = await deleteCertificate(record.id);
                  refresh(res.message);
                }}
                className="rounded p-1 text-muted hover:text-error"
              >
                <Trash2 size={15} />
              </button>
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, t],
  );

  return (
    <div className="space-y-2">
      {(expiring?.length ?? 0) > 0 && (
        <div className="rounded-lg border border-warning/40 bg-warning/10 px-3 py-2">
          <p className="flex items-center gap-1.5 text-xs font-semibold text-warning">
            <AlertTriangle size={13} /> {t("Expiring within 90 days")} ({expiring!.length})
          </p>
          <p className="mt-1 text-xs text-muted">
            {expiring!.slice(0, 5).map((c) => `${c.employeeName}: ${c.title} (${fmtDate(c.expiresOn)})`).join(" · ")}
            {expiring!.length > 5 ? " …" : ""}
          </p>
        </div>
      )}
      {actionMsg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{actionMsg}</p>}
      <EntityListShell listKey="trainingCertificates" listLabel="Certifications" columns={columns} {...list} />
      {printFor && <PrintModal certificate={printFor} onClose={() => setPrintFor(null)} />}
      {renewFor && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-sm rounded-lg border border-border bg-background p-4 shadow-xl">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="text-sm font-semibold text-foreground">{t("Renew certificate")}</h3>
              <button type="button" onClick={() => setRenewFor(null)} className="rounded p-1 text-muted hover:bg-secondary/40"><X size={16} /></button>
            </div>
            <p className="mb-3 text-xs text-muted">{renewFor.employeeName} — {renewFor.title} · {t("current expiry")} {fmtDate(renewFor.expiresOn)}</p>
            <label className="mb-1 block text-xs font-medium text-muted">{t("New expiry date")}</label>
            <input type="date" className={INPUT} value={renewDate} onChange={(e) => setRenewDate(e.target.value)} />
            <div className="mt-4 flex justify-end gap-2">
              <button type="button" onClick={() => setRenewFor(null)} className="rounded-md border border-border px-3 py-1.5 text-sm text-foreground hover:bg-secondary/40">{t("Cancel")}</button>
              <button type="button" disabled={busy || !renewDate} onClick={confirmRenew}
                className="rounded-md bg-primary px-3.5 py-1.5 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
                {busy ? t("Saving…") : t("Renew")}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default TrainingCertificateList;
