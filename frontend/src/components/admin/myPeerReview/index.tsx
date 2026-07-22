"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Users, Send, CheckCircle2, Star } from "lucide-react";
import { getMyPeerReviews, submitAppraisalPeer } from "@/services/admin/appraisal";
import type { MyPeerReviewModel } from "@/models";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

/** One peer-review card: the appraisee + cycle (never their ratings) plus the peer's own score + comments. */
function PeerReviewCard({ item, onSubmitted }: { item: MyPeerReviewModel; onSubmitted: () => void }) {
  const { t } = useTranslation();
  const submitted = item.status === "Submitted";
  const [score, setScore] = useState<string>(item.score != null ? String(item.score) : "");
  const [comments, setComments] = useState<string>(item.comments ?? "");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState<string>("");

  useEffect(() => {
    setScore(item.score != null ? String(item.score) : "");
    setComments(item.comments ?? "");
  }, [item.id, item.score, item.comments]);

  const submit = async () => {
    setBusy(true);
    setMsg("");
    const res = await submitAppraisalPeer({
      id: item.id,
      score: score === "" ? null : Number(score),
      comments: comments || undefined,
    });
    setBusy(false);
    if (res.status === "success") onSubmitted();
    else setMsg(res.message || "Could not submit.");
  };

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-foreground">{item.employeeName || t("Colleague")}</p>
          <p className="text-xs text-muted">{item.reviewCycleName}</p>
        </div>
        <span className={`shrink-0 rounded-full px-2.5 py-0.5 text-xs font-semibold ${submitted ? "bg-success/15 text-success" : "bg-warning/15 text-warning"}`}>
          {submitted ? t("Submitted") : t("Awaiting your feedback")}
        </span>
      </div>

      <p className="mb-3 flex items-center gap-1.5 rounded-md border border-border/70 bg-secondary/20 px-2.5 py-1.5 text-xs text-muted">
        <Star className="h-3.5 w-3.5 shrink-0" />
        {t("Assess this colleague on teamwork, collaboration and core values. You do not see their self-assessment or manager ratings — your input is independent.")}
      </p>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-[120px_1fr]">
        <div>
          <label className={LABEL}>{t("Score")}</label>
          <input type="number" step="any" className={INPUT} value={score} disabled={submitted} onChange={(e) => setScore(e.target.value)} placeholder="0–5" />
        </div>
        <div>
          <label className={LABEL}>{t("Comments")}</label>
          <textarea className={INPUT} rows={2} value={comments} disabled={submitted} onChange={(e) => setComments(e.target.value)} placeholder={t("Teamwork, leadership, values…") ?? ""} />
        </div>
      </div>

      {msg && <p className="mt-2 text-xs text-error">{msg}</p>}

      {!submitted && (
        <div className="mt-3 flex justify-end">
          <button type="button" disabled={busy} onClick={submit} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <Send className="h-4 w-4" /> {busy ? t("Submitting…") : t("Submit Feedback")}
          </button>
        </div>
      )}
      {submitted && (
        <p className="mt-2 inline-flex items-center gap-1 text-xs text-success">
          <CheckCircle2 className="h-3.5 w-3.5" /> {t("Thank you — your feedback has been recorded.")}
        </p>
      )}
    </div>
  );
}

function MyPeerReviews() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading } = useQuery({ queryKey: ["myPeerReviews"], queryFn: getMyPeerReviews });

  if (isLoading) return <Loading />;

  const items = data ?? [];
  const pending = items.filter((i) => i.status !== "Submitted");
  const done = items.filter((i) => i.status === "Submitted");
  const refresh = () => queryClient.invalidateQueries({ queryKey: ["myPeerReviews"] });

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><Users className="h-5 w-5" /></span>
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("My Peer Reviews")}</h1>
          <p className="text-xs text-muted">{t("360° feedback requests where a colleague has asked for your independent input.")}</p>
        </div>
      </div>

      {items.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("You have no peer-review requests right now.")}
        </p>
      ) : (
        <div className="min-h-0 flex-1 space-y-4 overflow-auto">
          {pending.length > 0 && (
            <div className="space-y-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-muted">{t("Awaiting your feedback")} ({pending.length})</p>
              {pending.map((i) => <PeerReviewCard key={i.id} item={i} onSubmitted={refresh} />)}
            </div>
          )}
          {done.length > 0 && (
            <div className="space-y-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-muted">{t("Submitted")} ({done.length})</p>
              {done.map((i) => <PeerReviewCard key={i.id} item={i} onSubmitted={refresh} />)}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default memo(MyPeerReviews);
