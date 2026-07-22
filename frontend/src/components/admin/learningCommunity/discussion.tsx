"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, MessageSquare, Send, Trash2, CornerDownRight } from "lucide-react";
import { getCommunityPosts, createCommunityPost, deleteCommunityPost } from "@/services/admin/learningCommunity";
import type { LearningCommunityModel, CommunityPostModel } from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";

const fmtWhen = (v?: string) => (v ? `${v.slice(0, 10)} ${v.slice(11, 16)}` : "");

/** One topic + its inline replies; members reply, authors/moderators delete. */
function TopicCard({ post, community, onChanged }: {
  post: CommunityPostModel;
  community: LearningCommunityModel;
  onChanged: (msg: string) => void;
}) {
  const { t } = useTranslation();
  const [reply, setReply] = useState("");
  const [showReply, setShowReply] = useState(false);
  const [busy, setBusy] = useState(false);

  const sendReply = async () => {
    if (!post.id || !community.id || !reply.trim()) return;
    setBusy(true);
    const res = await createCommunityPost(community.id, reply.trim(), post.id);
    setBusy(false);
    if (res.ok) {
      setReply("");
      setShowReply(false);
    }
    onChanged(res.ok ? "" : res.message);
  };

  const remove = async (postId?: string) => {
    if (!postId) return;
    setBusy(true);
    const res = await deleteCommunityPost(postId);
    setBusy(false);
    onChanged(res.ok ? "" : res.message);
  };

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-1 flex items-start justify-between gap-2">
        <p className="text-sm font-semibold text-foreground">{post.authorName || t("Colleague")}</p>
        <span className="flex items-center gap-1.5">
          <span className="text-[11px] text-muted">{fmtWhen(post.postedAt)}</span>
          <button type="button" title={t("Delete (author / moderator)")} onClick={() => remove(post.id)} className="rounded p-1 text-muted hover:text-error">
            <Trash2 size={13} />
          </button>
        </span>
      </div>
      <p className="whitespace-pre-wrap text-sm text-foreground">{post.content}</p>

      {(post.replies?.length ?? 0) > 0 && (
        <div className="mt-3 space-y-2 border-l-2 border-border/70 pl-3">
          {post.replies!.map((r) => (
            <div key={r.id} className="rounded-md bg-secondary/10 px-3 py-2">
              <div className="mb-0.5 flex items-start justify-between gap-2">
                <p className="text-xs font-semibold text-foreground">{r.authorName}</p>
                <span className="flex items-center gap-1.5">
                  <span className="text-[11px] text-muted">{fmtWhen(r.postedAt)}</span>
                  <button type="button" title={t("Delete")} onClick={() => remove(r.id)} className="rounded p-0.5 text-muted hover:text-error">
                    <Trash2 size={12} />
                  </button>
                </span>
              </div>
              <p className="whitespace-pre-wrap text-xs text-foreground">{r.content}</p>
            </div>
          ))}
        </div>
      )}

      {community.isMember && (
        <div className="mt-3">
          {!showReply ? (
            <button type="button" onClick={() => setShowReply(true)}
              className="inline-flex items-center gap-1 text-xs font-semibold text-primary hover:opacity-80">
              <CornerDownRight size={12} /> {t("Reply")}
            </button>
          ) : (
            <div className="flex items-end gap-2">
              <input type="text" className={INPUT} placeholder={t("Write a reply…")} value={reply}
                onChange={(e) => setReply(e.target.value)}
                onKeyDown={(e) => { if (e.key === "Enter") sendReply(); }} />
              <button type="button" disabled={busy || !reply.trim()} onClick={sendReply}
                className="rounded-md bg-primary p-2 text-on-accent hover:opacity-90 disabled:opacity-50">
                <Send size={14} />
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

/** The community's discussion feed (HC198): open reading, member-only posting. */
function DiscussionView({ community, onBack }: { community: LearningCommunityModel; onBack: () => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [topic, setTopic] = useState("");
  const [busy, setBusy] = useState(false);
  const [msg, setMsg] = useState("");

  const [param] = useState({ ...parameterInitialData, take: 20 });
  const { data, isLoading } = useQuery({
    queryKey: ["communityPosts", community.id],
    queryFn: () => getCommunityPosts(community.id!, param),
    placeholderData: keepPreviousData,
  });
  const topics = data?.data ?? [];

  const refresh = (message: string) => {
    setMsg(message);
    queryClient.invalidateQueries({ queryKey: ["communityPosts", community.id] });
  };

  const postTopic = async () => {
    if (!community.id || !topic.trim()) return;
    setBusy(true);
    const res = await createCommunityPost(community.id, topic.trim());
    setBusy(false);
    if (res.ok) setTopic("");
    refresh(res.ok ? "" : res.message);
  };

  return (
    <div className="flex h-full min-h-0 flex-col p-3">
      <div className="mb-3 flex items-center gap-2">
        <button type="button" onClick={onBack} className="rounded-md border border-border p-1.5 text-muted hover:bg-secondary/40">
          <ArrowLeft size={15} />
        </button>
        <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><MessageSquare className="h-5 w-5" /></span>
        <div className="min-w-0">
          <h1 className="truncate text-base font-semibold text-foreground">{community.name}</h1>
          <p className="truncate text-xs text-muted">
            {community.description || t("Community discussion")} · {community.memberCount} {t("members")}
          </p>
        </div>
      </div>

      {community.isMember ? (
        <div className="mb-3 flex items-end gap-2">
          <textarea className={INPUT} rows={2} placeholder={t("Share knowledge, ask a question…")}
            value={topic} onChange={(e) => setTopic(e.target.value)} />
          <button type="button" disabled={busy || !topic.trim()} onClick={postTopic}
            className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <Send size={14} /> {t("Post")}
          </button>
        </div>
      ) : (
        <p className="mb-3 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
          {t("You are reading as a guest — join the community to post.")}
        </p>
      )}

      {msg && <p className="mb-2 rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-error">{msg}</p>}

      {isLoading ? (
        <Loading />
      ) : topics.length === 0 ? (
        <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
          {t("No discussions yet — start the first topic.")}
        </p>
      ) : (
        <div className="min-h-0 flex-1 space-y-3 overflow-auto">
          {topics.map((p) => (
            <TopicCard key={p.id} post={p} community={community} onChanged={refresh} />
          ))}
        </div>
      )}
    </div>
  );
}

export default memo(DiscussionView);
