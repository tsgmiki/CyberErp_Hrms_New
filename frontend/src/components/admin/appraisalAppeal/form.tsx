"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Play, Gavel } from "lucide-react";
import { getAppraisalAppeal, startAppraisalAppealReview, resolveAppraisalAppeal } from "@/services/admin/appraisalAppeal";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

function AppraisalAppealForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: appeal, isLoading } = useQuery({
    queryKey: ["appraisalAppeal", id],
    queryFn: () => getAppraisalAppeal(id),
    enabled: id !== "",
  });

  const [upheld, setUpheld] = useState("true");
  const [resolution, setResolution] = useState("");
  const [formState, setFormState] = useState<any>({});
  const [isBusy, setIsBusy] = useState(false);

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["appraisalAppeal", id] });
    queryClient.invalidateQueries({ queryKey: ["appraisalAppeals"] });
  };

  const start = async () => {
    setIsBusy(true);
    const result = await startAppraisalAppealReview(id);
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") refresh();
  };

  const resolve = async () => {
    setIsBusy(true);
    const result = await resolveAppraisalAppeal({ id, upheld: upheld === "true", resolution });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") { setResolution(""); refresh(); }
  };

  if (isLoading || !appeal) return <Loading />;

  const closed = appeal.status === "Resolved" || appeal.status === "Rejected";

  return (
    <div className="space-y-5 p-1 text-foreground">
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-base font-semibold">{appeal.employeeName}</h2>
            {appeal.requestFollowUp && <p className="text-xs text-warning">{t("Follow-up discussion requested")}</p>}
          </div>
          <span className="rounded-full bg-primary/10 px-3 py-1 text-xs font-semibold text-primary">{appeal.status}</span>
        </div>
        <div className="mt-3">
          <label className={LABEL}>{t("Appeal Comments")}</label>
          <p className="whitespace-pre-wrap rounded-md border border-border bg-secondary/20 px-3 py-2 text-sm">{appeal.comments}</p>
        </div>
        {closed && (
          <div className="mt-3">
            <label className={LABEL}>{t("Resolution")} ({appeal.status})</label>
            <p className="whitespace-pre-wrap rounded-md border border-border bg-secondary/20 px-3 py-2 text-sm">{appeal.resolution}</p>
          </div>
        )}
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {appeal.status === "Open" && (
        <div className="flex justify-end">
          <button type="button" disabled={isBusy} onClick={start} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <Play className="h-4 w-4" /> {t("Start Review")}
          </button>
        </div>
      )}

      {appeal.status === "UnderReview" && (
        <section className="rounded-lg border border-border bg-card p-4">
          <h3 className="mb-3 text-sm font-semibold">{t("Resolve Appeal")}</h3>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-[200px_1fr]">
            <div>
              <label className={LABEL}>{t("Decision")}</label>
              <select className={INPUT} value={upheld} onChange={(e) => setUpheld(e.target.value)}>
                <option value="true">{t("Uphold (Resolved)")}</option>
                <option value="false">{t("Reject")}</option>
              </select>
            </div>
            <div>
              <label className={LABEL}>{t("Resolution")} *</label>
              <input className={INPUT} value={resolution} onChange={(e) => setResolution(e.target.value)} placeholder={t("Document the decision") ?? ""} />
            </div>
          </div>
          <div className="mt-3 flex justify-end gap-2">
            <button type="button" onClick={() => setId("")} className="rounded-md border border-border px-4 py-2 text-sm font-semibold hover:bg-secondary/40">{t("Back")}</button>
            <button type="button" disabled={isBusy || !resolution} onClick={resolve} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
              <Gavel className="h-4 w-4" /> {t("Resolve")}
            </button>
          </div>
        </section>
      )}
    </div>
  );
}

export default memo(AppraisalAppealForm);
